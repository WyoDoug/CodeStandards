// // FieldNamingUtilities.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using Microsoft.CodeAnalysis;

#endregion

namespace CodeStructure.Analyzers.Utilities;

public static class FieldNamingUtilities
{
    public static string GetExpectedFieldName(IFieldSymbol fieldSymbol)
    {
        string expectedPrefix = GetExpectedPrefix(fieldSymbol);
        string baseName = GetBaseName(fieldSymbol.Name);
        string pascalName = ToPascalCase(baseName);
        string expectedName = string.Concat(expectedPrefix, pascalName);

        return expectedName;
    }

    private static string GetExpectedPrefix(IFieldSymbol fieldSymbol)
    {
        string prefix = PrivateInstancePrefix;

        if (fieldSymbol.DeclaredAccessibility == Accessibility.Public ||
            fieldSymbol.DeclaredAccessibility == Accessibility.Protected ||
            fieldSymbol.DeclaredAccessibility == Accessibility.Internal ||
            fieldSymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal ||
            fieldSymbol.DeclaredAccessibility == Accessibility.ProtectedAndInternal)
            prefix = PublicInstancePrefix;
        else
        {
            if (fieldSymbol.IsStatic)
                prefix = fieldSymbol.IsReadOnly ? PrivateStaticReadonlyPrefix : PrivateStaticPrefix;
        }

        return prefix;
    }

    private static string GetBaseName(string name)
    {
        string baseName = name.TrimStart('_');

        if (StartsWithPrefix(baseName, PrivateStaticReadonlyPrefix))
            baseName = baseName.Substring(PrivateStaticReadonlyPrefix.Length);
        else
        {
            if (StartsWithPrefix(baseName, PrivateStaticPrefix))
                baseName = baseName.Substring(PrivateStaticPrefix.Length);
            else
            {
                if (StartsWithPrefix(baseName, PublicInstancePrefix))
                    baseName = baseName.Substring(PublicInstancePrefix.Length);
                else
                {
                    if (StartsWithPrefix(baseName, PrivateInstancePrefix))
                        baseName = baseName.Substring(PrivateInstancePrefix.Length);
                }
            }
        }

        if (string.IsNullOrWhiteSpace(baseName))
            baseName = name;

        return baseName;
    }

    private static bool StartsWithPrefix(string name, string prefix)
    {
        var result = false;

        if (!string.IsNullOrWhiteSpace(name) && name.StartsWith(prefix, StringComparison.Ordinal))
        {
            if (name.Length > prefix.Length)
            {
                char nextChar = name[prefix.Length];
                result = char.IsUpper(nextChar);
            }
        }

        return result;
    }

    private static string ToPascalCase(string name)
    {
        string result = name;

        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Length == 1)
                result = name.ToUpperInvariant();
            else
            {
                string firstChar = name.Substring(startIndex: 0, length: 1).ToUpperInvariant();
                string rest = name.Substring(startIndex: 1);
                result = string.Concat(firstChar, rest);
            }
        }

        return result;
    }

    private const string PrivateInstancePrefix = "m";
    private const string PrivateStaticPrefix = "ps";
    private const string PrivateStaticReadonlyPrefix = "sm";
    private const string PublicInstancePrefix = "pm";
}
