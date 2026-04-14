// // ArgumentValidationUtilities.cs
// // Copyright � 2012�Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace CodeStructure.Analyzers.Utilities;

public static class ArgumentValidationUtilities
{
    public static bool HasNullCheck(MethodDeclarationSyntax methodDeclaration,
                                    IParameterSymbol parameterSymbol,
                                    SemanticModel semanticModel)
    {
        var result = false;

        if (methodDeclaration.Body != null)
        {
            var nodes = methodDeclaration.Body.DescendantNodes();

            foreach(var node in nodes)
            {
                if (IsNullComparison(node, parameterSymbol, semanticModel))
                {
                    result = true;
                    break;
                }

                if (IsThrowIfInvocation(node, parameterSymbol, semanticModel))
                {
                    result = true;
                    break;
                }

                if (IsArgumentNullException(node, parameterSymbol, semanticModel))
                {
                    result = true;
                    break;
                }
            }
        }

        return result;
    }

    private static bool IsNullComparison(SyntaxNode node, IParameterSymbol parameterSymbol, SemanticModel semanticModel)
    {
        var result = false;

        if (node is BinaryExpressionSyntax binaryExpression)
        {
            if (binaryExpression.IsKind(SyntaxKind.EqualsExpression) ||
                binaryExpression.IsKind(SyntaxKind.NotEqualsExpression))
            {
                bool leftIsParam = IsParameterIdentifier(binaryExpression.Left, parameterSymbol, semanticModel);
                bool rightIsNull = IsNullLiteral(binaryExpression.Right);
                bool rightIsParam = IsParameterIdentifier(binaryExpression.Right, parameterSymbol, semanticModel);
                bool leftIsNull = IsNullLiteral(binaryExpression.Left);

                if ((leftIsParam && rightIsNull) || (rightIsParam && leftIsNull))
                    result = true;
            }
        }

        return result;
    }

    private static bool IsThrowIfInvocation(SyntaxNode node,
                                             IParameterSymbol parameterSymbol,
                                             SemanticModel semanticModel)
    {
        var result = false;

        if (node is InvocationExpressionSyntax invocation)
        {
            string methodName = GetInvocationName(invocation);

            if (methodName.StartsWith("ThrowIf", StringComparison.Ordinal))
            {
                if (IsExceptionClassMethod(invocation, semanticModel))
                {
                    var arguments = invocation.ArgumentList.Arguments;

                    if (arguments.Count > 0)
                    {
                        var firstArgument = arguments[index: 0];

                        if (IsParameterIdentifier(firstArgument.Expression, parameterSymbol, semanticModel))
                            result = true;
                    }
                }
            }
        }

        return result;
    }

    private static bool IsExceptionClassMethod(InvocationExpressionSyntax invocation,
                                                SemanticModel semanticModel)
    {
        var result = false;
        var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

        if (methodSymbol != null)
        {
            var containingType = methodSymbol.ContainingType;

            if (containingType != null)
            {
                if (InheritsFromException(containingType))
                    result = true;
            }
        }

        return result;
    }

    private static bool InheritsFromException(INamedTypeSymbol type)
    {
        var result = false;
        var current = type;

        while (current != null)
        {
            if (string.Equals(current.Name, "Exception", StringComparison.Ordinal) &&
                string.Equals(current.ContainingNamespace?.ToString(), "System", StringComparison.Ordinal))
            {
                result = true;
                break;
            }

            current = current.BaseType;
        }

        return result;
    }

    private static bool IsArgumentNullException(SyntaxNode node,
                                                IParameterSymbol parameterSymbol,
                                                SemanticModel semanticModel)
    {
        var result = false;

        if (node is ObjectCreationExpressionSyntax objectCreation)
        {
            var typeName = objectCreation.Type.ToString();

            if (typeName.Contains(ArgumentNullExceptionName, StringComparison.Ordinal))
            {
                foreach(var argument in objectCreation.ArgumentList?.Arguments ??
                                            [])
                {
                    if (argument.Expression is InvocationExpressionSyntax invocation)
                    {
                        string invocationName = GetInvocationName(invocation);

                        if (string.Equals(invocationName, "nameof", StringComparison.Ordinal))
                        {
                            if (invocation.ArgumentList.Arguments.Count == 1)
                            {
                                var argumentExpression = invocation.ArgumentList.Arguments[index: 0].Expression;

                                if (IsParameterIdentifier(argumentExpression, parameterSymbol, semanticModel))
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private static string GetInvocationName(InvocationExpressionSyntax invocation)
    {
        var name = string.Empty;

        if (invocation.Expression is IdentifierNameSyntax identifierName)
            name = identifierName.Identifier.Text;
        else
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                name = memberAccess.Name.Identifier.Text;
        }

        return name;
    }

    private static bool IsParameterIdentifier(ExpressionSyntax expression,
                                              IParameterSymbol parameterSymbol,
                                              SemanticModel semanticModel)
    {
        var result = false;

        if (expression is IdentifierNameSyntax identifierName)
        {
            var symbol = semanticModel.GetSymbolInfo(identifierName).Symbol;

            if (SymbolEqualityComparer.Default.Equals(symbol, parameterSymbol))
                result = true;
        }

        return result;
    }

    private static bool IsNullLiteral(ExpressionSyntax expression)
    {
        var result = false;

        if (expression.IsKind(SyntaxKind.NullLiteralExpression))
            result = true;

        return result;
    }

    private const string ArgumentNullExceptionName = "ArgumentNullException";
}
