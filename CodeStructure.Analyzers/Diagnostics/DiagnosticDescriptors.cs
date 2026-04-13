// // DiagnosticDescriptors.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using Microsoft.CodeAnalysis;

#endregion

namespace CodeStructure.Analyzers.Diagnostics;

public static class DiagnosticDescriptors
{
    private static string GetHelpLink(string diagnosticId)
    {
        return $"{HelpLinkBase}{diagnosticId}.md";
    }

    private const string HelpLinkBase =
        "https://github.com/jackalope-technologies/CodingStandards/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor STR0001 =
        new DiagnosticDescriptor(DiagnosticIds.STR0001,
                                 "No continue statements",
                                 "Avoid using continue statements; restructure the logic instead",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Error,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0001)
                                );

    public static readonly DiagnosticDescriptor STR0002 =
        new DiagnosticDescriptor(DiagnosticIds.STR0002,
                                 "Multiple return statements",
                                 "Method has multiple return statements; use a single return at the end",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Error,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0002)
                                );

    public static readonly DiagnosticDescriptor STR0003 =
        new DiagnosticDescriptor(DiagnosticIds.STR0003,
                                 "Return in void method",
                                 "Void methods should not use return statements",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0003)
                                );

    public static readonly DiagnosticDescriptor STR0004 =
        new DiagnosticDescriptor(DiagnosticIds.STR0004,
                                 "If-else chains",
                                 "If-else chain has {0} branches; consider using switch expression",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0004)
                                );

    public static readonly DiagnosticDescriptor STR0005 =
        new DiagnosticDescriptor(DiagnosticIds.STR0005,
                                 "Return in nested block",
                                 "Return statement inside {0}; use a result variable instead",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Error,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0005)
                                );

    public static readonly DiagnosticDescriptor STR0006 =
        new DiagnosticDescriptor(DiagnosticIds.STR0006,
                                 "No else if",
                                 "Avoid else-if chains; use switch expressions or pattern matching",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0006)
                                );

    public static readonly DiagnosticDescriptor STR0007 =
        new DiagnosticDescriptor(DiagnosticIds.STR0007,
                                 "Suspicious regions",
                                 "Region '{0}' has a suspicious name",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0007)
                                );

    public static readonly DiagnosticDescriptor STR0008 =
        new DiagnosticDescriptor(DiagnosticIds.STR0008,
                                 "One type per file",
                                 "File contains multiple types; each type should be in its own file",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0008)
                                );

    public static readonly DiagnosticDescriptor STR0009 =
        new DiagnosticDescriptor(DiagnosticIds.STR0009,
                                 "Nesting depth exceeded",
                                 "Nesting depth of {0} exceeds the limit of {1}",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0009)
                                );

    public static readonly DiagnosticDescriptor STR0009Error =
        new DiagnosticDescriptor(DiagnosticIds.STR0009,
                                 "Nesting depth exceeded",
                                 "Nesting depth of {0} exceeds the limit of {1}",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Error,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0009)
                                );

    public static readonly DiagnosticDescriptor STR0010 =
        new DiagnosticDescriptor(DiagnosticIds.STR0010,
                                 "Missing argument validation",
                                 "Public method '{0}' does not validate parameter '{1}'",
                                 DiagnosticCategories.STRUCTURE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STR0010)
                                );

    public static readonly DiagnosticDescriptor STY0001 =
        new DiagnosticDescriptor(DiagnosticIds.STY0001,
                                 "End-of-line comments",
                                 "Move comment to line above",
                                 DiagnosticCategories.STYLE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STY0001)
                                );

    public static readonly DiagnosticDescriptor STY0002 =
        new DiagnosticDescriptor(DiagnosticIds.STY0002,
                                 "Null-forgiving operator",
                                 "Avoid null-forgiving operator; use proper null checks",
                                 DiagnosticCategories.STYLE,
                                 DiagnosticSeverity.Error,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STY0002)
                                );

    public static readonly DiagnosticDescriptor STY0003 =
        new DiagnosticDescriptor(DiagnosticIds.STY0003,
                                 "Avoid dynamic",
                                 "Avoid dynamic keyword; use strong typing",
                                 DiagnosticCategories.STYLE,
                                 DiagnosticSeverity.Error,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STY0003)
                                );

    public static readonly DiagnosticDescriptor STY0004 =
        new DiagnosticDescriptor(DiagnosticIds.STY0004,
                                 "Field naming convention",
                                 "Field '{0}' should be named '{1}'",
                                 DiagnosticCategories.STYLE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STY0004)
                                );

    public static readonly DiagnosticDescriptor STY0005 =
        new DiagnosticDescriptor(DiagnosticIds.STY0005,
                                 "Non-nullable string initialized to null",
                                 "Non-nullable string should not be initialized to null",
                                 DiagnosticCategories.STYLE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STY0005)
                                );

    public static readonly DiagnosticDescriptor STY0006 =
        new DiagnosticDescriptor(DiagnosticIds.STY0006,
                                 "Method naming convention",
                                 "Method '{0}' should use PascalCase naming",
                                 DiagnosticCategories.STYLE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STY0006)
                                );

    public static readonly DiagnosticDescriptor STY0007 =
        new DiagnosticDescriptor(DiagnosticIds.STY0007,
                                 "Region naming convention",
                                 "Region name '{0}' is too generic; use feature-based names",
                                 DiagnosticCategories.STYLE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STY0007)
                                );

    public static readonly DiagnosticDescriptor STY0008 =
        new DiagnosticDescriptor(DiagnosticIds.STY0008,
                                 "Magic strings",
                                 "Magic string '{0}' should be extracted to a named constant or added to string resources if user-facing",
                                 DiagnosticCategories.STYLE,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.STY0008)
                                );

    public static readonly DiagnosticDescriptor NUM0001 =
        new DiagnosticDescriptor(DiagnosticIds.NUM0001,
                                 "Floating-point equality",
                                 "Direct equality comparison of floating-point values is unreliable",
                                 DiagnosticCategories.NUMERIC,
                                 DiagnosticSeverity.Error,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.NUM0001)
                                );

    public static readonly DiagnosticDescriptor NUM0002 =
        new DiagnosticDescriptor(DiagnosticIds.NUM0002,
                                 "Magic numbers",
                                 "Magic number '{0}' should be extracted to a named constant",
                                 DiagnosticCategories.NUMERIC,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.NUM0002)
                                );

    public static readonly DiagnosticDescriptor ENC0001 =
        new DiagnosticDescriptor(DiagnosticIds.ENC0001,
                                 "Avoid hiding with 'new'",
                                 "Member '{0}' hides inherited member; use override if base is virtual",
                                 DiagnosticCategories.ENCAPSULATION,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.ENC0001)
                                );

    public static readonly DiagnosticDescriptor ENC0002 =
        new DiagnosticDescriptor(DiagnosticIds.ENC0002,
                                 "Direct inherited field access",
                                 "Direct access to inherited field '{0}'; use property instead",
                                 DiagnosticCategories.ENCAPSULATION,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.ENC0002)
                                );

    public static readonly DiagnosticDescriptor ENC0003 =
        new DiagnosticDescriptor(DiagnosticIds.ENC0003,
                                 "Public/protected fields",
                                 "Field '{0}' should be converted to a property",
                                 DiagnosticCategories.ENCAPSULATION,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.ENC0003)
                                );

    public static readonly DiagnosticDescriptor ENC0004 =
        new DiagnosticDescriptor(DiagnosticIds.ENC0004,
                                 "Interface I prefix",
                                 "Interface '{0}' should be named 'I{0}'",
                                 DiagnosticCategories.ENCAPSULATION,
                                 DiagnosticSeverity.Warning,
                                 isEnabledByDefault: true,
                                 helpLinkUri: GetHelpLink(DiagnosticIds.ENC0004)
                                );
}
