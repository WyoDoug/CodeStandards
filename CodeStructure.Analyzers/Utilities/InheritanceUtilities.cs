// // InheritanceUtilities.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#endregion

namespace CodeStructure.Analyzers.Utilities;

public static class InheritanceUtilities
{
    public static bool IsWpfType(INamedTypeSymbol? typeSymbol)
    {
        var result = false;

        if (typeSymbol != null)
        {
            var current = typeSymbol;

            while (current != null && !result)
            {
                string fullName = current.ToDisplayString();

                if (smWpfBaseTypeNames.Contains(fullName))
                    result = true;
                else
                    current = current.BaseType;
            }
        }

        return result;
    }

    public static bool HasStructLayoutAttribute(INamedTypeSymbol? typeSymbol)
    {
        var result = false;

        if (typeSymbol != null)
        {
            foreach(var attribute in typeSymbol.GetAttributes())
            {
                string attributeName = attribute.AttributeClass?.ToDisplayString() ?? string.Empty;

                if (string.Equals(attributeName,
                                  "System.Runtime.InteropServices.StructLayoutAttribute",
                                  StringComparison.Ordinal
                                 ))
                {
                    result = true;
                    break;
                }
            }
        }

        return result;
    }

    private static readonly ImmutableHashSet<string> smWpfBaseTypeNames =
        ImmutableHashSet.Create(StringComparer.Ordinal,
                                "System.Windows.DependencyObject",
                                "System.Windows.FrameworkElement",
                                "System.Windows.UIElement",
                                "System.Windows.Controls.Control"
                               );
}
