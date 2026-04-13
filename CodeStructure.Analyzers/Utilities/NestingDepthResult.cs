// // NestingDepthResult.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using Microsoft.CodeAnalysis;

#endregion

namespace CodeStructure.Analyzers.Utilities;

public sealed class NestingDepthResult
{
    public NestingDepthResult(SyntaxNode node, int depth)
    {
        Node = node;
        Depth = depth;
    }

    public SyntaxNode Node { get; }

    public int Depth { get; }
}
