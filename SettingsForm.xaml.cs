using MailAgent.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class SettingsForm : Window
    {
        public ObservableCollection<AccountSettings> Accounts { get; set; }
        public readonly MailAgentSettings settings;

        public SettingsForm(MailAgentSettings settings)
        {
            this.settings = settings;
            InitializeComponent();
            Accounts = new ObservableCollection<AccountSettings>();

            LocalizationHelper.LanguageChanged += LanguageChanged;
            LanguageChanged(this, EventArgs.Empty);

            foreach (var account in this.settings.GetAccountSettings())
            {
                Accounts.Add(account);
            }
            AccountsListView.ItemsSource = Accounts;
            MailCheckIntervalSlider.Value = settings.checkInterval;
            ReconnectIntervalSlider.Value = settings.timeToReconnect;
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            var accountWindow = new AccountWindow();
            if (accountWindow.ShowDialog() == true)
            {
                Accounts.Add(accountWindow.Account);
            }
        }

        private void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsListView.SelectedItem is AccountSettings selectedAccount)
            {
                var accountWindow = new AccountWindow(selectedAccount);
                if (accountWindow.ShowDialog() == true)
                {
                    var index = Accounts.IndexOf(selectedAccount);
                    Accounts[index] = accountWindow.Account;
                }
            }
            else
            {
                MessageBox.Show(LocalizationHelper.GetString("SelectAccountForEdit"));
            }
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsListView.SelectedItem is AccountSettings selectedAccount)
            {
                Accounts.Remove(selectedAccount);
            }
            else
            {
                MessageBox.Show(LocalizationHelper.GetString("SelectAccountForDelete"));
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            // Логика применения настроек
            this.settings.checkInterval = int.Parse(MailCheckIntervalValue.Text);
            this.settings.timeToReconnect= int.Parse(ReconnectIntervalValue.Text);

            this.settings.accounts = new List<Dictionary<string, string>>();
            foreach (AccountSettings accountItem in AccountsListView.Items) {
                this.settings.accounts.Add(accountItem.toArray());
            }
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
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
