// // STR0010MissingArgumentValidationAnalyzer.cs
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

namespace CodeStructure.Analyzers.Structure;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Str0010MissingArgumentValidationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STR0010];

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

                                                   compilationContext.RegisterSyntaxNodeAction(AnalyzeMethod,
                                                            SyntaxKind.MethodDeclaration
                                                       );
                                               }
                                              );
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax) context.Node;
        var methodSymbol =
            ModelExtensions.GetDeclaredSymbol(context.SemanticModel, methodDeclaration, context.CancellationToken) as
                IMethodSymbol;

        if (methodSymbol == null)
            return;

        // Skip methods that don't have a body (interface, abstract, extern, partial)
        if (methodDeclaration.Body == null && methodDeclaration.ExpressionBody == null)
            return;

        // Skip abstract methods
        if (methodSymbol.IsAbstract)
            return;

        // Skip methods in interfaces
        if (methodSymbol.ContainingType?.TypeKind == TypeKind.Interface)
            return;

        if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
            return;

        foreach(var parameter in methodSymbol.Parameters)
        {
            bool shouldCheck = ShouldValidate(parameter);

            if (shouldCheck)
            {
                bool hasCheck =
                    ArgumentValidationUtilities.HasNullCheck(methodDeclaration, parameter, context.SemanticModel);

                if (!hasCheck)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STR0010,
                                                       parameter.Locations[index: 0],
                                                       methodSymbol.Name,
                                                       parameter.Name
                                                      );

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool ShouldValidate(IParameterSymbol parameter)
    {
        var result = false;

        if (parameter.Type.IsReferenceType)
        {
            bool isNullable = AnalyzerUtilities.IsNullableReferenceType(parameter.Type);

            if (!isNullable)
            {
                if (!parameter.HasExplicitDefaultValue)
                    result = true;
            }
        }

        return result;
    }
}
