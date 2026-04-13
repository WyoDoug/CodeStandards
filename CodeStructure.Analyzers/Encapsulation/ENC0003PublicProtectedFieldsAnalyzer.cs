// // ENC0003PublicProtectedFieldsAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System.Collections.Immutable;
using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Encapsulation;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Enc0003PublicProtectedFieldsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.ENC0003];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax) context.Node;
        var containingType = context.ContainingSymbol?.ContainingType;

        bool isExemptType = IsExemptType(containingType);

        if (!isExemptType)
        {
            foreach(var variable in fieldDeclaration.Declaration.Variables)
            {
                var fieldSymbol =
                    ModelExtensions.GetDeclaredSymbol(context.SemanticModel, variable, context.CancellationToken) as
                        IFieldSymbol;

                if (fieldSymbol != null)
                {
                    bool isExemptField = fieldSymbol.IsConst || fieldSymbol.IsReadOnly || fieldSymbol.IsStatic;

                    if (!isExemptField)
                    {
                        bool isNonPrivate = fieldSymbol.DeclaredAccessibility == Accessibility.Public ||
                                            fieldSymbol.DeclaredAccessibility == Accessibility.Protected ||
                                            fieldSymbol.DeclaredAccessibility == Accessibility.Internal ||
                                            fieldSymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal ||
                                            fieldSymbol.DeclaredAccessibility == Accessibility.ProtectedAndInternal;

                        if (isNonPrivate)
                        {
                            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ENC0003,
                                                               variable.Identifier.GetLocation(),
                                                               fieldSymbol.Name
                                                              );
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }

    private static bool IsExemptType(INamedTypeSymbol? typeSymbol)
    {
        var result = false;

        if (typeSymbol != null)
        {
            if (typeSymbol.IsValueType)
                result = true;
            else
            {
                if (typeSymbol.IsRecord)
                    result = true;
                else
                {
                    if (InheritanceUtilities.HasStructLayoutAttribute(typeSymbol))
                        result = true;
                    else
                    {
                        if (InheritanceUtilities.IsWpfType(typeSymbol))
                            result = true;
                    }
                }
            }
        }

        return result;
    }
}
