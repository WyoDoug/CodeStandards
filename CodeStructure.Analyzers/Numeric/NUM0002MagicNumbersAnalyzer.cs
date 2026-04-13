// // NUM0002MagicNumbersAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using System.Linq;
using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Numeric;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Num0002MagicNumbersAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.NUM0002];

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

                                                   compilationContext.RegisterSyntaxNodeAction(AnalyzeLiteralExpression,
                                                            SyntaxKind.NumericLiteralExpression
                                                       );
                                               }
                                              );
    }

    private static void AnalyzeLiteralExpression(SyntaxNodeAnalysisContext context)
    {
        var literalExpression = (LiteralExpressionSyntax) context.Node;
        bool isExempt = IsExempt(literalExpression, context);

        if (!isExempt)
        {
            var constantValue = context.SemanticModel.GetConstantValue(literalExpression, context.CancellationToken);

            if (constantValue.HasValue)
            {
                bool isAllowed = IsAllowedLiteral(constantValue.Value);

                if (!isAllowed)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.NUM0002,
                                                       literalExpression.GetLocation(),
                                                       literalExpression.Token.ValueText
                                                      );
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsAllowedLiteral(object? value)
    {
        var result = false;

        if (value is int intValue)
            result = NumericLiteralUtilities.IsAllowedInteger(intValue);
        else
        {
            if (value is long longValue)
                result = NumericLiteralUtilities.IsAllowedInteger(longValue);
            else
            {
                if (value is double doubleValue)
                    result = NumericLiteralUtilities.IsAllowedDouble(doubleValue);
                else
                {
                    if (value is float floatValue)
                        result = NumericLiteralUtilities.IsAllowedDouble(floatValue);
                }
            }
        }

        return result;
    }

    private static bool IsExempt(LiteralExpressionSyntax literalExpression, SyntaxNodeAnalysisContext context)
    {
        var result = false;

        if (NumericLiteralUtilities.IsHexLiteral(literalExpression))
            result = true;

        if (!result)
        {
            if (IsInConstDeclaration(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInStaticReadonlyField(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInEnumValue(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInAttributeArgument(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInInitializer(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInGetHashCode(context))
                result = true;
        }

        if (!result)
        {
            if (IsInEntityFrameworkConfiguration(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (AnalyzerUtilities.IsTestMethod(context.ContainingSymbol))
                result = true;
        }

        return result;
    }

    private static bool IsInConstDeclaration(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        if (literalExpression.Parent is EqualsValueClauseSyntax equalsValue)
        {
            if (equalsValue.Parent is VariableDeclaratorSyntax variableDeclarator)
            {
                if (variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration)
                {
                    if (variableDeclaration.Parent is FieldDeclarationSyntax fieldDeclaration)
                    {
                        if (fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
                            result = true;
                    }
                    else
                    {
                        if (variableDeclaration.Parent is LocalDeclarationStatementSyntax localDeclaration)
                        {
                            if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
                                result = true;
                        }
                    }
                }
            }
        }

        return result;
    }

    private static bool IsInStaticReadonlyField(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        if (literalExpression.Parent is EqualsValueClauseSyntax equalsValue)
        {
            if (equalsValue.Parent is VariableDeclaratorSyntax variableDeclarator)
            {
                if (variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration)
                {
                    if (variableDeclaration.Parent is FieldDeclarationSyntax fieldDeclaration)
                    {
                        bool isStatic = fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);
                        bool isReadonly = fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);

                        if (isStatic && isReadonly)
                            result = true;
                    }
                }
            }
        }

        return result;
    }

    private static bool IsInEnumValue(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        if (literalExpression.Parent is EqualsValueClauseSyntax equalsValue)
        {
            if (equalsValue.Parent is EnumMemberDeclarationSyntax)
                result = true;
        }

        return result;
    }

    private static bool IsInAttributeArgument(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        var current = literalExpression.Parent;

        while (current != null && !result)
        {
            if (current is AttributeArgumentSyntax)
                result = true;

            current = current.Parent;
        }

        return result;
    }

    private static bool IsInInitializer(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        var current = literalExpression.Parent;

        while (current != null && !result)
        {
            if (current is InitializerExpressionSyntax)
                result = true;

            current = current.Parent;
        }

        return result;
    }

    private static bool IsInGetHashCode(SyntaxNodeAnalysisContext context)
    {
        var result = false;

        if (context.ContainingSymbol is IMethodSymbol methodSymbol)
        {
            if (string.Equals(methodSymbol.Name, "GetHashCode", StringComparison.Ordinal))
                result = true;
        }

        return result;
    }

    private static bool IsInEntityFrameworkConfiguration(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        // Check if this is an argument to an EF Core fluent API method
        if (literalExpression.Parent is ArgumentSyntax argument)
        {
            if (argument.Parent?.Parent is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    string methodName = memberAccess.Name.Identifier.Text;

                    // Common EF Core fluent API methods that use numeric literals
                    string[] efConfigMethods =
                        [
                            // Column configuration
                            "HasMaxLength", "HasPrecision", "HasColumnOrder",
                            // Index configuration
                            "IsUnique", "IsClustered",
                            // Value generation
                            "HasDefaultValue", "StartsAt", "IncrementsBy",
                            // Sequence configuration
                            "HasMin", "HasMax",
                            // Temporal tables
                            "HasPeriodStart", "HasPeriodEnd",
                            // Other configuration
                            "HasAnnotation"
                        ];

                    result = efConfigMethods.Any(m => string.Equals(m, methodName, StringComparison.Ordinal));
                }
            }
        }

        // Also check for being inside EF configuration methods or classes
        if (!result)
        {
            var current = literalExpression.Parent;

            while (current != null && !result)
            {
                // Check if we're in a method that's commonly used for EF model configuration
                if (current is MethodDeclarationSyntax methodDeclaration)
                {
                    string methodName = methodDeclaration.Identifier.Text;

                    // Common EF configuration method names
                    if (methodName == "OnModelCreating" ||
                        methodName == "Configure" ||
                        methodName == "Up" ||
                        methodName == "Down" ||
                        methodName == "BuildModel" ||
                        methodName == "BuildTargetModel")
                        result = true;
                }

                // Check if we're in a class that implements IEntityTypeConfiguration or inherits from Migration
                if (current is ClassDeclarationSyntax classDeclaration)
                {
                    if (classDeclaration.BaseList != null)
                    {
                        foreach(var baseType in classDeclaration.BaseList.Types)
                        {
                            var baseTypeName = baseType.Type.ToString();

                            if (baseTypeName.Contains("IEntityTypeConfiguration") ||
                                baseTypeName.Contains("Migration") ||
                                baseTypeName.Contains("DbContext"))
                                result = true;
                        }
                    }
                }

                current = current.Parent;
            }
        }

        return result;
    }
}
