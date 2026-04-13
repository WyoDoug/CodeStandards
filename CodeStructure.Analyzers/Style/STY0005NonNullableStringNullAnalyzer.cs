// // STY0005NonNullableStringNullAnalyzer.cs
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

namespace CodeStructure.Analyzers.Style;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Sty0005NonNullableStringNullAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STY0005];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclarator, SyntaxKind.VariableDeclarator);
    }

    private static void AnalyzeVariableDeclarator(SyntaxNodeAnalysisContext context)
    {
        var variableDeclarator = (VariableDeclaratorSyntax) context.Node;

        if (variableDeclarator.Initializer != null &&
            variableDeclarator.Initializer.Value.IsKind(SyntaxKind.NullLiteralExpression))
        {
            var typeSyntax = GetDeclaredType(variableDeclarator);

            if (typeSyntax != null)
            {
                bool isNonNullableString = IsNonNullableString(typeSyntax);

                if (isNonNullableString)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STY0005,
                                                       variableDeclarator.Initializer.Value.GetLocation()
                                                      );
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static TypeSyntax? GetDeclaredType(VariableDeclaratorSyntax declarator)
    {
        TypeSyntax? result = null;

        if (declarator.Parent is VariableDeclarationSyntax declaration)
            result = declaration.Type;

        return result;
    }

    private static bool IsNonNullableString(TypeSyntax typeSyntax)
    {
        var result = false;

        if (typeSyntax is PredefinedTypeSyntax predefinedType)
        {
            if (predefinedType.Keyword.IsKind(SyntaxKind.StringKeyword))
                result = true;
        }
        else
        {
            if (typeSyntax is NullableTypeSyntax nullableType)
            {
                if (nullableType.ElementType is PredefinedTypeSyntax nullablePredefined)
                {
                    if (nullablePredefined.Keyword.IsKind(SyntaxKind.StringKeyword))
                        result = false;
                }
            }
        }

        return result;
    }
}
