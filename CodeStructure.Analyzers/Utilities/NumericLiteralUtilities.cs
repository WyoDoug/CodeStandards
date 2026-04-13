// // NumericLiteralUtilities.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace CodeStructure.Analyzers.Utilities;

public static class NumericLiteralUtilities
{
    public static bool IsAllowedInteger(long value)
    {
        bool result = smAllowedIntegers.Contains(value);
        return result;
    }

    public static bool IsAllowedDouble(double value)
    {
        bool result = smAllowedDoubles.Contains(value);
        return result;
    }

    public static bool IsHexLiteral(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        if (literalExpression.Token.Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            result = true;

        return result;
    }

    public static bool IsNumericLiteral(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        var kind = literalExpression.Kind();

        if (kind == SyntaxKind.NumericLiteralExpression)
            result = true;

        return result;
    }

    private static readonly ImmutableHashSet<long> smAllowedIntegers = ImmutableHashSet.Create(-10000L,
             -1000L,
             -100L,
             -10L,
             -9L,
             -8L,
             -7L,
             -6L,
             -5L,
             -4L,
             -3L,
             -2L,
             -1L,
             0L,
             1L,
             2L,
             3L,
             4L,
             5L,
             6L,
             7L,
             8L,
             9L,
             10L,
             11L,
             12L,
             13L,
             14L,
             15L,
             16L,
             17L,
             18L,
             19L,
             20L,
             23L,
             24L,
             25L,
             30L,
             31L,
             32L,
             37L,
             40L,
             45L,
             50L,
             60L,
             64L,
             75L,
             90L,
             100L,
             120L,
             128L,
             135L,
             180L,
             225L,
             255L,
             256L,
             260L,
             270L,
             315L,
             360L,
             365L,
             366L,
             397L,
             500L,
             512L,
             8000L,
             1024L,
             2000L,
             2048L,
             3000L,
             4096L,
             5000L,
             60000L,
             8192L,
             10000L,
             120000L,
             16384L,
             32768L,
             65535L,
             100000L,
             1000000L,
             0x7FFFFFFF
        );

    private static readonly ImmutableHashSet<double> smAllowedDoubles = ImmutableHashSet.Create(0.0,
             1.0,
             -0.5,
             -0.25,
             -0.1,
             -0.01,
             0.01,
             0.02,
             0.05,
             0.1,
             0.2,
             0.25,
             0.33,
             0.5,
             0.67,
             0.75,
             0.8,
             0.85,
             0.9,
             0.95,
             0.99,
             0.999,
             -10.0,
             -9.0,
             -8.0,
             -7.0,
             -6.0,
             -5.0,
             -4.0,
             -3.0,
             -2.0,
             -1.0,
             2.0,
             3.0,
             4.0,
             5.0,
             6.0,
             7.0,
             8.0,
             9.0,
             10.0,
             -1000.0,
             -100.0,
             100.0,
             1000.0,
             45.0,
             60.0,
             90.0,
             180.0,
             270.0,
             360.0,
             1e-10,
             1e-9,
             1e-8,
             1e-7,
             1e-6,
             1e-5,
             1e-4,
             1e-3,
             3.14159,
             3.141592653589793,
             6.283185307179586
        );
}
