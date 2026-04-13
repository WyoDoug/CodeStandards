// // STR0005ReturnInNestedBlockAnalyzer.cs
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
public sealed class Str0005ReturnInNestedBlockAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STR0005];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);
    }

    private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
    {
        var returnStatement = (ReturnStatementSyntax) context.Node;
        string? containerName = GetContainingBlockName(returnStatement);

        if (!string.IsNullOrWhiteSpace(containerName))
        {
            var diagnostic =
                Diagnostic.Create(DiagnosticDescriptors.STR0005, returnStatement.GetLocation(), containerName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string? GetContainingBlockName(SyntaxNode node)
    {
        string? result = null;
        var current = node.Parent;

        while (current != null && result == null)
        {
            if (current is IfStatementSyntax)
                result = "if statement";
            else
            {
                if (current is ForStatementSyntax)
                    result = "for loop";
                else
                {
                    if (current is ForEachStatementSyntax || current is ForEachVariableStatementSyntax)
                        result = "foreach loop";
                    else
                    {
                        if (current is WhileStatementSyntax)
                            result = "while loop";
                        else
                        {
                            if (current is DoStatementSyntax)
                                result = "do-while loop";
                            else
                            {
                                if (current is SwitchSectionSyntax)
                                    result = "switch section";
                                else
                                {
                                    if (current is TryStatementSyntax)
                                        result = "try statement";
                                    else
                                    {
                                        if (current is CatchClauseSyntax)
                                            result = "catch clause";
                                        else
                                        {
                                            if (current is FinallyClauseSyntax)
                                                result = "finally clause";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            current = current.Parent;
        }

        return result;
    }
}
