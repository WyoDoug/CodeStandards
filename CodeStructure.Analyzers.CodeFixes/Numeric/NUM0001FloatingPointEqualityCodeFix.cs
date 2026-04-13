// // NUM0001FloatingPointEqualityCodeFix.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeStructure.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace CodeStructure.Analyzers.CodeFixes.Numeric;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Num0001FloatingPointEqualityCodeFix))]
public sealed class Num0001FloatingPointEqualityCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.NUM0001];

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken)
                                 .ConfigureAwait(continueOnCapturedContext: false) ??
                   throw new InvalidOperationException();
        var diagnostic = context.Diagnostics[index: 0];
        var span = diagnostic.Location.SourceSpan;
        var node = root.FindNode(span);

        // The diagnostic is on the operator token, find the parent binary expression
        var binaryExpression = node.AncestorsAndSelf().OfType<BinaryExpressionSyntax>().FirstOrDefault();

        if (binaryExpression == null)
            return;

        bool isEquality = binaryExpression.IsKind(SyntaxKind.EqualsExpression);
        bool isInequality = binaryExpression.IsKind(SyntaxKind.NotEqualsExpression);

        if (!isEquality && !isInequality)
            return;

        string title = isEquality
                           ? "Use Math.Abs comparison with epsilon"
                           : "Use Math.Abs comparison with epsilon";

        CodeAction action = CodeAction.Create(title,
                                              cancellationToken =>
                                                  ReplaceWithEpsilonComparisonAsync(document,
                                                           binaryExpression,
                                                           isEquality,
                                                           cancellationToken
                                                      ),
                                              nameof(Num0001FloatingPointEqualityCodeFix)
                                             );

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> ReplaceWithEpsilonComparisonAsync(Document document,
                                                                          BinaryExpressionSyntax binaryExpression,
                                                                          bool isEquality,
                                                                          CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(continueOnCapturedContext: false) ??
                   throw new InvalidOperationException();

        var left = binaryExpression.Left;
        var right = binaryExpression.Right;

        // Create: Math.Abs(left - right)
        var subtraction = SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression,
                                                         left.WithoutTrivia(),
                                                         right.WithoutTrivia()
                                                        );

        var mathAbs =
            SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind
                                                            .SimpleMemberAccessExpression,
                                                        SyntaxFactory.IdentifierName("Math"),
                                                        SyntaxFactory.IdentifierName("Abs")
                                                   ),
                                               SyntaxFactory.ArgumentList(SyntaxFactory
                                                                              .SingletonSeparatedList(SyntaxFactory
                                                                                      .Argument(subtraction)
                                                                                  )
                                                                         )
                                              );

        // Create epsilon literal
        var epsilon = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                      SyntaxFactory.Literal(double.Parse(DefaultEpsilon))
                                                     );

        // Create comparison: Math.Abs(left - right) < epsilon (for ==)
        //                 or Math.Abs(left - right) >= epsilon (for !=)
        ExpressionSyntax newExpression;

        if (isEquality)
        {
            newExpression = SyntaxFactory.BinaryExpression(SyntaxKind.LessThanExpression,
                                                           mathAbs,
                                                           epsilon
                                                          );
        }
        else
        {
            newExpression = SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression,
                                                           mathAbs,
                                                           epsilon
                                                          );
        }

        // Preserve trivia
        newExpression = newExpression
                        .WithLeadingTrivia(binaryExpression.GetLeadingTrivia())
                        .WithTrailingTrivia(binaryExpression.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(binaryExpression, newExpression);

        return document.WithSyntaxRoot(newRoot);
    }

    private const string DefaultEpsilon = "0.0001";
}
