// // STR0010MissingArgumentValidationCodeFix.cs
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

namespace CodeStructure.Analyzers.CodeFixes.Structure;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Str0010MissingArgumentValidationCodeFix))]
public sealed class Str0010MissingArgumentValidationCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.STR0010];

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

        // Find the parameter
        var parameter = node.AncestorsAndSelf().OfType<ParameterSyntax>().FirstOrDefault();

        if (parameter == null)
            return;

        // Find the containing method
        var method = parameter.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();

        if (method?.Body == null)
            return;

        string parameterName = parameter.Identifier.Text;

        CodeAction action = CodeAction.Create($"Add ArgumentNullException.ThrowIfNull({parameterName})",
                                              cancellationToken =>
                                                  AddNullCheckAsync(document, method, parameterName, cancellationToken),
                                              nameof(Str0010MissingArgumentValidationCodeFix) + "_" + parameterName
                                             );

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> AddNullCheckAsync(Document document,
                                                          MethodDeclarationSyntax method,
                                                          string parameterName,
                                                          CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(continueOnCapturedContext: false) ??
                   throw new InvalidOperationException();

        // Create: ArgumentNullException.ThrowIfNull(parameterName);
        var throwIfNullStatement =
            SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory
                                                           .MemberAccessExpression(SyntaxKind
                                                                        .SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName("ArgumentNullException"
                                                                        ),
                                                                    SyntaxFactory.IdentifierName("ThrowIfNull")
                                                               ),
                                                       SyntaxFactory.ArgumentList(SyntaxFactory
                                                               .SingletonSeparatedList(SyntaxFactory
                                                                       .Argument(SyntaxFactory
                                                                               .IdentifierName(parameterName
                                                                                   )
                                                                           )
                                                                   )
                                                           )
                                                  )
                                             );

        // Add proper formatting
        var formattedStatement = throwIfNullStatement
                                 .WithLeadingTrivia(SyntaxFactory.Whitespace("        "))
                                 .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

        // Find the method in the current root (it may have changed)
        var currentMethod = root.DescendantNodes()
                                .OfType<MethodDeclarationSyntax>()
                                .FirstOrDefault(m => m.Identifier.Text == method.Identifier.Text &&
                                                     m.Span.Start == method.Span.Start
                                               );

        if (currentMethod?.Body == null)
            return document;

        // Insert at the beginning of the method body
        var newStatements = currentMethod.Body.Statements.Insert(index: 0, formattedStatement);
        var newBody = currentMethod.Body.WithStatements(newStatements);
        var newMethod = currentMethod.WithBody(newBody);

        var newRoot = root.ReplaceNode(currentMethod, newMethod);

        return document.WithSyntaxRoot(newRoot);
    }
}
