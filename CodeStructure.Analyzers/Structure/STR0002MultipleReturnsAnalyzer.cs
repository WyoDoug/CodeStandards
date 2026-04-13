// // STR0002MultipleReturnsAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System.Collections.Immutable;
using System.Linq;
using CodeStructure.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Structure;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Str0002MultipleReturnsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STR0002];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax) context.Node;
        ReportMultipleReturns(context, methodDeclaration.Body);
    }

    private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        var localFunction = (LocalFunctionStatementSyntax) context.Node;
        ReportMultipleReturns(context, localFunction.Body);
    }

    private static void ReportMultipleReturns(SyntaxNodeAnalysisContext context, BlockSyntax? body)
    {
        if (body != null)
        {
            var returns = body.DescendantNodes().OfType<ReturnStatementSyntax>().ToList();

            if (returns.Count > 1)
            {
                returns.Sort((left, right) => left.SpanStart.CompareTo(right.SpanStart));

                for(var index = 0; index < returns.Count - 1; index++)
                {
                    var returnStatement = returns[index];
                    var diagnostic =
                        Diagnostic.Create(DiagnosticDescriptors.STR0002, returnStatement.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
