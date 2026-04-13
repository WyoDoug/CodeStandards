// // STR0009NestingDepthAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System.Collections.Immutable;
using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Structure;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Str0009NestingDepthAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STR0009, DiagnosticDescriptors.STR0009Error];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var walker = new NestingDepthWalker();
        walker.Visit(root);

        foreach(var result in walker.Results)
        {
            if (result.Depth > ErrorLimit)
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STR0009Error,
                                                   result.Node.GetLocation(),
                                                   result.Depth,
                                                   ErrorLimit
                                                  );
                context.ReportDiagnostic(diagnostic);
            }
            else
            {
                if (result.Depth > WarningLimit)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STR0009,
                                                       result.Node.GetLocation(),
                                                       result.Depth,
                                                       WarningLimit
                                                      );
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private const int WarningLimit = 3;
    private const int ErrorLimit = 6;
}
