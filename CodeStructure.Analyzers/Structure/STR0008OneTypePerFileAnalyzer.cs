// // STR0008OneTypePerFileAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System.Collections.Immutable;
using System.Linq;
using CodeStructure.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Structure;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Str0008OneTypePerFileAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STR0008];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var topLevelTypes = root.DescendantNodes()
                                .OfType<BaseTypeDeclarationSyntax>()
                                .Where(node => node.Parent is NamespaceDeclarationSyntax ||
                                               node.Parent is FileScopedNamespaceDeclarationSyntax ||
                                               node.Parent is CompilationUnitSyntax
                                      )
                                .ToArray();

        if (topLevelTypes.Length > 1)
        {
            var secondType = topLevelTypes[1];
            var location = secondType.Identifier.GetLocation();
            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STR0008, location);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
