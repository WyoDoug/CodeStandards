// // ExtractToResourceUtilities.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

#endregion

namespace CodeStructure.Analyzers.CodeFixes.Utilities;

/// <summary>
///     Represents a resource entry found in a .resx file or referenced assembly.
/// </summary>
internal sealed class ResourceMatch
{
    public ResourceMatch(string key, string value, string resourceClassName, TextDocument resxDocument)
    {
        Key = key;
        Value = value;
        ResourceClassName = resourceClassName;
        ResxDocument = resxDocument;
        AssemblyName = null;
    }

    public ResourceMatch(string key, string value, string resourceClassName, string assemblyName)
    {
        Key = key;
        Value = value;
        ResourceClassName = resourceClassName;
        AssemblyName = assemblyName;
        ResxDocument = null;
    }

    public string Key { get; }
    public string Value { get; }
    public string ResourceClassName { get; }
    public string? AssemblyName { get; }
    public TextDocument? ResxDocument { get; }
    public bool IsFromAssembly => AssemblyName != null;

    public string GetAccessExpression()
    {
        if (IsFromAssembly)
            return $"{AssemblyName}.{ResourceClassName}.{Key}";

        return $"{ResourceClassName}.{Key}";
    }
}

internal static class ExtractToResourceUtilities
{
    #region Find All String Occurrences

    /// <summary>
    ///     Finds all occurrences of the same string literal in the project.
    /// </summary>
    internal static async Task<List<(Document Document, LiteralExpressionSyntax Literal)>>
        FindAllStringOccurrencesAsync(Project project,
                                      string searchValue,
                                      CancellationToken cancellationToken)
    {
        var occurrences = new List<(Document, LiteralExpressionSyntax)>();

        foreach(var document in project.Documents)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                                     .ConfigureAwait(continueOnCapturedContext: false);

            if (root == null)
                continue;

            var literals = root.DescendantNodes()
                               .OfType<LiteralExpressionSyntax>()
                               .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression) &&
                                           string.Equals(l.Token.ValueText, searchValue, StringComparison.Ordinal)
                                     );

            foreach(var literal in literals)
                occurrences.Add((document, literal));
        }

        return occurrences;
    }

    #endregion

    #region Find Resource Files

    internal static IEnumerable<TextDocument> FindAllResourceFiles(Project project)
    {
        return project.AdditionalDocuments
                      .Where(d => d.Name.EndsWith(".resx", StringComparison.OrdinalIgnoreCase) &&
                                  !d.Name.Contains(".Designer.")
                            );
    }

    internal static TextDocument? FindResourceFile(Project project)
    {
        return FindAllResourceFiles(project).FirstOrDefault();
    }

    internal static string GetResourceClassName(TextDocument resxDocument)
    {
        string fileName = Path.GetFileNameWithoutExtension(resxDocument.Name);

        return fileName;
    }

    internal static string GetResourceNamespace(TextDocument resxDocument, Project project)
    {
        string? folder = Path.GetDirectoryName(resxDocument.Name);

        if (!string.IsNullOrEmpty(folder))
            return $"{project.AssemblyName}.{folder.Replace(Path.DirectorySeparatorChar, newChar: '.')}";

        return project.AssemblyName ?? "Resources";
    }

    #endregion

    #region Scan for Existing Resources

    /// <summary>
    ///     Scans all .resx files in the project for an existing resource with the given value.
    /// </summary>
    internal static async Task<List<ResourceMatch>> FindExistingResourcesInProjectAsync(Project project,
        string searchValue,
        CancellationToken cancellationToken)
    {
        var matches = new List<ResourceMatch>();
        var resxFiles = FindAllResourceFiles(project);

        foreach(var resxDoc in resxFiles)
        {
            var text = await resxDoc.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            var resourceMatches = FindMatchingResourcesInResx(text.ToString(), searchValue, resxDoc);
            matches.AddRange(resourceMatches);
        }

        return matches;
    }

    /// <summary>
    ///     Parses a .resx file and finds any resources with a value matching the search string.
    /// </summary>
    internal static IEnumerable<ResourceMatch> FindMatchingResourcesInResx(string resxContent,
                                                                           string searchValue,
                                                                           TextDocument resxDocument)
    {
        var matches = new List<ResourceMatch>();

        try
        {
            var doc = XDocument.Parse(resxContent);
            var root = doc.Root;

            if (root == null)
                return matches;

            string resourceClassName = GetResourceClassName(resxDocument);

            foreach(var dataElement in root.Elements("data"))
            {
                var nameAttr = dataElement.Attribute("name");
                var valueElement = dataElement.Element("value");

                if (nameAttr != null && valueElement != null)
                {
                    string value = valueElement.Value;

                    if (string.Equals(value, searchValue, StringComparison.Ordinal))
                        matches.Add(new ResourceMatch(nameAttr.Value, value, resourceClassName, resxDocument));
                }
            }
        }
        catch
        {
            // If XML parsing fails, return empty list
        }

        return matches;
    }

    /// <summary>
    ///     Scans referenced assemblies for string resources matching the given value.
    /// </summary>
    internal static List<ResourceMatch> FindExistingResourcesInReferencedAssemblies(Compilation compilation,
        string searchValue)
    {
        var matches = new List<ResourceMatch>();

        foreach(var reference in compilation.References.OfType<PortableExecutableReference>())
        {
            if (reference.FilePath == null)
                continue;

            try
            {
                var assemblyMatches = ScanAssemblyForResources(reference.FilePath, searchValue);
                matches.AddRange(assemblyMatches);
            }
            catch
            {
                // Skip assemblies that can't be loaded
            }
        }

        return matches;
    }

    private static IEnumerable<ResourceMatch> ScanAssemblyForResources(string assemblyPath, string searchValue)
    {
        var matches = new List<ResourceMatch>();

        try
        {
            // Load assembly in reflection-only context to avoid execution
            var assembly = Assembly.LoadFrom(assemblyPath);
            string assemblyName = assembly.GetName().Name ?? Path.GetFileNameWithoutExtension(assemblyPath);

            // Get all embedded resource names
            var resourceNames = assembly.GetManifestResourceNames()
                                        .Where(n => n.EndsWith(".resources", StringComparison.OrdinalIgnoreCase));

            foreach(string? resourceName in resourceNames)
            {
                try
                {
                    // Extract class name from resource name (e.g., "Acme.Properties.Resources.resources" -> "Resources")
                    string baseName = resourceName.Substring(startIndex: 0, resourceName.Length - ".resources".Length);
                    string className = baseName.Contains(value: '.')
                                           ? baseName.Substring(baseName.LastIndexOf(value: '.') + 1)
                                           : baseName;

                    using var stream = assembly.GetManifestResourceStream(resourceName);

                    if (stream == null)
                        continue;

                    using var reader = new ResourceReader(stream);

                    foreach(DictionaryEntry entry in reader)
                    {
                        if (entry.Value is string stringValue &&
                            string.Equals(stringValue, searchValue, StringComparison.Ordinal))
                        {
                            string key = entry.Key?.ToString() ?? "";
                            matches.Add(new ResourceMatch(key, stringValue, className, assemblyName));
                        }
                    }
                }
                catch
                {
                    // Skip resources that can't be read
                }
            }
        }
        catch
        {
            // Skip assemblies that can't be loaded
        }

        return matches;
    }

    #endregion

    #region Extract to Resource

    /// <summary>
    ///     Extracts a string to a new resource entry and replaces all occurrences in the project.
    /// </summary>
    internal static async Task<Solution> ExtractToResourceAsync(Document document,
                                                                TextDocument resxDocument,
                                                                LiteralExpressionSyntax literalExpression,
                                                                string resourceKey,
                                                                bool replaceAllOccurrences,
                                                                CancellationToken cancellationToken)
    {
        string stringValue = literalExpression.Token.ValueText;
        string resourceClassName = GetResourceClassName(resxDocument);
        var solution = document.Project.Solution;

        // Build the resource access expression: Resources.ResourceKey
        var resourceAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                  SyntaxFactory.IdentifierName(resourceClassName),
                                                                  SyntaxFactory.IdentifierName(resourceKey)
                                                                 );

        if (replaceAllOccurrences)
        {
            // Find all occurrences across the project
            var allOccurrences = await FindAllStringOccurrencesAsync(document.Project, stringValue, cancellationToken)
                                     .ConfigureAwait(continueOnCapturedContext: false);

            // Group by document to batch replacements
            var byDocument = allOccurrences.GroupBy(o => o.Document.Id);

            foreach(var group in byDocument)
            {
                var doc = solution.GetDocument(group.Key);

                if (doc == null)
                    continue;

                var root = await doc.GetSyntaxRootAsync(cancellationToken)
                                    .ConfigureAwait(continueOnCapturedContext: false);

                if (root == null)
                    continue;

                var literalsInDoc = group.Select(g => g.Literal).ToList();

                // Replace all literals in this document
                var newRoot = root.ReplaceNodes(literalsInDoc,
                                                (original, rewritten) => resourceAccess.WithTriviaFrom(original)
                                               );

                solution = solution.WithDocumentSyntaxRoot(doc.Id, newRoot);
            }
        }
        else
        {
            // Replace only the single occurrence
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                                     .ConfigureAwait(continueOnCapturedContext: false);

            if (root != null)
            {
                var newRoot = root.ReplaceNode(literalExpression, resourceAccess.WithTriviaFrom(literalExpression));
                solution = solution.WithDocumentSyntaxRoot(document.Id, newRoot);
            }
        }

        // Update the resx file with the new entry
        var resxText = await resxDocument.GetTextAsync(cancellationToken)
                                         .ConfigureAwait(continueOnCapturedContext: false);
        string updatedResxContent = AddResourceEntry(resxText.ToString(), resourceKey, stringValue);
        // Preserve original encoding
        var newResxText = SourceText.From(updatedResxContent, resxText.Encoding);
        solution = solution.WithAdditionalDocumentText(resxDocument.Id, newResxText);

        return solution;
    }

    /// <summary>
    ///     Uses an existing resource from a .resx file and replaces occurrences.
    /// </summary>
    internal static async Task<Solution> UseExistingResourceAsync(Document document,
                                                                  ResourceMatch existingResource,
                                                                  LiteralExpressionSyntax literalExpression,
                                                                  bool replaceAllOccurrences,
                                                                  CancellationToken cancellationToken)
    {
        string stringValue = literalExpression.Token.ValueText;
        var solution = document.Project.Solution;

        // Build the resource access expression
        ExpressionSyntax resourceAccess;

        if (existingResource.IsFromAssembly)
        {
            // For assembly resources: AssemblyName.Resources.Key
            resourceAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                  SyntaxFactory.MemberAccessExpression(SyntaxKind
                                                                               .SimpleMemberAccessExpression,
                                                                           SyntaxFactory.IdentifierName(existingResource
                                                                                    .AssemblyName!
                                                                               ),
                                                                           SyntaxFactory.IdentifierName(existingResource
                                                                                   .ResourceClassName
                                                                               )
                                                                      ),
                                                                  SyntaxFactory.IdentifierName(existingResource.Key)
                                                                 );
        }
        else
        {
            // For project resources: Resources.Key
            resourceAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                  SyntaxFactory.IdentifierName(existingResource
                                                                          .ResourceClassName
                                                                      ),
                                                                  SyntaxFactory.IdentifierName(existingResource.Key)
                                                                 );
        }

        if (replaceAllOccurrences)
        {
            var allOccurrences = await FindAllStringOccurrencesAsync(document.Project, stringValue, cancellationToken)
                                     .ConfigureAwait(continueOnCapturedContext: false);

            var byDocument = allOccurrences.GroupBy(o => o.Document.Id);

            foreach(var group in byDocument)
            {
                var doc = solution.GetDocument(group.Key);

                if (doc == null)
                    continue;

                var root = await doc.GetSyntaxRootAsync(cancellationToken)
                                    .ConfigureAwait(continueOnCapturedContext: false);

                if (root == null)
                    continue;

                var literalsInDoc = group.Select(g => g.Literal).ToList();

                var newRoot = root.ReplaceNodes(literalsInDoc,
                                                (original, rewritten) => resourceAccess.WithTriviaFrom(original)
                                               );

                solution = solution.WithDocumentSyntaxRoot(doc.Id, newRoot);
            }
        }
        else
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                                     .ConfigureAwait(continueOnCapturedContext: false);

            if (root != null)
            {
                var newRoot = root.ReplaceNode(literalExpression, resourceAccess.WithTriviaFrom(literalExpression));
                solution = solution.WithDocumentSyntaxRoot(document.Id, newRoot);
            }
        }

        return solution;
    }

    #endregion

    #region Helper Methods

    internal static string AddResourceEntry(string resxContent, string key, string value)
    {
        try
        {
            var doc = XDocument.Parse(resxContent);
            var root = doc.Root;

            if (root == null)
                return resxContent;

            // Check if key already exists
            var existingData = root.Elements("data")
                                   .FirstOrDefault(d => d.Attribute("name")?.Value == key);

            if (existingData != null)
                return resxContent; // Key already exists, don't add duplicate

            // Create new data element
            var dataElement = new XElement("data",
                                           new XAttribute("name", key),
                                           new XAttribute(XNamespace.Xml + "space", "preserve"),
                                           new XElement("value", value)
                                          );

            // Find the last data element and add after it, or add at the end
            var lastData = root.Elements("data").LastOrDefault();

            if (lastData != null)
                lastData.AddAfterSelf(dataElement);
            else
                root.Add(dataElement);

            return doc.ToString();
        }
        catch
        {
            return resxContent;
        }
    }

    internal static string GenerateResourceKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "EmptyValue";

        var result = new StringBuilder();
        var capitalizeNext = true;

        foreach(char c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                if (capitalizeNext)
                {
                    result.Append(char.ToUpperInvariant(c));
                    capitalizeNext = false;
                }
                else
                    result.Append(c);
            }
            else
                capitalizeNext = true;
        }

        var name = result.ToString();

        if (name.Length == 0 || !char.IsLetter(name[index: 0]))
            name = "String" + name;

        if (name.Length > 30)
            name = name.Substring(startIndex: 0, length: 30);

        return name;
    }

    /// <summary>
    ///     Counts how many occurrences of a string exist in the project.
    /// </summary>
    internal static async Task<int> CountStringOccurrencesAsync(Project project,
                                                                string searchValue,
                                                                CancellationToken cancellationToken)
    {
        var occurrences = await FindAllStringOccurrencesAsync(project, searchValue, cancellationToken)
                              .ConfigureAwait(continueOnCapturedContext: false);

        return occurrences.Count;
    }

    #endregion
}
