// // STR0007SuspiciousRegionsAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System.Collections.Immutable;
using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Structure;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Str0007SuspiciousRegionsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STR0007];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach(var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                var region = trivia.GetStructure() as RegionDirectiveTriviaSyntax;
                var name = string.Empty;

                if (region != null)
                    name = region.EndOfDirectiveToken.LeadingTrivia.ToString().Trim();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    bool isSuspicious = RegionNameUtilities.IsSuspiciousName(name);

                    if (isSuspicious)
                    {
                        var diagnostic =
                            Diagnostic.Create(DiagnosticDescriptors.STR0007, trivia.GetLocation(), name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
