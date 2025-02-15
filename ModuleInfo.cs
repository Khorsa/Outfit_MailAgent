using OutfitTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailAgent
{
    internal class ModuleInfo : ModuleInfoInterface
    {
        public string Name => "MailAgent";

        public string Description => "Mail Agent";

        public string Author => "Stolyarov Roman";

        public string AuthorContacts => "rshome@mail.ru";
    }
}
