// // ExtractConstantUtilities.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace CodeStructure.Analyzers.CodeFixes.Utilities;

internal static class ExtractConstantUtilities
{
    internal static async Task<Document> ExtractToConstantAsync(Document document,
                                                                LiteralExpressionSyntax literalExpression,
                                                                string constantName,
                                                                CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(continueOnCapturedContext: false);

        if (root == null)
            return document;

        // Find the containing type declaration
        var containingType = literalExpression.Ancestors()
                                              .OfType<TypeDeclarationSyntax>()
                                              .FirstOrDefault();

        if (containingType == null)
            return document;

        // Build the const field declaration
        var typeSyntax = GetTypeSyntax(literalExpression);
        var constField = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(typeSyntax)
                                                                     .WithVariables(SyntaxFactory
                                                                             .SingletonSeparatedList(SyntaxFactory
                                                                                     .VariableDeclarator(SyntaxFactory
                                                                                             .Identifier(constantName
                                                                                                 )
                                                                                         )
                                                                                     .WithInitializer(SyntaxFactory
                                                                                             .EqualsValueClause(literalExpression
                                                                                                 )
                                                                                         )
                                                                                 )
                                                                         )
                                                       )
                                      .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind
                                                                                     .PrivateKeyword
                                                                                 ),
                                                                             SyntaxFactory.Token(SyntaxKind.ConstKeyword
                                                                                 )
                                                                            )
                                                    )
                                      .NormalizeWhitespace()
                                      .WithLeadingTrivia(SyntaxFactory.Whitespace("    "))
                                      .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

        // Replace the literal with a reference to the constant
        var constantReference = SyntaxFactory.IdentifierName(constantName);
        var newRoot = root.ReplaceNode(literalExpression, constantReference);

        // Find the containing type again in the new tree
        var updatedContainingType = newRoot.DescendantNodes()
                                           .OfType<TypeDeclarationSyntax>()
                                           .FirstOrDefault(t => t.Identifier.Text == containingType.Identifier.Text);

        if (updatedContainingType == null)
            return document;

        // Insert the const field at the top of the type
        var firstMember = updatedContainingType.Members.FirstOrDefault();

        TypeDeclarationSyntax newType;

        if (firstMember != null)
            newType = updatedContainingType.InsertNodesBefore(firstMember, [constField]);
        else
            newType = updatedContainingType.AddMembers(constField);

        newRoot = newRoot.ReplaceNode(updatedContainingType, newType);

        return document.WithSyntaxRoot(newRoot);
    }

    internal static string GenerateConstantNameFromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "EmptyValue";

        // Convert to PascalCase: "some value" -> "SomeValue", "ACTIVE" -> "Active"
        var result = new StringBuilder();
        var capitalizeNext = true;

        foreach(char c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                if (capitalizeNext)
                {
                    result.Append(char.ToUpperInvariant(c));
                    capitalizeNext = false;
                }
                else
                    result.Append(c);
            }
            else
                capitalizeNext = true;
        }

        var name = result.ToString();

        // Ensure it starts with a letter
        if (name.Length == 0 || !char.IsLetter(name[index: 0]))
            name = "Value" + name;

        // Truncate if too long
        if (name.Length > 30)
            name = name.Substring(startIndex: 0, length: 30);

        return name;
    }

    internal static string GenerateConstantNameFromNumber(object? value)
    {
        if (value == null)
            return "Value";

        string text = value.ToString() ?? "Value";

        // Replace negative sign and decimal point
        text = text.Replace("-", "Negative");
        text = text.Replace(".", "Point");

        // Ensure it starts with a letter
        if (text.Length > 0 && char.IsDigit(text[index: 0]))
            text = "Value" + text;

        return text;
    }

    private static TypeSyntax GetTypeSyntax(LiteralExpressionSyntax literal)
    {
        TypeSyntax result;

        if (literal.IsKind(SyntaxKind.StringLiteralExpression))
            result = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
        else
        {
            if (literal.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                if (literal.Token.Value is int)
                    result = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
                else
                {
                    if (literal.Token.Value is long)
                        result = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword));
                    else
                    {
                        if (literal.Token.Value is double)
                            result = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
                        else
                        {
                            if (literal.Token.Value is float)
                                result = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword));
                            else
                                result = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
                        }
                    }
                }
            }
            else
                result = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
        }

        return result;
    }
}
