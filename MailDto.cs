using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MailAgent;

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
    public UniqueId Id = id;
    public string FolderId = folderId;
    public string Subject = subject;
    public InternetAddressList From = from;
    public InternetAddressList To = to;
    public DateTimeOffset Date = date;
    public string MailRef = mailRef;
}