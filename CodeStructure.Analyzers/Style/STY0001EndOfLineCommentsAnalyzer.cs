// // STY0001EndOfLineCommentsAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System.Collections.Immutable;
using CodeStructure.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Style;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Sty0001EndOfLineCommentsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STY0001];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var text = context.Tree.GetText(context.CancellationToken);

        foreach(var token in root.DescendantTokens())
        {
            foreach(var trivia in token.TrailingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    int tokenLine = text.Lines.GetLineFromPosition(token.Span.End).LineNumber;
                    int triviaLine = text.Lines.GetLineFromPosition(trivia.SpanStart).LineNumber;

                    if (tokenLine == triviaLine)
                    {
                        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STY0001, trivia.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
