// // STY0006MethodNamingAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using CodeStructure.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Style;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Sty0006MethodNamingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STY0006];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax) context.Node;
        var methodSymbol =
            ModelExtensions.GetDeclaredSymbol(context.SemanticModel, methodDeclaration, context.CancellationToken) as
                IMethodSymbol;

        if (methodSymbol != null)
        {
            if (!HasInteropAttribute(methodSymbol))
            {
                string methodName = methodSymbol.Name;
                bool isValid = IsPascalCase(methodName);

                if (!isValid)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STY0006,
                                                       methodDeclaration.Identifier.GetLocation(),
                                                       methodName
                                                      );
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool HasInteropAttribute(IMethodSymbol methodSymbol)
    {
        var result = false;

        foreach(var attribute in methodSymbol.GetAttributes())
        {
            string attributeName = attribute.AttributeClass?.Name ?? string.Empty;

            if (string.Equals(attributeName, "DllImportAttribute", StringComparison.Ordinal) ||
                string.Equals(attributeName, "LibraryImportAttribute", StringComparison.Ordinal))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    private static bool IsPascalCase(string name)
    {
        var result = false;

        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Contains("_", StringComparison.Ordinal))
                result = false;
            else
            {
                char firstChar = name[index: 0];
                result = char.IsUpper(firstChar);
            }
        }

        return result;
    }
}
