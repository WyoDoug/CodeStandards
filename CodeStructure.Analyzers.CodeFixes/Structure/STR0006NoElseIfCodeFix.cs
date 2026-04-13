// // STR0006NoElseIfCodeFix.cs
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

#endregion

namespace CodeStructure.Analyzers.CodeFixes.Structure;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Str0006NoElseIfCodeFix))]
public sealed class Str0006NoElseIfCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.STR0006];

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
        var ifStatement = root.FindNode(span).AncestorsAndSelf().OfType<IfStatementSyntax>().FirstOrDefault();

        if (ifStatement != null)
        {
            var topIf = GetTopLevelIf(ifStatement);
            bool canConvert = TryBuildSwitchExpression(topIf, out var _);

            if (canConvert)
            {
                CodeAction action = CodeAction.Create("Convert to switch expression",
                                                      cancellationToken =>
                                                          ConvertToSwitchExpressionAsync(document,
                                                                   topIf,
                                                                   cancellationToken
                                                              ),
                                                      nameof(Str0006NoElseIfCodeFix)
                                                     );

                context.RegisterCodeFix(action, diagnostic);
            }
        }
    }

    private static IfStatementSyntax GetTopLevelIf(IfStatementSyntax ifStatement)
    {
        var result = ifStatement;
        var current = ifStatement.Parent;

        while (current is ElseClauseSyntax elseClause)
        {
            if (elseClause.Parent is IfStatementSyntax parentIf)
            {
                result = parentIf;
                current = parentIf.Parent;
            }
            else
                current = null;
        }

        return result;
    }

    private static async Task<Document> ConvertToSwitchExpressionAsync(Document document,
                                                                       IfStatementSyntax ifStatement,
                                                                       CancellationToken cancellationToken)
    {
        var updatedDocument = document;
        var root =
            await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false) ??
            throw new InvalidOperationException();
        bool canConvert = TryBuildSwitchExpression(ifStatement, out var switchReturn);

        if (canConvert && switchReturn != null)
        {
            var newRoot = root.ReplaceNode(ifStatement, switchReturn);
            updatedDocument = document.WithSyntaxRoot(newRoot);
        }

        return updatedDocument;
    }

    private static bool TryBuildSwitchExpression(IfStatementSyntax ifStatement, out ReturnStatementSyntax? switchReturn)
    {
        var result = false;
        switchReturn = null;
        List<(ExpressionSyntax Pattern, ExpressionSyntax Body)> arms =
                [];
        ExpressionSyntax? targetExpression = null;
        ExpressionSyntax? elseExpression = null;
        var current = ifStatement;

        while (current != null)
        {
            if (TryGetConditionPattern(current.Condition, out var candidateTarget, out var patternLiteral))
            {
                if (targetExpression == null)
                    targetExpression = candidateTarget;

                if (targetExpression != null && SyntaxFactory.AreEquivalent(targetExpression, candidateTarget))
                {
                    if (patternLiteral != null && TryGetReturnExpression(current.Statement, out var armExpression))
                        arms.Add((patternLiteral, armExpression));
                }
            }

            if (current.Else == null)
                current = null;
            else
            {
                if (current.Else.Statement is IfStatementSyntax elseIf)
                    current = elseIf;
                else
                {
                    if (TryGetReturnExpression(current.Else.Statement, out var elseReturnExpression))
                        elseExpression = elseReturnExpression;

                    current = null;
                }
            }
        }

        if (targetExpression != null && elseExpression != null)
        {
            if (arms.Count > 0)
            {
                List<SwitchExpressionArmSyntax> switchArms = [];

                foreach(var arm in arms)
                {
                    switchArms.Add(SyntaxFactory.SwitchExpressionArm(SyntaxFactory.ConstantPattern(arm.Pattern),
                                                                     arm.Body
                                                                    )
                                  );
                }

                switchArms.Add(SyntaxFactory.SwitchExpressionArm(SyntaxFactory.DiscardPattern(), elseExpression));

                var switchExpression =
                    SyntaxFactory.SwitchExpression(targetExpression, SyntaxFactory.SeparatedList(switchArms));
                switchReturn = SyntaxFactory.ReturnStatement(switchExpression);
                result = true;
            }
        }

        return result;
    }

    private static bool TryGetReturnExpression(StatementSyntax statement, out ExpressionSyntax expression)
    {
        var result = false;
        expression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

        if (statement is ReturnStatementSyntax returnStatement && returnStatement.Expression != null)
        {
            expression = returnStatement.Expression;
            result = true;
        }
        else
        {
            if (statement is BlockSyntax block && block.Statements.Count == 1)
            {
                if (block.Statements[index: 0] is ReturnStatementSyntax blockReturn && blockReturn.Expression != null)
                {
                    expression = blockReturn.Expression;
                    result = true;
                }
            }
        }

        return result;
    }

    private static bool TryGetConditionPattern(ExpressionSyntax condition,
                                               out ExpressionSyntax? target,
                                               out ExpressionSyntax? literal)
    {
        var result = false;
        target = null;
        literal = null;

        if (condition is BinaryExpressionSyntax binaryExpression)
        {
            if (binaryExpression.IsKind(SyntaxKind.EqualsExpression))
            {
                if (binaryExpression.Left is LiteralExpressionSyntax leftLiteral)
                {
                    target = binaryExpression.Right;
                    literal = leftLiteral;
                    result = true;
                }
                else
                {
                    if (binaryExpression.Right is LiteralExpressionSyntax rightLiteral)
                    {
                        target = binaryExpression.Left;
                        literal = rightLiteral;
                        result = true;
                    }
                }
            }
        }

        return result;
    }
}
