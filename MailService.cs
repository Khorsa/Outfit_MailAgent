/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MailKit.Net.Imap;
using MailKit.Security;
using MailKit;
using MailAgent.Settings;
using OutfitTool.API1.Logger;

namespace MailAgent;

internal class MailService
{
    private readonly Dictionary<string, MailDto> _lastMessages = new();
    private readonly Dictionary<string, ImapClient> _clients = new();
    private readonly Dictionary<string, DateTime> _connectedAt = new();
    
    private readonly object _lockObject = new();
    private bool _isBusy;

    public ILogger? Logger { get; set; }

    /// <summary>
    /// Проверяет непрочитанные сообщения для указанной учетной записи почты.
    /// </summary>
    public List<MailDto>? CheckMail(AccountSettings settings, int timeToReconnect)
    {
        lock (_lockObject)
        {
            if (_isBusy) return null;
            _isBusy = true;
        }
        
        try
        {
            var email = settings.Email;
            var imapServer = settings.Server;
            var port = settings.Port;

            var clientId = imapServer + ":" + port + email;

            if (!_clients.ContainsKey(clientId))
            {
                _clients[clientId] = CreateClient(settings);
                _connectedAt[clientId] = DateTime.UtcNow;
            }
            
            if (!_clients[clientId].IsConnected)
            {
                DestroyClient(clientId);
                _clients[clientId] = CreateClient(settings);
                _connectedAt[clientId] = DateTime.UtcNow;
            }
            else if (timeToReconnect > 0
                     && _connectedAt.TryGetValue(clientId, out var connectedAt)
                     && DateTime.UtcNow - connectedAt > TimeSpan.FromSeconds(timeToReconnect))
            {
                // Периодически переподключаемся, чтобы не держать «залипшее» соединение.
                DestroyClient(clientId);
                _clients[clientId] = CreateClient(settings);
                _connectedAt[clientId] = DateTime.UtcNow;
            }

            try
            {
                _clients[clientId].NoOp();
            }
            catch
            {
                DestroyClient(clientId);
                _clients[clientId] = CreateClient(settings);
                _connectedAt[clientId] = DateTime.UtcNow;
            }
            
            var client = _clients[clientId];

            var unreadMessages = new List<MailDto>();
            foreach (var ns in client.PersonalNamespaces)
            {
                foreach (var folder in client.GetFolders(ns))
                {
                    if (
                        folder.Attributes.HasFlag(FolderAttributes.Drafts)
                        || folder.Attributes.HasFlag(FolderAttributes.Junk)
                        || folder.Attributes.HasFlag(FolderAttributes.Sent)
                        || folder.Attributes.HasFlag(FolderAttributes.Trash)
                        || folder.Attributes.HasFlag(FolderAttributes.Archive)
                    )
                    {
                        continue;
                    }

                    folder.Open(FolderAccess.ReadOnly);
                    var uuids = folder.Search(MailKit.Search.SearchQuery.NotSeen);

                    foreach (var uuid in uuids)
                    {
                        var cacheKey = $"{email}|{folder.FullName}|{uuid}";
                        if (!_lastMessages.TryGetValue(cacheKey, out var value))
                        {
                            var message = folder.GetMessage(uuid);
                            value = new MailDto(uuid, email, folder.FullName, message.Subject ?? "", message.From,
                                message.To, message.Date, settings.MailServiceRef);
                            _lastMessages[cacheKey] = value;
                        }

                        unreadMessages.Add(value);
                    }
                }
            }
            return unreadMessages;
        }
        finally
        {
            lock (_lockObject)
            {
                _isBusy = false;
            }
        }
    }

    private void DestroyClient(string clientKey)
    {
        try
        {
            _clients[clientKey]?.Disconnect(true);
            _clients.Remove(clientKey);
        }
        catch
        {
            // ignored
        }
        finally
        {
            _connectedAt.Remove(clientKey);
        }
    }

    public void Dispose()
    {
        foreach (var clientKey in _clients.Keys.ToList())
        {
            DestroyClient(clientKey);
        }

        _lastMessages.Clear();
    }

    private ImapClient CreateClient(AccountSettings account)
    {
        var client = new ImapClient();
        client.Timeout = 5000;
        client.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        client.CheckCertificateRevocation = false;
        client.ServerCertificateValidationCallback = MyCertificateValidationCallback;

        try
        {
            if (account.Port == 993) client.Connect(account.Server, account.Port, SecureSocketOptions.SslOnConnect);
            else client.Connect(account.Server, account.Port, SecureSocketOptions.StartTls);
        }
        catch (Exception e)
        {
            WriteLine(e.Message);
            WriteLine(e.StackTrace ?? "");
            throw;
        }
        client.Authenticate(account.Email, account.GetPassword());
        
        return client;
    }
    
    
    bool MyCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        var host = sender as string ?? "unknown";
        
        WriteLine($"--- Проверка сертификата для {host} ---");

        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        if (certificate is X509Certificate2 certificate2)
        {
            WriteLine($"Издатель (Issuer): {certificate2.Issuer}");
            WriteLine($"Кому выдан (Subject): {certificate2.Subject}");
            WriteLine($"Простое имя: {certificate2.GetNameInfo(X509NameType.SimpleName, false)}");
            WriteLine($"Отпечаток (Thumbprint): {certificate2.Thumbprint}");
            WriteLine($"Действителен с: {certificate2.NotBefore} по {certificate2.NotAfter}");
        }
        else
        {
            WriteLine($"Субъект сертификата (Subject): {certificate?.Subject}");
        }

        WriteLine($"Обнаружены ошибки проверки: {sslPolicyErrors}");

        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
        {
            WriteLine("Ошибка: Сертификат сервера недоступен.");
        }
        
        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
        {
            WriteLine("Ошибка: Имя в сертификате не совпадает с адресом хоста.");
        }

        if (chain != null)
        {
            WriteLine("Детали цепочки сертификатов:");
            foreach (var element in chain.ChainElements)
            {
                WriteLine($"  Сертификат: {element.Certificate.Subject}");
                foreach (var status in element.ChainElementStatus)
                {
                    WriteLine($"    Статус: {status.StatusInformation} (Код: {status.Status})");
                }
            }
        }

        WriteLine("--- Конец проверки ---");

        // Сертификат с ошибкой не принимаем; подробности уже в логе.
        return false; 
    }

    private void WriteLine(string text)
    {
        Logger?.Warning(text);
    }
}