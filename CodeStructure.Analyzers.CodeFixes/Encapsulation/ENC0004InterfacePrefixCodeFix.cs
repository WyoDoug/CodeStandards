// // ENC0004InterfacePrefixCodeFix.cs
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

namespace CodeStructure.Analyzers.CodeFixes.Encapsulation;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Enc0004InterfacePrefixCodeFix))]
public sealed class Enc0004InterfacePrefixCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.ENC0004];

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
        var interfaceDeclaration =
            root.FindNode(span).AncestorsAndSelf().OfType<InterfaceDeclarationSyntax>().FirstOrDefault();

        if (interfaceDeclaration != null)
        {
            var semanticModel =
                await document.GetSemanticModelAsync(context.CancellationToken)
                              .ConfigureAwait(continueOnCapturedContext: false) ??
                throw new InvalidOperationException();
            INamedTypeSymbol? interfaceSymbol =
                semanticModel.GetDeclaredSymbol(interfaceDeclaration, context.CancellationToken) as INamedTypeSymbol;

            if (interfaceSymbol != null)
            {
                string newName = string.Concat("I", interfaceSymbol.Name);

                CodeAction action = CodeAction.Create($"Rename to '{newName}'",
                                                      cancellationToken =>
                                                          RenameUtilities.RenameSymbolAsync(document,
                                                                   interfaceSymbol,
                                                                   newName,
                                                                   cancellationToken
                                                              ),
                                                      nameof(Enc0004InterfacePrefixCodeFix)
                                                     );

                context.RegisterCodeFix(action, diagnostic);
            }
        }
    }
}
