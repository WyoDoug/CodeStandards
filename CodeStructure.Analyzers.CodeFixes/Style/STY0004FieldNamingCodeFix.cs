// // STY0004FieldNamingCodeFix.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeStructure.Analyzers.CodeFixes.Utilities;
using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace CodeStructure.Analyzers.CodeFixes.Style;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Sty0004FieldNamingCodeFix))]
public sealed class Sty0004FieldNamingCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.STY0004];

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
        var variableDeclarator = root.FindToken(span.Start)
                                     .Parent?.AncestorsAndSelf()
                                     .OfType<VariableDeclaratorSyntax>()
                                     .FirstOrDefault();

        if (variableDeclarator != null)
        {
            var semanticModel =
                await document.GetSemanticModelAsync(context.CancellationToken)
                              .ConfigureAwait(continueOnCapturedContext: false) ??
                throw new InvalidOperationException();
            IFieldSymbol? fieldSymbol =
                semanticModel.GetDeclaredSymbol(variableDeclarator, context.CancellationToken) as IFieldSymbol;

            if (fieldSymbol != null)
            {
                string expectedName = FieldNamingUtilities.GetExpectedFieldName(fieldSymbol);

                CodeAction action = CodeAction.Create($"Rename to '{expectedName}'",
                                                      cancellationToken =>
                                                          ApplyRenameAsync(document,
                                                                           fieldSymbol,
                                                                           expectedName,
                                                                           cancellationToken
                                                                          ),
                                                      nameof(Sty0004FieldNamingCodeFix)
                                                     );

                context.RegisterCodeFix(action, diagnostic);
            }
        }
    }

    private static async Task<Solution> ApplyRenameAsync(Document document,
                                                         IFieldSymbol fieldSymbol,
                                                         string expectedName,
                                                         CancellationToken cancellationToken)
    {
        var updatedSolution = await RenameUtilities
                                    .RenameSymbolAsync(document, fieldSymbol, expectedName, cancellationToken)
                                    .ConfigureAwait(continueOnCapturedContext: false);
        return updatedSolution;
    }
}
