// // STY0008MagicStringsCodeFix.cs
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Sty0008MagicStringsCodeFix))]
public sealed class Sty0008MagicStringsCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.STY0008];

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
        var literalExpression = root.FindNode(span)
                                    .AncestorsAndSelf()
                                    .OfType<LiteralExpressionSyntax>()
                                    .FirstOrDefault();

        if (literalExpression == null)
            return;

        string stringValue = literalExpression.Token.ValueText;
        var project = document.Project;
        var cancellationToken = context.CancellationToken;

        // Count total occurrences for display
        int occurrenceCount = await ExtractToResourceUtilities
                                    .CountStringOccurrencesAsync(project, stringValue, cancellationToken)
                                    .ConfigureAwait(continueOnCapturedContext: false);

        // 1. Check for existing resources in project .resx files
        var existingProjectResources = await ExtractToResourceUtilities
                                             .FindExistingResourcesInProjectAsync(project,
                                                      stringValue,
                                                      cancellationToken
                                                 )
                                             .ConfigureAwait(continueOnCapturedContext: false);

        foreach(var existingResource in existingProjectResources)
        {
            // Use existing resource (single occurrence)
            context.RegisterCodeFix(CodeAction
                                        .Create($"Use existing '{existingResource.ResourceClassName}.{existingResource.Key}'",
                                                ct => ExtractToResourceUtilities.UseExistingResourceAsync(document,
                                                         existingResource,
                                                         literalExpression,
                                                         replaceAllOccurrences: false,
                                                         ct
                                                    ),
                                                $"{nameof(Sty0008MagicStringsCodeFix)}_UseExisting_{existingResource.Key}"
                                               ),
                                    diagnostic
                                   );

            // Use existing resource (all occurrences) - only if more than 1
            if (occurrenceCount > 1)
            {
                context.RegisterCodeFix(CodeAction
                                            .Create($"Use existing '{existingResource.ResourceClassName}.{existingResource.Key}' (replace all {occurrenceCount} occurrences)",
                                                    ct => ExtractToResourceUtilities.UseExistingResourceAsync(document,
                                                             existingResource,
                                                             literalExpression,
                                                             replaceAllOccurrences: true,
                                                             ct
                                                        ),
                                                    $"{nameof(Sty0008MagicStringsCodeFix)}_UseExistingAll_{existingResource.Key}"
                                                   ),
                                        diagnostic
                                       );
            }
        }

        // 2. Check for existing resources in referenced assemblies
        var compilation = await project.GetCompilationAsync(cancellationToken)
                                       .ConfigureAwait(continueOnCapturedContext: false);

        if (compilation != null)
        {
            var assemblyResources =
                ExtractToResourceUtilities.FindExistingResourcesInReferencedAssemblies(compilation, stringValue);

            foreach(var assemblyResource in assemblyResources)
            {
                // Use assembly resource (single occurrence)
                context.RegisterCodeFix(CodeAction
                                            .Create($"Use '{assemblyResource.GetAccessExpression()}' from {assemblyResource.AssemblyName}",
                                                    ct => ExtractToResourceUtilities.UseExistingResourceAsync(document,
                                                             assemblyResource,
                                                             literalExpression,
                                                             replaceAllOccurrences: false,
                                                             ct
                                                        ),
                                                    $"{nameof(Sty0008MagicStringsCodeFix)}_UseAssembly_{assemblyResource.AssemblyName}_{assemblyResource.Key}"
                                                   ),
                                        diagnostic
                                       );

                // Use assembly resource (all occurrences) - only if more than 1
                if (occurrenceCount > 1)
                {
                    context.RegisterCodeFix(CodeAction
                                                .Create($"Use '{assemblyResource.GetAccessExpression()}' (replace all {occurrenceCount} occurrences)",
                                                        ct =>
                                                            ExtractToResourceUtilities
                                                                .UseExistingResourceAsync(document,
                                                                         assemblyResource,
                                                                         literalExpression,
                                                                         replaceAllOccurrences: true,
                                                                         ct
                                                                    ),
                                                        $"{nameof(Sty0008MagicStringsCodeFix)}_UseAssemblyAll_{assemblyResource.AssemblyName}_{assemblyResource.Key}"
                                                       ),
                                            diagnostic
                                           );
                }
            }
        }

        // 3. Add to project resource file (if exists and string not already in resources)
        var resxDocument = ExtractToResourceUtilities.FindResourceFile(project);

        if (resxDocument != null && !existingProjectResources.Any())
        {
            string resourceKey = ExtractToResourceUtilities.GenerateResourceKey(stringValue);
            string resourceClassName = ExtractToResourceUtilities.GetResourceClassName(resxDocument);

            // Add to resources (single occurrence)
            context.RegisterCodeFix(CodeAction.Create($"Add to {resourceClassName} as '{resourceKey}'",
                                                      ct => ExtractToResourceUtilities.ExtractToResourceAsync(document,
                                                               resxDocument,
                                                               literalExpression,
                                                               resourceKey,
                                                               replaceAllOccurrences: false,
                                                               ct
                                                          ),
                                                      $"{nameof(Sty0008MagicStringsCodeFix)}_AddResource"
                                                     ),
                                    diagnostic
                                   );

            // Add to resources (all occurrences) - only if more than 1
            if (occurrenceCount > 1)
            {
                context.RegisterCodeFix(CodeAction
                                            .Create($"Add to {resourceClassName} as '{resourceKey}' (replace all {occurrenceCount} occurrences)",
                                                    ct => ExtractToResourceUtilities.ExtractToResourceAsync(document,
                                                             resxDocument,
                                                             literalExpression,
                                                             resourceKey,
                                                             replaceAllOccurrences: true,
                                                             ct
                                                        ),
                                                    $"{nameof(Sty0008MagicStringsCodeFix)}_AddResourceAll"
                                                   ),
                                        diagnostic
                                       );
            }
        }

        // 4. Extract to constant (always available as fallback)
        string constantName = ExtractConstantUtilities.GenerateConstantNameFromString(stringValue);

        context.RegisterCodeFix(CodeAction.Create($"Extract to constant '{constantName}'",
                                                  ct => ExtractConstantUtilities.ExtractToConstantAsync(document,
                                                           literalExpression,
                                                           constantName,
                                                           ct
                                                      ),
                                                  $"{nameof(Sty0008MagicStringsCodeFix)}_Constant"
                                                 ),
                                diagnostic
                               );
    }
}
