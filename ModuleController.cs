using OutfitTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MailKit;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Reflection;
using MailAgent.Settings;
using Common.Logger;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace MailAgent
{
    internal class ModuleController : ModuleControllerInterface
    {
        private static System.Timers.Timer _timer;

        private Dictionary<UniqueId, MailDto> unreadMessages = new Dictionary<UniqueId, MailDto>();
        private Dictionary<UniqueId, MailDto> notificatedMessages = new Dictionary<UniqueId, MailDto>();

        private MailService mailService;
        private SettingsManager<MailAgentSettings> settingManager;
        private Logger logger;

        public ModuleController()
        {
            this.mailService = new MailService();
            this.settingManager = new SettingsManager<MailAgentSettings>();
            this.logger = new Logger();
        }

        public List<CommandInterface> getCommandList()
        {
            return new List<CommandInterface>();
        }

        public void init()
        {
            var settings = settingManager.LoadSettings();

            _timer = new System.Timers.Timer(settings.checkInterval * 1000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        public void setLanguage(string language)
        {
            LocalizationHelper.Language = new CultureInfo(language);
        }

        public void shutdown()
        {
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                var settings = settingManager.LoadSettings();
                var tempUnreadMessages = new Dictionary<UniqueId, MailDto>();
                foreach (var accountSettings in settings.GetAccountSettings())
                {
                    try
                    {
                        // Здесь заполняется tempUnreadMessages
                        var unreadAccountMessages = mailService.checkMail(accountSettings, settings.timeToReconnect);
                        if (unreadAccountMessages != null)
                        {
                            foreach (var unreadMessage in unreadAccountMessages)
                            {
                                tempUnreadMessages.Add(unreadMessage.Key, unreadMessage.Value);
                            }
                        }
                    }
                    catch (Exception ex) {
                        logger.Error(ex.Message);
                        Debug.WriteLine(ex.Message);
                    }
                }

                // Здесь переносим сообщения в unreadMessages (для обеспечения атомарности операции)
                this.unreadMessages = tempUnreadMessages;


            }
            catch (Exception ex) {
                this.logger.Error(ex.ToString());
            }
        }

        public BitmapImage? getTaskbarIcon()
        {
            if (this.unreadMessages.Count == 0)
            {
                return null;
            }
            return new BitmapImage(new Uri("pack://application:,,,/MailAgent;component/Resources/mail.ico"));
        }

        private int counter = 0;

        public string? getTaskbarIconText()
        {
            if (this.unreadMessages.Count == 0)
            {
                return null;
            }
            return "непрочитанных - " + this.unreadMessages.Count;
        }

        public Notification? popNotification()
        {
            Dictionary<UniqueId, MailDto> newUnreadMessages = new Dictionary<UniqueId, MailDto>();
            foreach (var unreadMessage in this.unreadMessages)
            {
                if (!this.notificatedMessages.ContainsKey(unreadMessage.Key))
                {
                    newUnreadMessages[unreadMessage.Key] = unreadMessage.Value;
                }
            }

            Notification? notification = null;

            if (newUnreadMessages.Count == 1)
            {
                var last = newUnreadMessages.Last().Value;
                notification = new Notification(last.to + ": <" + last.from + ">", last.subject, last.mailRef);
            }
            if (newUnreadMessages.Count > 1)
            {
                var last = newUnreadMessages.Last().Value;
                notification = new Notification(last.to + ":", "У вас " + newUnreadMessages.Count + " " + getMessageEnding(newUnreadMessages.Count), last.mailRef);
            }

            this.notificatedMessages = new Dictionary<UniqueId, MailDto>();
            foreach (var unreadMessage in this.unreadMessages)
            {
                this.notificatedMessages[unreadMessage.Key] = unreadMessage.Value;
            }

            return notification;
        }

        string getMessageEnding(int count)
        {
            if (count % 10 == 1 && count % 100 != 11)
                return "новое сообщение";
            else if ((count % 10 >= 2 && count % 10 <= 4) && (count % 100 < 12 || count % 100 > 14))
                return "новых сообщения";
            else
                return "новых сообщений";
        }

        public void openSettings()
        {
            var settings = settingManager.LoadSettings();
            SettingsForm form = new SettingsForm(settings);
            if (form.ShowDialog() == true){
            var settingManager = new SettingsManager<MailAgentSettings>();
                settingManager.SaveSettings(settings);
            }
        }
    }
}
