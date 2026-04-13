// // STY0001EndOfLineCommentsCodeFix.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using CodeStructure.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

#endregion

namespace CodeStructure.Analyzers.CodeFixes.Style;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Sty0001EndOfLineCommentsCodeFix))]
public sealed class Sty0001EndOfLineCommentsCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.STY0001];

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
        var trivia = root.FindTrivia(span.Start);

        CodeAction action = CodeAction.Create("Move comment to line above",
                                              cancellationToken =>
                                                  MoveCommentAsync(document, trivia, cancellationToken),
                                              nameof(Sty0001EndOfLineCommentsCodeFix)
                                             );

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> MoveCommentAsync(Document document,
                                                         SyntaxTrivia trivia,
                                                         CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        var line = text.Lines.GetLineFromPosition(trivia.SpanStart);
        var lineText = line.ToString();
        string indentation = GetIndentation(lineText);
        string commentText = trivia.ToString().TrimEnd();
        string newLineText = string.Concat(indentation, commentText, Environment.NewLine);

        int removalStart = trivia.SpanStart;

        if (removalStart > line.Start)
        {
            int index = removalStart - 1;

            while (index >= line.Start && (text[index] == ' ' || text[index] == '\t'))
                index = index - 1;

            removalStart = index + 1;
        }

        TextSpan removalSpan = TextSpan.FromBounds(removalStart, trivia.Span.End);
        TextChange insertChange = new TextChange(new TextSpan(line.Start, length: 0), newLineText);
        TextChange removeChange = new TextChange(removalSpan, string.Empty);
        var newText = text.WithChanges(insertChange, removeChange);
        var updatedDocument = document.WithText(newText);

        return updatedDocument;
    }

    private static string GetIndentation(string lineText)
    {
        var index = 0;

        while (index < lineText.Length && char.IsWhiteSpace(lineText[index]))
            index = index + 1;

        string result = lineText.Substring(startIndex: 0, index);
        return result;
    }
}
