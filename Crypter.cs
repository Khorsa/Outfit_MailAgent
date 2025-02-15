using Org.BouncyCastle.Asn1.Pkcs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MailAgent
{
    internal class Crypter
    {
        public static string Protect(string rawData)
        {
            byte[] data = Encoding.UTF8.GetBytes(rawData);

            byte[] s_aditionalEntropy = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"));
            byte[] crypted = ProtectedData.Protect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser);

            string base64String = Convert.ToBase64String(crypted);


            return base64String;
        }

        public static string Unprotect(string cryptedData)
        {
            byte[] data = Convert.FromBase64String(cryptedData);

            byte[] s_aditionalEntropy = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"));

            byte[] res = ProtectedData.Unprotect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser);


            return Encoding.UTF8.GetString(res);
        }

    }
}
