using OutfitTool.Common;

namespace MailAgent
{
    internal class ModuleInfo : ModuleInfoInterface
    {
        public string Name => "mail_agent";
        public string DisplayName => "MailAgent";
        public string AssemblyName => "MailAgent";
        public string Description => "Mail Agent";
        public ModuleVersion Version => new ModuleVersion(0, 1, "pre-alpha");
        public ModuleVersion Require => new ModuleVersion(3, 0);
        public string Changes => "Первая версия";
        public string Author => "Stolyarov Roman";
        public string AuthorContacts => "rshome@mail.ru";
    }
}
