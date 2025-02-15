using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailAgent.Settings
{
    public class AccountSettings
    {
        public string server { get; set; }
        public string mailServiceRef { get; set; }
        public int port { get; set; }
        public string email { get; set; }
        public string encryptedPassword { get; set; }

        public AccountSettings() {
            server = "";
            mailServiceRef = "";
            port = 0;
            email = "";
            encryptedPassword = "";
        }

        public string getPassword()
        {
            return Crypter.Unprotect(encryptedPassword);
        }

        public void setPassword(string password)
        {
            this.encryptedPassword = Crypter.Protect(password);
        }

        public Dictionary<string, string> toArray()
        {
            var arr = new Dictionary<string, string>();
            arr["port"] = this.port.ToString();
            arr["server"] = this.server;
            arr["email"] = this.email;
            arr["password"] = this.encryptedPassword;
            arr["mailref"] = this.mailServiceRef;

            return arr;
        }

        public static AccountSettings fromArray(Dictionary<string, string> arr)
        {
            var accountSettings = new AccountSettings();

            accountSettings.port = int.Parse(arr["port"]);
            accountSettings.server = arr["server"];
            accountSettings.email = arr["email"];
            accountSettings.encryptedPassword = arr["password"];
            accountSettings.mailServiceRef = arr["mailref"];

            return accountSettings;
        }


    }
}
