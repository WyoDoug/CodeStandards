// // ENC0001AvoidHidingWithNewCodeFix.cs
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Enc0001AvoidHidingWithNewCodeFix))]
public sealed class Enc0001AvoidHidingWithNewCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.ENC0001];

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
        var member = root.FindNode(span).AncestorsAndSelf().OfType<MemberDeclarationSyntax>().FirstOrDefault();

        if (member != null)
        {
            var semanticModel =
                await document.GetSemanticModelAsync(context.CancellationToken)
                              .ConfigureAwait(continueOnCapturedContext: false) ??
                throw new InvalidOperationException();
            bool canFix = CanOverride(member, semanticModel, context.CancellationToken);

            if (canFix)
            {
                CodeAction action = CodeAction.Create("Replace 'new' with 'override'",
                                                      cancellationToken =>
                                                          ReplaceNewWithOverrideAsync(document,
                                                                   member,
                                                                   cancellationToken
                                                              ),
                                                      nameof(Enc0001AvoidHidingWithNewCodeFix)
                                                     );

                context.RegisterCodeFix(action, diagnostic);
            }
        }
    }

    private static bool CanOverride(MemberDeclarationSyntax member,
                                    SemanticModel semanticModel,
                                    CancellationToken cancellationToken)
    {
        var result = false;
        var symbol = semanticModel.GetDeclaredSymbol(member, cancellationToken);

        if (symbol != null)
        {
            var containingType = symbol.ContainingType;

            if (containingType != null && containingType.BaseType != null)
            {
                foreach(var baseMember in containingType.BaseType.GetMembers(symbol.Name))
                {
                    bool isVirtual = baseMember.IsVirtual || baseMember.IsAbstract || baseMember.IsOverride;

                    if (isVirtual)
                    {
                        result = true;
                        break;
                    }
                }
            }
        }

        return result;
    }

    private static async Task<Document> ReplaceNewWithOverrideAsync(Document document,
                                                                    MemberDeclarationSyntax member,
                                                                    CancellationToken cancellationToken)
    {
        var root =
            await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false) ??
            throw new InvalidOperationException();
        var semanticModel =
            await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false) ??
            throw new InvalidOperationException();
        var baseAccessibility = GetBaseAccessibility(member, semanticModel, cancellationToken);

        var modifiers = member.Modifiers;
        SyntaxTokenList newModifiers = [];
        SyntaxTokenList accessibilityTokens = [];

        foreach(var token in modifiers)
        {
            if (token.IsKind(SyntaxKind.NewKeyword))
                newModifiers = newModifiers.Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword));
            else
            {
                if (IsAccessibilityToken(token))
                    accessibilityTokens = accessibilityTokens.Add(token);
                else
                    newModifiers = newModifiers.Add(token);
            }
        }

        var finalAccessibilityTokens = baseAccessibility.HasValue
                                           ? GetAccessibilityTokens(baseAccessibility.Value)
                                           : accessibilityTokens;

        newModifiers = RemoveAccessibilityTokens(newModifiers);
        newModifiers = finalAccessibilityTokens.AddRange(newModifiers);

        var newMember = member.WithModifiers(newModifiers);
        var newRoot = root.ReplaceNode(member, newMember);
        var updatedDocument = document.WithSyntaxRoot(newRoot);

        return updatedDocument;
    }

    private static Accessibility? GetBaseAccessibility(MemberDeclarationSyntax member,
                                                       SemanticModel semanticModel,
                                                       CancellationToken cancellationToken)
    {
        Accessibility? result = null;
        var symbol = semanticModel.GetDeclaredSymbol(member, cancellationToken);

        if (symbol != null)
        {
            var containingType = symbol.ContainingType;

            if (containingType != null && containingType.BaseType != null)
            {
                foreach(var baseMember in containingType.BaseType.GetMembers(symbol.Name))
                {
                    if (IsMatchingMember(symbol, baseMember))
                    {
                        result = baseMember.DeclaredAccessibility;
                        break;
                    }
                }
            }
        }

        return result;
    }

    private static bool IsMatchingMember(ISymbol symbol, ISymbol baseMember)
    {
        var result = false;

        if (symbol.Kind == baseMember.Kind)
        {
            if (symbol is IMethodSymbol methodSymbol && baseMember is IMethodSymbol baseMethod)
            {
                if (methodSymbol.Parameters.Length == baseMethod.Parameters.Length)
                {
                    var parametersMatch = true;

                    for(var index = 0; index < methodSymbol.Parameters.Length; index++)
                    {
                        if (!SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[index].Type,
                                                                   baseMethod.Parameters[index].Type
                                                                  ))
                        {
                            parametersMatch = false;
                            break;
                        }
                    }

                    if (parametersMatch)
                        result = true;
                }
            }
            else
            {
                if (symbol is IPropertySymbol propertySymbol && baseMember is IPropertySymbol baseProperty)
                    result = SymbolEqualityComparer.Default.Equals(propertySymbol.Type, baseProperty.Type);
                else
                {
                    if (symbol is IEventSymbol eventSymbol && baseMember is IEventSymbol baseEvent)
                        result = SymbolEqualityComparer.Default.Equals(eventSymbol.Type, baseEvent.Type);
                    else
                    {
                        if (symbol is IFieldSymbol fieldSymbol && baseMember is IFieldSymbol baseField)
                            result = SymbolEqualityComparer.Default.Equals(fieldSymbol.Type, baseField.Type);
                    }
                }
            }
        }

        return result;
    }

    private static bool IsAccessibilityToken(SyntaxToken token)
    {
        bool result = token.IsKind(SyntaxKind.PublicKeyword) ||
                      token.IsKind(SyntaxKind.PrivateKeyword) ||
                      token.IsKind(SyntaxKind.ProtectedKeyword) ||
                      token.IsKind(SyntaxKind.InternalKeyword);

        return result;
    }

    private static SyntaxTokenList RemoveAccessibilityTokens(SyntaxTokenList tokens)
    {
        SyntaxTokenList result = [];

        foreach(var token in tokens)
        {
            if (!IsAccessibilityToken(token))
                result = result.Add(token);
        }

        return result;
    }

    private static SyntaxTokenList GetAccessibilityTokens(Accessibility accessibility)
    {
        SyntaxTokenList result = [];

        switch(accessibility)
        {
            case Accessibility.Public:
                result = result.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                break;
            case Accessibility.Protected:
                result = result.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                break;
            case Accessibility.Internal:
                result = result.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                break;
            case Accessibility.ProtectedOrInternal:
                result = result.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                result = result.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                break;
            case Accessibility.ProtectedAndInternal:
                result = result.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                result = result.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                break;
        }

        return result;
    }
}
