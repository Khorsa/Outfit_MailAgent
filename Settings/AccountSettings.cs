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

public class AccountSettings
{
    public string Server { get; set; }
    public string MailServiceRef { get; set; }
    public int Port { get; set; }
    public string Email { get; set; }
    public string EncryptedPassword { get; set; }

    public AccountSettings() {
        Server = "";
        MailServiceRef = "";
        Port = 0;
        Email = "";
        EncryptedPassword = "";
    }

    public string GetPassword()
    {
        if (string.IsNullOrWhiteSpace(EncryptedPassword)) return "";

        try
        {
            return Crypter.Unprotect(EncryptedPassword);
        }
        catch
        {
            // Повреждённый/старый пароль: не роняем настройки, пользователь введёт заново.
            return "";
        }
    }

    public void SetPassword(string password)
    {
        this.EncryptedPassword = Crypter.Protect(password);
    }

    public Dictionary<string, string> ToArray()
    {
        var arr = new Dictionary<string, string>();
        arr["port"] = this.Port.ToString();
        arr["server"] = this.Server;
        arr["email"] = this.Email;
        arr["password"] = this.EncryptedPassword;
        arr["mailref"] = this.MailServiceRef;

        return arr;
    }

    public static AccountSettings FromArray(Dictionary<string, string> arr)
    {
        var accountSettings = new AccountSettings();

        accountSettings.Port = int.TryParse(GetValue(arr, "port"), out var port) ? port : 993;
        accountSettings.Server = GetValue(arr, "server");
        accountSettings.Email = GetValue(arr, "email");
        accountSettings.EncryptedPassword = GetValue(arr, "password");
        accountSettings.MailServiceRef = GetValue(arr, "mailref");

        return accountSettings;
    }

    private static string GetValue(Dictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out var value) ? value ?? "" : "";
    }


}