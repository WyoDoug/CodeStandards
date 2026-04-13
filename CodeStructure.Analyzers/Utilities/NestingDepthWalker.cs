// // NestingDepthWalker.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace CodeStructure.Analyzers.Utilities;

public sealed class NestingDepthWalker : CSharpSyntaxWalker
{
    public NestingDepthWalker()
    {
        mDepth = 0;
    }

    public IReadOnlyList<NestingDepthResult> Results => mResults;
    private readonly List<NestingDepthResult> mResults = [];
    private int mDepth;

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        int previousDepth = mDepth;
        mDepth = 0;
        base.VisitMethodDeclaration(node);
        mDepth = previousDepth;
    }

    public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        int previousDepth = mDepth;
        mDepth = 0;
        base.VisitLocalFunctionStatement(node);
        mDepth = previousDepth;
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        VisitWithDepth(node, () => base.VisitIfStatement(node));
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        VisitWithDepth(node, () => base.VisitForStatement(node));
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        VisitWithDepth(node, () => base.VisitForEachStatement(node));
    }

    public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
    {
        VisitWithDepth(node, () => base.VisitForEachVariableStatement(node));
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        VisitWithDepth(node, () => base.VisitWhileStatement(node));
    }

    public override void VisitDoStatement(DoStatementSyntax node)
    {
        VisitWithDepth(node, () => base.VisitDoStatement(node));
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        VisitWithDepth(node, () => base.VisitSwitchStatement(node));
    }

    public override void VisitTryStatement(TryStatementSyntax node)
    {
        VisitWithDepth(node, () => base.VisitTryStatement(node));
    }

    public override void VisitUsingStatement(UsingStatementSyntax node)
    {
        VisitWithDepth(node, () => base.VisitUsingStatement(node));
    }

    public override void VisitLockStatement(LockStatementSyntax node)
    {
        VisitWithDepth(node, () => base.VisitLockStatement(node));
    }

    private void VisitWithDepth(CSharpSyntaxNode node, Action action)
    {
        mDepth = mDepth + 1;
        mResults.Add(new NestingDepthResult(node, mDepth));
        action.Invoke();
        mDepth = mDepth - 1;
    }
}
