// // STY0004FieldNamingAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Style;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Sty0004FieldNamingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STY0004];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax) context.Node;

        foreach(var variable in fieldDeclaration.Declaration.Variables)
        {
            var fieldSymbol =
                ModelExtensions.GetDeclaredSymbol(context.SemanticModel, variable, context.CancellationToken) as
                    IFieldSymbol;

            if (fieldSymbol != null)
            {
                if (!fieldSymbol.IsConst)
                {
                    string expectedName = FieldNamingUtilities.GetExpectedFieldName(fieldSymbol);

                    if (!string.Equals(fieldSymbol.Name, expectedName, StringComparison.Ordinal))
                    {
                        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STY0004,
                                                           variable.Identifier.GetLocation(),
                                                           fieldSymbol.Name,
                                                           expectedName
                                                          );
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
