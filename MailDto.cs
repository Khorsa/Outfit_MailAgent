using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MailAgent
{
    internal class MailDto(
        UniqueId id,
        string folderId,
        string subject,
        InternetAddressList from,
        InternetAddressList to,
        DateTimeOffset date,
        string mailRef
        )
    {
        public UniqueId id = id;
        public string folderId = folderId;
        public string subject = subject;
        public InternetAddressList from = from;
        public InternetAddressList to = to;
        public DateTimeOffset date = date;
        public string mailRef = mailRef;
    }
}
