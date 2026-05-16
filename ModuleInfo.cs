/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using OutfitTool.API1;
using OutfitTool.API1.Dto;

namespace MailAgent;

internal class ModuleInfo : IModuleInfo
{
    public string Name => "mail_agent";
    public string DisplayName => "MailAgent";
    public string AssemblyName => "MailAgent";
    public string Description => "Mail Agent";
    public ModuleVersion Version => new (1, 0, "release");
    public string Changes => "Первая версия";
    public string Author => "Stolyarov Roman";
    public string AuthorContacts => "rshome@mail.ru";
    public int RequireApiVersion => 1;
}