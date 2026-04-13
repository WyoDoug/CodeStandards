// // NUM0002MagicNumbersCodeFix.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CodeStructure.Analyzers.CodeFixes.Utilities;
using CodeStructure.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace CodeStructure.Analyzers.CodeFixes.Numeric;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Num0002MagicNumbersCodeFix))]
public sealed class Num0002MagicNumbersCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.NUM0002];

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
        var literalExpression = root.FindNode(span)
                                    .AncestorsAndSelf()
                                    .OfType<LiteralExpressionSyntax>()
                                    .FirstOrDefault();

        if (literalExpression != null)
        {
            string constantName =
                ExtractConstantUtilities.GenerateConstantNameFromNumber(literalExpression.Token.Value);

            CodeAction action = CodeAction.Create($"Extract to constant '{constantName}'",
                                                  cancellationToken =>
                                                      ExtractConstantUtilities.ExtractToConstantAsync(document,
                                                               literalExpression,
                                                               constantName,
                                                               cancellationToken
                                                          ),
                                                  nameof(Num0002MagicNumbersCodeFix)
                                                 );

            context.RegisterCodeFix(action, diagnostic);
        }
    }
}
