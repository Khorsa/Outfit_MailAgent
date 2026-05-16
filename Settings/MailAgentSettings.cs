/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
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