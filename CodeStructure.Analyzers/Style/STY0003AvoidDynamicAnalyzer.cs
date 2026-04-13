// // STY0003AvoidDynamicAnalyzer.cs
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
public sealed class Sty0003AvoidDynamicAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STY0003];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Use compilation start to check once if this is a test assembly
        context.RegisterCompilationStartAction(compilationContext =>
                                               {
                                                   // Skip analysis entirely for test assemblies
                                                   if (AnalyzerUtilities.IsTestAssembly(compilationContext.Compilation))
                                                       return;

                                                   compilationContext.RegisterSyntaxNodeAction(AnalyzeIdentifierName,
                                                            SyntaxKind.IdentifierName
                                                       );
                                               }
                                              );
    }

    private static void AnalyzeIdentifierName(SyntaxNodeAnalysisContext context)
    {
        var identifierName = (IdentifierNameSyntax) context.Node;
        bool isDynamicIdentifier = string.Equals(identifierName.Identifier.Text, "dynamic", StringComparison.Ordinal);
        var isDynamicType = false;

        if (isDynamicIdentifier)
        {
            var typeSymbol = context.SemanticModel.GetTypeInfo(identifierName, context.CancellationToken).Type;

            if (typeSymbol != null)
            {
                if (typeSymbol.TypeKind == TypeKind.Dynamic)
                    isDynamicType = true;
            }
        }

        if (isDynamicType)
        {
            bool isComImportType = IsComImportContainingType(context.ContainingSymbol?.ContainingType);

            if (!isComImportType)
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STY0003, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsComImportContainingType(INamedTypeSymbol? typeSymbol)
    {
        var result = false;

        if (typeSymbol != null)
        {
            foreach(var attribute in typeSymbol.GetAttributes())
            {
                string attributeName = attribute.AttributeClass?.ToDisplayString() ?? string.Empty;

                if (string.Equals(attributeName,
                                  "System.Runtime.InteropServices.ComImportAttribute",
                                  StringComparison.Ordinal
                                 ))
                {
                    result = true;
                    break;
                }
            }
        }

        return result;
    }
}
