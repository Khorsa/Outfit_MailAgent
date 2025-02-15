using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Windows;
using System.Text.RegularExpressions;
using MailKit.Net.Imap;
using MailKit.Security;
using MailKit;
using MimeKit;
using System.Security.Cryptography;
using MailAgent.Settings;

namespace MailAgent
{
    internal delegate void NewMailEventHandler(Dictionary<UniqueId, MailDto> unreadMessages);

    internal class MailService
    {
        public Dictionary<UniqueId, MailDto> lastMessages = new Dictionary<UniqueId, MailDto>();
        private Dictionary<string, ImapClient> clients = new System.Collections.Generic.Dictionary<string, ImapClient>();
        private DateTime lastConnect = DateTime.MinValue;
        public bool isBusy = false;


        public Dictionary<UniqueId, MailDto>? checkMail(AccountSettings settings, int timeToReconnect)
        {
            if (isBusy)
            {
                return null;
            }
            try
            {
                isBusy = true;
                string email = settings.email;
                string password = settings.getPassword();
                string imapServer = settings.server;
                int port = settings.port;

                // Идентификатор клиента для того, чтобы не разрывать соединение
                string clientId = imapServer + ":" + port + email;

                if (!this.clients.ContainsKey(clientId))
                {
                    this.clients[clientId] = new ImapClient();
                }
                var client = this.clients[clientId];

                if (!client.IsConnected)
                {
                    client.Connect(imapServer, port, SecureSocketOptions.SslOnConnect);
                    client.Authenticate(email, password);
                    this.lastConnect = DateTime.Now;
                }

                Dictionary<UniqueId, MailDto> unreadMessages = new Dictionary<UniqueId, MailDto>();
                Dictionary<UniqueId, MailDto> newUnreadMessages = new Dictionary<UniqueId, MailDto>();

                foreach (var ns in client.PersonalNamespaces)
                {
                    foreach (var folder in client.GetFolders(ns))
                    {
                        if (
                            folder.Attributes.HasFlag(MailKit.FolderAttributes.Drafts)
                            || folder.Attributes.HasFlag(MailKit.FolderAttributes.Junk)
                            || folder.Attributes.HasFlag(MailKit.FolderAttributes.Sent)
                            || folder.Attributes.HasFlag(MailKit.FolderAttributes.Trash)
                            || folder.Attributes.HasFlag(MailKit.FolderAttributes.Archive)
                            )
                        {
                            continue;
                        }

                        folder.Open(FolderAccess.ReadOnly);
                        
                        var uuids = folder.Search(MailKit.Search.SearchQuery.NotSeen);


                        foreach (var uuid in uuids)
                        {
                            if (lastMessages.ContainsKey(uuid))
                            {
                                unreadMessages[uuid] = lastMessages[uuid];
                            }
                            else
                            {
                                MimeMessage message = folder.GetMessage(uuid);
                                MailDto mailDto = new MailDto(uuid, folder.Id, message.Subject, message.From, message.To, message.Date, settings.mailServiceRef);
                                unreadMessages[uuid] = mailDto;
                                newUnreadMessages[uuid] = mailDto;
                            }
                        }
                    }
                }

                lastMessages = unreadMessages;

                // Время от времени отключаемся
                if (this.lastConnect.AddSeconds(timeToReconnect) < DateTime.Now)
                {
                    client.Disconnect(true);
                }

                return unreadMessages;
            }
            finally
            {
                isBusy = false;
            }
        }
    }
}
