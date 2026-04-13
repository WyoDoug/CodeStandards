// // ENC0004InterfacePrefixAnalyzer.cs
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

namespace CodeStructure.Analyzers.Encapsulation;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Enc0004InterfacePrefixAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.ENC0004];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
    }

    private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        var interfaceDeclaration = (InterfaceDeclarationSyntax) context.Node;
        string name = interfaceDeclaration.Identifier.Text;
        bool isValid = name.StartsWith("I", StringComparison.Ordinal) &&
                       name.Length > 1 &&
                       char.IsUpper(name[index: 1]);

        if (!isValid)
        {
            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ENC0004,
                                               interfaceDeclaration.Identifier.GetLocation(),
                                               name
                                              );
            context.ReportDiagnostic(diagnostic);
        }
    }
}
