// // ENC0003PublicProtectedFieldsCodeFix.cs
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

namespace CodeStructure.Analyzers.CodeFixes.Encapsulation;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Enc0003PublicProtectedFieldsCodeFix))]
public sealed class Enc0003PublicProtectedFieldsCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.ENC0003];

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
        var fieldDeclaration = root.FindNode(span).AncestorsAndSelf().OfType<FieldDeclarationSyntax>().FirstOrDefault();

        if (fieldDeclaration != null)
        {
            CodeAction action = CodeAction.Create("Convert field to auto-property",
                                                  cancellationToken =>
                                                      ConvertToPropertyAsync(document,
                                                                             fieldDeclaration,
                                                                             cancellationToken
                                                                            ),
                                                  nameof(Enc0003PublicProtectedFieldsCodeFix)
                                                 );

            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private static async Task<Document> ConvertToPropertyAsync(Document document,
                                                               FieldDeclarationSyntax fieldDeclaration,
                                                               CancellationToken cancellationToken)
    {
        var updatedDocument = document;
        var root =
            await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false) ??
            throw new InvalidOperationException();
        var variable = fieldDeclaration.Declaration.Variables.FirstOrDefault();

        if (variable != null)
        {
            string propertyName = variable.Identifier.Text;
            var type = fieldDeclaration.Declaration.Type;
            var modifiers = fieldDeclaration.Modifiers;
            SyntaxTokenList propertyModifiers = [];

            foreach(var token in modifiers)
            {
                if (!token.IsKind(SyntaxKind.ReadOnlyKeyword) && !token.IsKind(SyntaxKind.ConstKeyword))
                    propertyModifiers = propertyModifiers.Add(token);
            }

            var getAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            var setAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            var accessorList = SyntaxFactory.AccessorList(SyntaxFactory.List([getAccessor, setAccessor]));

            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(type, propertyName)
                                                   .WithModifiers(propertyModifiers)
                                                   .WithAccessorList(accessorList);

            if (variable.Initializer != null)
            {
                propertyDeclaration = propertyDeclaration.WithInitializer(variable.Initializer)
                                                         .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind
                                                                                     .SemicolonToken
                                                                                 )
                                                                            );
            }

            var newRoot = root.ReplaceNode(fieldDeclaration, propertyDeclaration);
            updatedDocument = document.WithSyntaxRoot(newRoot);
        }

        return updatedDocument;
    }
}
