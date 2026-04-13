// // STR0004IfElseChainsAnalyzer.cs
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
public sealed class Str0004IfElseChainsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STR0004];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax) context.Node;
        bool isTopLevel = ifStatement.Parent is not ElseClauseSyntax;

        if (isTopLevel)
        {
            int branchCount = CountBranches(ifStatement);

            if (branchCount > 3)
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STR0004,
                                                   ifStatement.IfKeyword.GetLocation(),
                                                   branchCount
                                                  );
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static int CountBranches(IfStatementSyntax ifStatement)
    {
        var count = 1;
        var current = ifStatement;
        var elseClause = current.Else;

        while (elseClause != null)
        {
            if (elseClause.Statement is IfStatementSyntax nestedIf)
            {
                count = count + 1;
                current = nestedIf;
                elseClause = current.Else;
            }
            else
            {
                count = count + 1;
                elseClause = null;
            }
        }

        return count;
    }
}
