// // STR0003ReturnInVoidMethodCodeFix.cs
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
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace CodeStructure.Analyzers.CodeFixes.Structure;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Str0003ReturnInVoidMethodCodeFix))]
public sealed class Str0003ReturnInVoidMethodCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.STR0003];

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
        var returnStatement = root.FindNode(span).AncestorsAndSelf().OfType<ReturnStatementSyntax>().FirstOrDefault();

        if (returnStatement != null)
        {
            CodeAction action = CodeAction.Create("Remove return statement",
                                                  cancellationToken =>
                                                      RemoveReturnAsync(document, returnStatement, cancellationToken),
                                                  nameof(Str0003ReturnInVoidMethodCodeFix)
                                                 );

            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private static async Task<Document> RemoveReturnAsync(Document document,
                                                          ReturnStatementSyntax returnStatement,
                                                          CancellationToken cancellationToken)
    {
        var root =
            await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false) ??
            throw new InvalidOperationException();
        var newRoot = root.RemoveNode(returnStatement, SyntaxRemoveOptions.KeepNoTrivia) ?? root;
        var updatedDocument = document.WithSyntaxRoot(newRoot);

        return updatedDocument;
    }
}
