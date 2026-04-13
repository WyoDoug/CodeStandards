// // AnalyzerUtilities.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

#endregion

namespace CodeStructure.Analyzers.Utilities;

public static class AnalyzerUtilities
{
    /// <summary>
    ///     Detects if the compilation is for a test assembly by checking for
    ///     references to common test frameworks (MSTest, xUnit, NUnit).
    /// </summary>
    public static bool IsTestAssembly(Compilation? compilation)
    {
        if (compilation == null)
            return false;

        // Check for test framework assembly references
        foreach(var reference in compilation.ReferencedAssemblyNames)
        {
            string assemblyName = reference.Name;

            if (smTestFrameworkAssemblies.Any(tf =>
                                                  assemblyName.Equals(tf, StringComparison.OrdinalIgnoreCase)
                                             ))
                return true;
        }

        return false;
    }

    public static bool IsTestMethod(ISymbol? methodSymbol)
    {
        var result = false;

        if (methodSymbol is IMethodSymbol method)
        {
            foreach(var attribute in method.GetAttributes())
            {
                string attributeName = attribute.AttributeClass?.Name ?? string.Empty;

                if (smTestAttributeNames.Contains(attributeName))
                {
                    result = true;
                    break;
                }
            }
        }

        return result;
    }

    public static bool IsNullableReferenceType(ITypeSymbol? typeSymbol)
    {
        var result = false;

        if (typeSymbol != null)
        {
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                result = true;
        }

        return result;
    }

    private static readonly ImmutableHashSet<string> smTestAttributeNames =
        ImmutableHashSet.Create(StringComparer.Ordinal,
                                "TestMethod",
                                "Fact",
                                "Theory",
                                "Test",
                                "TestCase"
                               );

    private static readonly ImmutableArray<string> smTestFrameworkAssemblies =
        [
            // MSTest
            "Microsoft.VisualStudio.TestPlatform.TestFramework",
            "MSTest.TestFramework",
            // xUnit
            "xunit",
            "xunit.core",
            "xunit.assert",
            // NUnit
            "nunit.framework",
            // Other frameworks
            "Fixie",
            "Machine.Specifications",
            "TechTalk.SpecFlow",
            // Benchmark (often similar to test projects)
            "BenchmarkDotNet"
        ];
}
