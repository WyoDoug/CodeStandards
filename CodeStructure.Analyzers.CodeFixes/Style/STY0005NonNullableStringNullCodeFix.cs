// // STY0005NonNullableStringNullCodeFix.cs
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

namespace CodeStructure.Analyzers.CodeFixes.Style;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Sty0005NonNullableStringNullCodeFix))]
public sealed class Sty0005NonNullableStringNullCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.STY0005];

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
        var variableDeclarator = node.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().FirstOrDefault();

        if (variableDeclarator != null)
        {
            CodeAction action = CodeAction.Create("Replace with string.Empty",
                                                  cancellationToken =>
                                                      ReplaceWithStringEmptyAsync(document,
                                                               variableDeclarator,
                                                               cancellationToken
                                                          ),
                                                  nameof(Sty0005NonNullableStringNullCodeFix)
                                                 );

            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private static async Task<Document> ReplaceWithStringEmptyAsync(Document document,
                                                                    VariableDeclaratorSyntax variableDeclarator,
                                                                    CancellationToken cancellationToken)
    {
        var root =
            await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false) ??
            throw new InvalidOperationException();

        var initializer = variableDeclarator.Initializer;
        var updatedDeclarator = variableDeclarator;

        if (initializer != null)
        {
            var newValue = SyntaxFactory.ParseExpression("string.Empty");
            var newInitializer = initializer.WithValue(newValue);
            updatedDeclarator = variableDeclarator.WithInitializer(newInitializer);
        }

        var newRoot = root.ReplaceNode(variableDeclarator, updatedDeclarator);
        var updatedDocument = document.WithSyntaxRoot(newRoot);

        return updatedDocument;
    }
}
