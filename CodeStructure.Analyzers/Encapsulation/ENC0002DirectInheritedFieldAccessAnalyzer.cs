// // ENC0002DirectInheritedFieldAccessAnalyzer.cs
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

namespace CodeStructure.Analyzers.Encapsulation;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Enc0002DirectInheritedFieldAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.ENC0002];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeIdentifier, SyntaxKind.IdentifierName);
    }

    private static void AnalyzeIdentifier(SyntaxNodeAnalysisContext context)
    {
        var identifier = (IdentifierNameSyntax) context.Node;
        var symbol = ModelExtensions.GetSymbolInfo(context.SemanticModel, identifier, context.CancellationToken).Symbol;

        if (symbol is IFieldSymbol fieldSymbol)
        {
            bool isEligible = IsEligibleField(fieldSymbol);

            if (isEligible)
            {
                var containingType = context.ContainingSymbol?.ContainingType;
                bool isInheritedField = IsInheritedField(fieldSymbol, containingType);

                if (isInheritedField)
                {
                    var diagnostic =
                        Diagnostic.Create(DiagnosticDescriptors.ENC0002, identifier.GetLocation(), fieldSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsEligibleField(IFieldSymbol fieldSymbol)
    {
        var result = false;

        if (!fieldSymbol.IsConst && !fieldSymbol.IsReadOnly && !fieldSymbol.IsStatic)
        {
            if (fieldSymbol.DeclaredAccessibility == Accessibility.Protected ||
                fieldSymbol.DeclaredAccessibility == Accessibility.Internal ||
                fieldSymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal ||
                fieldSymbol.DeclaredAccessibility == Accessibility.ProtectedAndInternal)
                result = true;
        }

        return result;
    }

    private static bool IsInheritedField(IFieldSymbol fieldSymbol, INamedTypeSymbol? containingType)
    {
        var result = false;

        if (containingType != null)
        {
            var currentBase = containingType.BaseType;

            while (currentBase != null && !result)
            {
                if (SymbolEqualityComparer.Default.Equals(currentBase, fieldSymbol.ContainingType))
                    result = true;
                else
                    currentBase = currentBase.BaseType;
            }
        }

        return result;
    }
}
