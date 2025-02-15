using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailAgent.Settings
{
    public class MailAgentSettings
    {
        public List<Dictionary<string, string>> accounts = new List<Dictionary<string, string>>();
        public int timeToReconnect = 3600;
        public int checkInterval = 5;

        public List<AccountSettings> GetAccountSettings()
        {
            List < AccountSettings > settings = new List<AccountSettings >();
            foreach (var s in this.accounts)
            {
                settings.Add(AccountSettings.fromArray(s));
            }
            return settings;
        }
    }
}
