using MailAgent.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MailAgent
{
    /// <summary>
    /// Логика взаимодействия для AccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        public AccountSettings Account { get; private set; }

        public AccountWindow()
        {
            Account = new AccountSettings();

            InitializeComponent();
            LocalizationHelper.LanguageChanged += LanguageChanged;
            LanguageChanged(this, EventArgs.Empty);
        }

        public AccountWindow(AccountSettings account) : this()
        {
            Account = account;
            ServerTextBox.Text = account.server;
            PortTextBox.Text = account.port.ToString();
            EmailTextBox.Text = account.email;
            PasswordBox.Password = account.getPassword();
            ServiceLinkTextBox.Text = account.mailServiceRef;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Account.server = ServerTextBox.Text;
            Account.port = int.Parse(PortTextBox.Text);
            Account.email = EmailTextBox.Text;
            Account.mailServiceRef = ServiceLinkTextBox.Text;
            Account.setPassword(PasswordBox.Password);

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void LanguageChanged(Object? sender, EventArgs e)
        {
            ResourceDictionary oldDict = (from d in this.Resources.MergedDictionaries
                                          where d.Source != null && d.Source.OriginalString.Contains("Languages/lang.")
                                          select d).First();
            if (oldDict != null)
            {
                int ind = this.Resources.MergedDictionaries.IndexOf(oldDict);
                this.Resources.MergedDictionaries.Remove(oldDict);
                this.Resources.MergedDictionaries.Insert(ind, LocalizationHelper.CurrentDictionary);
            }
            else
            {
                this.Resources.MergedDictionaries.Add(LocalizationHelper.CurrentDictionary);
            }
        }
    }
}
