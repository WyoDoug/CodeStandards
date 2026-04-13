// // RenameUtilities.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Rename;

#endregion

namespace CodeStructure.Analyzers.CodeFixes.Utilities;

public static class RenameUtilities
{
    public static async Task<Solution> RenameSymbolAsync(Document document,
                                                         ISymbol symbol,
                                                         string newName,
                                                         CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        SymbolRenameOptions options = new SymbolRenameOptions(RenameOverloads);
        var updatedSolution = await Renamer.RenameSymbolAsync(solution, symbol, options, newName, cancellationToken)
                                           .ConfigureAwait(continueOnCapturedContext: false);
        return updatedSolution;
    }

    private const bool RenameOverloads = true;
}
