using OutfitTool.API1;
using OutfitTool.API1.Dto;

namespace MailAgent;

internal class ModuleInfo : IModuleInfo
{
    public string Name => "mail_agent";
    public string DisplayName => "MailAgent";
    public string AssemblyName => "MailAgent";
    public string Description => "Mail Agent";
    public ModuleVersion Version => new (0, 1, "alpha");
    public string Changes => "Первая версия";
    public string Author => "Stolyarov Roman";
    public string AuthorContacts => "rshome@mail.ru";
    public int RequireApiVersion => 1;
}