using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailAgent.Settings;

public class MailAgentSettings
{
    public List<Dictionary<string, string>> Accounts = new List<Dictionary<string, string>>();
    public int TimeToReconnect = 3600;
    public int CheckInterval = 5;

    public List<AccountSettings> GetAccountSettings()
    {
        List < AccountSettings > settings = new List<AccountSettings >();
        foreach (var s in this.Accounts)
        {
            settings.Add(AccountSettings.FromArray(s));
        }
        return settings;
    }
}