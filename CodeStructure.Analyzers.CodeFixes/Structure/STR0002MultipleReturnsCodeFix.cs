// // STR0002MultipleReturnsCodeFix.cs
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Str0002MultipleReturnsCodeFix))]
public sealed class Str0002MultipleReturnsCodeFix : CodeFixProvider
{
    private sealed class ReturnStatementRewriter : CSharpSyntaxRewriter
    {
        public ReturnStatementRewriter(string resultName, TypeSyntax returnType)
        {
            mResultName = resultName;
            mReturnType = returnType;
        }

        private readonly string mResultName;
        private readonly TypeSyntax mReturnType;

        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
        {
            var expression = node.Expression ?? SyntaxFactory.DefaultExpression(mReturnType);
            StatementSyntax assignment =
                SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind
                                                               .SimpleAssignmentExpression,
                                                           SyntaxFactory.IdentifierName(mResultName),
                                                           expression
                                                      )
                                                 );

            StatementSyntax gotoStatement =
                SyntaxFactory.GotoStatement(SyntaxKind.GotoStatement, SyntaxFactory.IdentifierName(ReturnLabelName));
            var replacement = SyntaxFactory.Block(assignment, gotoStatement);
            return replacement;
        }
    }

    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.STR0002];

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
        var node = root.FindNode(span);

        var methodDeclaration = node.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        var localFunction = node.AncestorsAndSelf().OfType<LocalFunctionStatementSyntax>().FirstOrDefault();

        if (methodDeclaration != null || localFunction != null)
        {
            CodeAction action = CodeAction.Create("Convert to single return",
                                                  cancellationToken =>
                                                      ConvertToSingleReturnAsync(document,
                                                               methodDeclaration,
                                                               localFunction,
                                                               cancellationToken
                                                          ),
                                                  nameof(Str0002MultipleReturnsCodeFix)
                                                 );

            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private static async Task<Document> ConvertToSingleReturnAsync(Document document,
                                                                   MethodDeclarationSyntax? methodDeclaration,
                                                                   LocalFunctionStatementSyntax? localFunction,
                                                                   CancellationToken cancellationToken)
    {
        var updatedDocument = document;
        var root =
            await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false) ??
            throw new InvalidOperationException();
        var body = GetBody(methodDeclaration, localFunction);
        var returnType = GetReturnType(methodDeclaration, localFunction);

        if (body != null && returnType != null)
        {
            string resultName = GetUniqueResultName(body);
            List<ReturnStatementSyntax> returns = body.DescendantNodes().OfType<ReturnStatementSyntax>().ToList();

            if (returns.Count >= 2)
            {
                StatementSyntax resultDeclaration =
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory
                                                                         .IdentifierName("var"),
                                                                     SyntaxFactory.SingletonSeparatedList(SyntaxFactory
                                                                             .VariableDeclarator(SyntaxFactory
                                                                                     .Identifier(resultName)
                                                                                 )
                                                                             .WithInitializer(SyntaxFactory
                                                                                     .EqualsValueClause(SyntaxFactory
                                                                                             .DefaultExpression(returnType
                                                                                                 )
                                                                                         )
                                                                                 )
                                                                         )
                                                                )
                                                           );

                StatementSyntax returnLabel =
                    SyntaxFactory.LabeledStatement(ReturnLabelName,
                                                   SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(resultName
                                                               )
                                                       )
                                                  );
                List<StatementSyntax> newStatements =
                    [
                        resultDeclaration
                    ];

                foreach(var statement in body.Statements)
                {
                    var updatedStatement = ReplaceReturns(statement, resultName, returnType);
                    newStatements.Add(updatedStatement);
                }

                newStatements.Add(returnLabel);
                var newBody = body.WithStatements(SyntaxFactory.List(newStatements));
                var newRoot = root.ReplaceNode(body, newBody);
                updatedDocument = document.WithSyntaxRoot(newRoot);
            }
        }

        return updatedDocument;
    }

    private static BlockSyntax? GetBody(MethodDeclarationSyntax? methodDeclaration,
                                        LocalFunctionStatementSyntax? localFunction)
    {
        BlockSyntax? result = null;

        if (methodDeclaration != null)
            result = methodDeclaration.Body;

        if (result == null)
        {
            if (localFunction != null)
                result = localFunction.Body;
        }

        return result;
    }

    private static TypeSyntax? GetReturnType(MethodDeclarationSyntax? methodDeclaration,
                                             LocalFunctionStatementSyntax? localFunction)
    {
        TypeSyntax? result = null;

        if (methodDeclaration != null)
            result = methodDeclaration.ReturnType;

        if (result == null)
        {
            if (localFunction != null)
                result = localFunction.ReturnType;
        }

        return result;
    }

    private static string GetUniqueResultName(BlockSyntax body)
    {
        string candidate = ResultVariableBaseName;
        HashSet<string> names =
            new HashSet<string>(body.DescendantNodes()
                                    .OfType<VariableDeclaratorSyntax>()
                                    .Select(variable => variable.Identifier.Text),
                                StringComparer.Ordinal
                               );
        string result = candidate;

        if (names.Contains(candidate))
        {
            var suffix = 1;

            while (names.Contains(result))
            {
                result = string.Concat(candidate, suffix.ToString());
                suffix = suffix + 1;
            }
        }

        return result;
    }

    private static StatementSyntax ReplaceReturns(StatementSyntax statement, string resultName, TypeSyntax returnType)
    {
        ReturnStatementRewriter rewriter = new ReturnStatementRewriter(resultName, returnType);
        var newNode = rewriter.Visit(statement);
        var updatedStatement = statement;

        if (newNode is StatementSyntax newStatement)
            updatedStatement = newStatement;

        return updatedStatement;
    }

    private const string ResultVariableBaseName = "result";
    private const string ReturnLabelName = "ReturnLabel";
}
