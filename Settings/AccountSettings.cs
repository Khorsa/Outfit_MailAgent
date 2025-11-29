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
        return Crypter.Unprotect(EncryptedPassword);
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

        accountSettings.Port = int.Parse(arr["Port"]);
        accountSettings.Server = arr["Server"];
        accountSettings.Email = arr["Email"];
        accountSettings.EncryptedPassword = arr["Password"];
        accountSettings.MailServiceRef = arr["Mailref"];

        return accountSettings;
    }


}