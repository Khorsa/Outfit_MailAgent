/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using System.Security.Cryptography;
using System.Text;

namespace MailAgent;

internal static class Crypter
{
    public static string Protect(string rawData)
    {
        var data = Encoding.UTF8.GetBytes(rawData);
        var sAdditionalEntropy = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? string.Empty);
        var crypted = ProtectedData.Protect(data, sAdditionalEntropy, DataProtectionScope.CurrentUser);
        var base64String = Convert.ToBase64String(crypted);
        return base64String;
    }

    public static string Unprotect(string cryptedData)
    {
        var data = Convert.FromBase64String(cryptedData);
        var sAdditionalEntropy = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? string.Empty);
        var res = ProtectedData.Unprotect(data, sAdditionalEntropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(res);
    }
}