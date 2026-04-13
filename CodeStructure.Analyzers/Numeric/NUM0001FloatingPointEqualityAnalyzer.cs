// // NUM0001FloatingPointEqualityAnalyzer.cs
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

namespace CodeStructure.Analyzers.Numeric;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Num0001FloatingPointEqualityAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.NUM0001];

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

                                                   compilationContext.RegisterSyntaxNodeAction(AnalyzeBinaryExpression,
                                                            SyntaxKind.EqualsExpression,
                                                            SyntaxKind.NotEqualsExpression
                                                       );
                                               }
                                              );
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax) context.Node;
        var containingSymbol = context.ContainingSymbol;
        bool isTestMethod = AnalyzerUtilities.IsTestMethod(containingSymbol);

        if (!isTestMethod)
        {
            var leftType = context.SemanticModel.GetTypeInfo(binaryExpression.Left, context.CancellationToken).Type;
            var rightType = context.SemanticModel.GetTypeInfo(binaryExpression.Right, context.CancellationToken).Type;

            bool leftIsFloat = IsFloatingPointType(leftType);
            bool rightIsFloat = IsFloatingPointType(rightType);
            bool isFloatComparison = leftIsFloat || rightIsFloat;

            if (isFloatComparison)
            {
                bool isAllowedLiteralComparison =
                    IsAllowedLiteralComparison(binaryExpression.Left, binaryExpression.Right, context.SemanticModel);

                if (!isAllowedLiteralComparison)
                {
                    var diagnostic =
                        Diagnostic.Create(DiagnosticDescriptors.NUM0001, binaryExpression.OperatorToken.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsFloatingPointType(ITypeSymbol? typeSymbol)
    {
        var result = false;

        if (typeSymbol != null)
        {
            if (typeSymbol.SpecialType == SpecialType.System_Double ||
                typeSymbol.SpecialType == SpecialType.System_Single)
                result = true;
        }

        return result;
    }

    private static bool IsAllowedLiteralComparison(ExpressionSyntax left,
                                                   ExpressionSyntax right,
                                                   SemanticModel semanticModel)
    {
        var result = false;

        int? leftInt = GetIntegerLiteralValue(left, semanticModel);
        int? rightInt = GetIntegerLiteralValue(right, semanticModel);

        if (leftInt.HasValue)
        {
            if (IsAllowedIntLiteral(leftInt.Value))
                result = true;
        }

        if (rightInt.HasValue)
        {
            if (IsAllowedIntLiteral(rightInt.Value))
                result = true;
        }

        return result;
    }

    private static int? GetIntegerLiteralValue(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        int? result = null;
        var constantValue = semanticModel.GetConstantValue(expression);

        if (constantValue.HasValue)
        {
            if (constantValue.Value is int intValue)
                result = intValue;
            else
            {
                if (constantValue.Value is long longValue)
                {
                    if (longValue >= int.MinValue && longValue <= int.MaxValue)
                        result = (int) longValue;
                }
            }
        }

        return result;
    }

    private static bool IsAllowedIntLiteral(int value)
    {
        bool result = value == AllowedLiteralNegativeOne || value == AllowedLiteralZero || value == AllowedLiteralOne;
        return result;
    }

    private const int AllowedLiteralNegativeOne = -1;
    private const int AllowedLiteralZero = 0;
    private const int AllowedLiteralOne = 1;
}
