// // STY0006MethodNamingCodeFix.cs
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

namespace CodeStructure.Analyzers.CodeFixes.Style;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Sty0006MethodNamingCodeFix))]
public sealed class Sty0006MethodNamingCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.STY0006];

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
        var methodDeclaration =
            root.FindNode(span).AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();

        if (methodDeclaration != null)
        {
            var semanticModel =
                await document.GetSemanticModelAsync(context.CancellationToken)
                              .ConfigureAwait(continueOnCapturedContext: false) ??
                throw new InvalidOperationException();
            IMethodSymbol? methodSymbol =
                semanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken) as IMethodSymbol;

            if (methodSymbol != null)
            {
                string newName = ToPascalCase(methodSymbol.Name);

                CodeAction action = CodeAction.Create($"Rename to '{newName}'",
                                                      cancellationToken =>
                                                          RenameUtilities.RenameSymbolAsync(document,
                                                                   methodSymbol,
                                                                   newName,
                                                                   cancellationToken
                                                              ),
                                                      nameof(Sty0006MethodNamingCodeFix)
                                                     );

                context.RegisterCodeFix(action, diagnostic);
            }
        }
    }

    private static string ToPascalCase(string name)
    {
        var result = string.Empty;

        if (!string.IsNullOrWhiteSpace(name))
        {
            string[] parts = name.Split(['_'], StringSplitOptions.RemoveEmptyEntries);

            foreach(string part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    char firstChar = char.ToUpperInvariant(part[index: 0]);
                    string rest = part.Length > 1 ? part.Substring(startIndex: 1) : string.Empty;
                    result = string.Concat(result, firstChar, rest);
                }
            }
        }

        return result;
    }
}
