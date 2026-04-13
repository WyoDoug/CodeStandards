// // STY0007RegionNamingCodeFix.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Text;

#endregion

namespace CodeStructure.Analyzers.CodeFixes.Style;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Sty0007RegionNamingCodeFix))]
public sealed class Sty0007RegionNamingCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.STY0007];

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

        // Find the region directive trivia
        var trivia = root.FindTrivia(span.Start);

        if (!trivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            return;

        var regionDirective = trivia.GetStructure() as RegionDirectiveTriviaSyntax;

        if (regionDirective == null)
            return;

        string currentName = regionDirective.EndOfDirectiveToken.LeadingTrivia.ToString().Trim();

        // Find the containing type to suggest a better name
        var containingType = FindContainingType(root, span.Start);
        string typeName = containingType?.Identifier.Text ?? "Type";

        var suggestedName = $"{typeName} {currentName}";

        // Offer to add type prefix
        var prefixAction = CodeAction.Create($"Rename to '{suggestedName}'",
                                             cancellationToken =>
                                                 RenameRegionAsync(document, trivia, suggestedName, cancellationToken),
                                             nameof(Sty0007RegionNamingCodeFix) + "_Prefix"
                                            );

        context.RegisterCodeFix(prefixAction, diagnostic);

        // Offer to remove the region entirely
        var removeAction = CodeAction.Create("Remove region",
                                             cancellationToken =>
                                                 RemoveRegionAsync(document, trivia, cancellationToken),
                                             nameof(Sty0007RegionNamingCodeFix) + "_Remove"
                                            );

        context.RegisterCodeFix(removeAction, diagnostic);
    }

    private static TypeDeclarationSyntax? FindContainingType(SyntaxNode root, int position)
    {
        // Find nodes that contain this position
        var token = root.FindToken(position);
        var node = token.Parent;

        while (node != null)
        {
            if (node is TypeDeclarationSyntax typeDecl)
                return typeDecl;

            node = node.Parent;
        }

        // If trivia is before any code, look for the first type in the file
        return root.DescendantNodes().OfType<TypeDeclarationSyntax>().FirstOrDefault();
    }

    private static async Task<Document> RenameRegionAsync(Document document,
                                                          SyntaxTrivia regionTrivia,
                                                          string newName,
                                                          CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(continueOnCapturedContext: false) ??
                   throw new InvalidOperationException();

        var text = await document.GetTextAsync(cancellationToken)
                                 .ConfigureAwait(continueOnCapturedContext: false);

        var regionDirective = regionTrivia.GetStructure() as RegionDirectiveTriviaSyntax;

        if (regionDirective == null)
            return document;

        // Get the line containing the region directive
        var line = text.Lines.GetLineFromPosition(regionTrivia.SpanStart);
        var lineText = line.ToString();

        // Replace the region name in the line
        int regionIndex = lineText.IndexOf("#region", StringComparison.Ordinal);

        if (regionIndex < 0)
            return document;

        string newLineText = lineText.Substring(startIndex: 0, regionIndex) + "#region " + newName;

        // Create new text with the replaced line
        var newText = text.Replace(line.Span, newLineText);

        return document.WithText(newText);
    }

    private static async Task<Document> RemoveRegionAsync(Document document,
                                                          SyntaxTrivia regionTrivia,
                                                          CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(continueOnCapturedContext: false) ??
                   throw new InvalidOperationException();

        var text = await document.GetTextAsync(cancellationToken)
                                 .ConfigureAwait(continueOnCapturedContext: false);

        // Find both the #region and matching #endregion
        var regionDirective = regionTrivia.GetStructure() as RegionDirectiveTriviaSyntax;

        if (regionDirective == null)
            return document;

        // Find the matching endregion
        EndRegionDirectiveTriviaSyntax? endRegion = null;
        var depth = 0;

        foreach(var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.SpanStart <= regionTrivia.SpanStart)
                continue;

            if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
                depth++;
            else
            {
                if (trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                {
                    if (depth == 0)
                    {
                        endRegion = trivia.GetStructure() as EndRegionDirectiveTriviaSyntax;
                        break;
                    }

                    depth--;
                }
            }
        }

        // Get lines to remove
        var regionLine = text.Lines.GetLineFromPosition(regionTrivia.SpanStart);
        var regionLineSpan = TextSpan.FromBounds(regionLine.Start, regionLine.EndIncludingLineBreak);

        // Build list of changes
        var changes = new List<TextChange>
                          {
                              new TextChange(regionLineSpan, string.Empty)
                          };

        if (endRegion != null)
        {
            var endRegionLine = text.Lines.GetLineFromPosition(endRegion.SpanStart);
            var endRegionLineSpan = TextSpan.FromBounds(endRegionLine.Start, endRegionLine.EndIncludingLineBreak);
            changes.Add(new TextChange(endRegionLineSpan, string.Empty));
        }

        var newText = text.WithChanges(changes);

        return document.WithText(newText);
    }
}
