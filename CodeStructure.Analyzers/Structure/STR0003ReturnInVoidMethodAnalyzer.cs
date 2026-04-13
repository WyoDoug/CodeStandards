// // STR0003ReturnInVoidMethodAnalyzer.cs
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
public sealed class Str0003ReturnInVoidMethodAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STR0003];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);
    }

    private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
    {
        var returnStatement = (ReturnStatementSyntax) context.Node;

        if (returnStatement.Expression == null)
        {
            bool isVoidMethod = IsInVoidMethod(returnStatement, context.SemanticModel);

            if (isVoidMethod)
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STR0003, returnStatement.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsInVoidMethod(SyntaxNode node, SemanticModel semanticModel)
    {
        var result = false;

        var current = node;

        while (current != null && !result)
        {
            if (current is MethodDeclarationSyntax methodDeclaration)
            {
                var methodSymbol =
                    ModelExtensions.GetDeclaredSymbol(semanticModel, methodDeclaration) as IMethodSymbol;

                if (methodSymbol != null && methodSymbol.ReturnsVoid)
                    result = true;
            }
            else
            {
                if (current is LocalFunctionStatementSyntax localFunction)
                {
                    var localSymbol = semanticModel.GetDeclaredSymbol(localFunction);

                    if (localSymbol != null && localSymbol.ReturnsVoid)
                        result = true;
                }
            }

            current = current.Parent;
        }

        return result;
    }
}
