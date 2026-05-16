/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using MailKit.Net.Imap;
using MailKit.Security;
using MailKit;
using MailAgent.Settings;

namespace MailAgent;

internal class MailService
{
    private Dictionary<UniqueId, MailDto> _lastMessages = new();
    private readonly Dictionary<string, ImapClient> _clients = new();
    
    private DateTime _lastConnect = DateTime.MinValue;
    
    private readonly object _lockObject = new();
    private bool _isBusy;

    /// <summary>
    /// Проверяет непрочитанные сообщения для указанной учетной записи почты
    /// </summary>
    /// <param name="settings">Настройки учетной записи почты</param>
    /// <param name="timeToReconnect">Время в секундах до принудительного переподключения</param>
    /// <returns>
    /// Словарь непрочитанных сообщений, где ключ - UniqueId письма, значение - DTO с информацией о письме.
    /// Возвращает null если сервис занят другой операцией проверки.
    /// </returns>
    /// <remarks>
    /// Метод выполняет следующие действия:
    /// 1. Проверяет доступность сервиса (не занят ли другой проверкой)
    /// 2. Устанавливает соединение с IMAP сервером (если не установлено)
    /// 3. Проходит по всем пользовательским папкам кроме системных (Черновики, Спам, Отправленные, Корзина, Архив)
    /// 4. Ищет непрочитанные сообщения в каждой папке
    /// 5. Кэширует информацию о письмах для последующих проверок
    /// 6. Принудительно отключается после указанного времени для освобождения ресурсов
    /// </remarks>
    public Dictionary<UniqueId, MailDto>? CheckMail(AccountSettings settings, int timeToReconnect)
    {
        lock (_lockObject)
        {
            if (_isBusy) return null;
            _isBusy = true;
        }
        
        try
        {
            _isBusy = true;
            var email = settings.Email;
            var password = settings.GetPassword();
            var imapServer = settings.Server;
            var port = settings.Port;

            // Идентификатор клиента для того, чтобы не разрывать соединение
            var clientId = imapServer + ":" + port + email;

            if (!_clients.ContainsKey(clientId))
            {
                _clients[clientId] = new ImapClient();
            }
            var client = _clients[clientId];

            if (!client.IsConnected)
            {
                _clients[clientId].CheckCertificateRevocation = false;
                client.Connect(imapServer, port, SecureSocketOptions.SslOnConnect);
                client.Authenticate(email, password);
                _lastConnect = DateTime.Now;
            }

            var unreadMessages = new Dictionary<UniqueId, MailDto>();
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
                        if (!_lastMessages.TryGetValue(uuid, out var value))
                        {
                            var message = folder.GetMessage(uuid);
                            var mailDto = new MailDto(uuid, folder.Id, message.Subject, message.From, message.To,
                                message.Date, settings.MailServiceRef);
                            unreadMessages[uuid] = mailDto;
                        }
                        else
                        {
                            unreadMessages[uuid] = value;
                        }
                    }
                }
            }
            _lastMessages = unreadMessages;

            if (_lastConnect.AddSeconds(timeToReconnect) < DateTime.Now)
            {
                // Время от времени отключаемся
                client.Disconnect(true);
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
}