// // RegionNameUtilities.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;

#endregion

namespace CodeStructure.Analyzers.Utilities;

public static class RegionNameUtilities
{
    public static bool IsSuspiciousName(string name)
    {
        var result = false;

        foreach(string suspiciousName in smSuspiciousNames)
        {
            if (name.Contains(suspiciousName, StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    public static bool IsGenericName(string name)
    {
        bool result = smGenericNames.Contains(name);
        return result;
    }

    private static readonly ImmutableHashSet<string> smSuspiciousNames =
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
                                "hack",
                                "workaround",
                                "todo",
                                "fixme",
                                "temp",
                                "temporary",
                                "delete",
                                "remove",
                                "deprecated",
                                "obsolete",
                                "broken",
                                "bug",
                                "ugly",
                                "bad"
                               );

    private static readonly ImmutableHashSet<string> smGenericNames =
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
                                "constructors",
                                "properties",
                                "methods",
                                "fields",
                                "events",
                                "private members",
                                "public members"
                               );
}
