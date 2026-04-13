// // STR0006NoElseIfAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System.Collections.Immutable;
using CodeStructure.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Structure;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Str0006NoElseIfAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STR0006];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeElseClause, SyntaxKind.ElseClause);
    }

    private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
    {
        var elseClause = (ElseClauseSyntax) context.Node;

        if (elseClause.Statement is IfStatementSyntax)
        {
            var diagnostic =
                Diagnostic.Create(DiagnosticDescriptors.STR0006, elseClause.ElseKeyword.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
