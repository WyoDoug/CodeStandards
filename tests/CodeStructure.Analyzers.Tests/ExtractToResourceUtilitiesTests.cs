// // ExtractToResourceUtilitiesTests.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using CodeStructure.Analyzers.CodeFixes.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

#endregion

namespace CodeStructure.Analyzers.Tests;

public sealed class ExtractToResourceUtilitiesTests
{
    #region GenerateResourceKey Tests

    [Fact]
    public void GenerateResourceKey_SimpleWord_ReturnsPascalCase()
    {
        string result = ExtractToResourceUtilities.GenerateResourceKey("hello");

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void GenerateResourceKey_MultipleWords_ReturnsPascalCase()
    {
        string result = ExtractToResourceUtilities.GenerateResourceKey("hello world");

        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void GenerateResourceKey_WithSpecialCharacters_RemovesThem()
    {
        string result = ExtractToResourceUtilities.GenerateResourceKey("hello-world_test!");

        Assert.Equal("HelloWorldTest", result);
    }

    [Fact]
    public void GenerateResourceKey_StartsWithNumber_PrependsString()
    {
        string result = ExtractToResourceUtilities.GenerateResourceKey("123test");

        Assert.Equal("String123test", result);
    }

    [Fact]
    public void GenerateResourceKey_EmptyString_ReturnsEmptyValue()
    {
        string result = ExtractToResourceUtilities.GenerateResourceKey("");

        Assert.Equal("EmptyValue", result);
    }

    [Fact]
    public void GenerateResourceKey_LongString_TruncatesTo30Characters()
    {
        string result =
            ExtractToResourceUtilities
                .GenerateResourceKey("this is a very long string that should be truncated to thirty characters");

        Assert.True(result.Length <= 30);
    }

    #endregion

    #region AddResourceEntry Tests

    [Fact]
    public void AddResourceEntry_ValidResx_AddsEntry()
    {
        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""ExistingKey"" xml:space=""preserve"">
    <value>Existing Value</value>
  </data>
</root>";

        string result = ExtractToResourceUtilities.AddResourceEntry(resxContent, "NewKey", "New Value");

        Assert.Contains("NewKey", result);
        Assert.Contains("New Value", result);
        Assert.Contains("ExistingKey", result);
    }

    [Fact]
    public void AddResourceEntry_EmptyResx_AddsEntry()
    {
        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
</root>";

        string result = ExtractToResourceUtilities.AddResourceEntry(resxContent, "FirstKey", "First Value");

        Assert.Contains("FirstKey", result);
        Assert.Contains("First Value", result);
    }

    [Fact]
    public void AddResourceEntry_DuplicateKey_DoesNotAddDuplicate()
    {
        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""ExistingKey"" xml:space=""preserve"">
    <value>Existing Value</value>
  </data>
</root>";

        string result = ExtractToResourceUtilities.AddResourceEntry(resxContent, "ExistingKey", "Different Value");

        // Should still contain original value, not the new one
        Assert.Contains("Existing Value", result);
        // Count occurrences of ExistingKey - should only be one
        int count = result.Split("ExistingKey").Length - 1;
        Assert.Equal(expected: 1, count);
    }

    [Fact]
    public void AddResourceEntry_InvalidXml_ReturnsOriginal()
    {
        var invalidXml = "not valid xml at all";

        string result = ExtractToResourceUtilities.AddResourceEntry(invalidXml, "Key", "Value");

        Assert.Equal(invalidXml, result);
    }

    [Fact]
    public void AddResourceEntry_PreservesXmlSpace()
    {
        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
</root>";

        string result = ExtractToResourceUtilities.AddResourceEntry(resxContent, "TestKey", "Test Value");

        Assert.Contains("xml:space=\"preserve\"", result);
    }

    #endregion

    #region FindMatchingResourcesInResx Tests

    [Fact]
    public void FindMatchingResourcesInResx_ExactMatch_ReturnsMatch()
    {
        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""WelcomeMessage"" xml:space=""preserve"">
    <value>Welcome to our application</value>
  </data>
  <data name=""GoodbyeMessage"" xml:space=""preserve"">
    <value>Goodbye!</value>
  </data>
</root>";

        var resxDoc = CreateResxDocument("Resources.resx", resxContent);

        var matches =
            ExtractToResourceUtilities.FindMatchingResourcesInResx(resxContent, "Welcome to our application", resxDoc);

        Assert.Single(matches);
        Assert.Equal("WelcomeMessage", matches.First().Key);
        Assert.Equal("Resources", matches.First().ResourceClassName);
    }

    [Fact]
    public void FindMatchingResourcesInResx_NoMatch_ReturnsEmpty()
    {
        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""WelcomeMessage"" xml:space=""preserve"">
    <value>Welcome to our application</value>
  </data>
</root>";

        var resxDoc = CreateResxDocument("Resources.resx", resxContent);

        var matches =
            ExtractToResourceUtilities.FindMatchingResourcesInResx(resxContent, "This string does not exist", resxDoc);

        Assert.Empty(matches);
    }

    [Fact]
    public void FindMatchingResourcesInResx_MultipleMatches_ReturnsAll()
    {
        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""Message1"" xml:space=""preserve"">
    <value>Same Value</value>
  </data>
  <data name=""Message2"" xml:space=""preserve"">
    <value>Same Value</value>
  </data>
</root>";

        var resxDoc = CreateResxDocument("Resources.resx", resxContent);

        var matches = ExtractToResourceUtilities.FindMatchingResourcesInResx(resxContent, "Same Value", resxDoc);

        Assert.Equal(expected: 2, matches.Count());
    }

    [Fact]
    public void FindMatchingResourcesInResx_CaseSensitive_NoMatchOnDifferentCase()
    {
        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""Message"" xml:space=""preserve"">
    <value>Hello World</value>
  </data>
</root>";

        var resxDoc = CreateResxDocument("Resources.resx", resxContent);

        var matches = ExtractToResourceUtilities.FindMatchingResourcesInResx(resxContent, "hello world", resxDoc);

        Assert.Empty(matches);
    }

    #endregion

    #region FindAllResourceFiles Tests

    [Fact]
    public void FindAllResourceFiles_WithResxFiles_ReturnsFiles()
    {
        var project = CreateProjectWithAdditionalDocuments(("Resources.resx", "<root></root>"),
                                                           ("Strings.resx", "<root></root>"),
                                                           ("Resources.Designer.cs", "// generated")
                                                          );

        var resxFiles = ExtractToResourceUtilities.FindAllResourceFiles(project).ToList();

        Assert.Equal(expected: 2, resxFiles.Count);
        Assert.Contains(resxFiles, f => f.Name == "Resources.resx");
        Assert.Contains(resxFiles, f => f.Name == "Strings.resx");
    }

    [Fact]
    public void FindAllResourceFiles_NoResxFiles_ReturnsEmpty()
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);

        var resxFiles = ExtractToResourceUtilities.FindAllResourceFiles(project);

        Assert.Empty(resxFiles);
    }

    [Fact]
    public void FindResourceFile_MultipleResxFiles_ReturnsFirst()
    {
        var project = CreateProjectWithAdditionalDocuments(("Resources.resx", "<root></root>"),
                                                           ("Strings.resx", "<root></root>")
                                                          );

        var resxFile = ExtractToResourceUtilities.FindResourceFile(project);

        Assert.NotNull(resxFile);
    }

    #endregion

    #region FindExistingResourcesInProjectAsync Tests

    [Fact]
    public async Task FindExistingResourcesInProjectAsync_MatchExists_ReturnsMatch()
    {
        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""ErrorMessage"" xml:space=""preserve"">
    <value>An error occurred</value>
  </data>
</root>";

        var project = CreateProjectWithAdditionalDocuments(("Resources.resx", resxContent));

        var matches =
            await ExtractToResourceUtilities.FindExistingResourcesInProjectAsync(project,
                     "An error occurred",
                     CancellationToken.None
                );

        Assert.Single(matches);
        Assert.Equal("ErrorMessage", matches[index: 0].Key);
        Assert.Equal("An error occurred", matches[index: 0].Value);
    }

    [Fact]
    public async Task FindExistingResourcesInProjectAsync_NoMatch_ReturnsEmpty()
    {
        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""ErrorMessage"" xml:space=""preserve"">
    <value>An error occurred</value>
  </data>
</root>";

        var project = CreateProjectWithAdditionalDocuments(("Resources.resx", resxContent));

        var matches =
            await ExtractToResourceUtilities.FindExistingResourcesInProjectAsync(project,
                     "Some other string",
                     CancellationToken.None
                );

        Assert.Empty(matches);
    }

    [Fact]
    public async Task FindExistingResourcesInProjectAsync_SearchesAllResxFiles()
    {
        var resx1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""Message1"" xml:space=""preserve"">
    <value>Target String</value>
  </data>
</root>";

        var resx2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""Message2"" xml:space=""preserve"">
    <value>Target String</value>
  </data>
</root>";

        var project = CreateProjectWithAdditionalDocuments(("Resources.resx", resx1),
                                                           ("Strings.resx", resx2)
                                                          );

        var matches =
            await ExtractToResourceUtilities.FindExistingResourcesInProjectAsync(project,
                     "Target String",
                     CancellationToken.None
                );

        Assert.Equal(expected: 2, matches.Count);
    }

    #endregion

    #region ResourceMatch Tests

    [Fact]
    public void ResourceMatch_FromProject_GetAccessExpression_ReturnsCorrectFormat()
    {
        var resxDoc = CreateResxDocument("Strings.resx", "<root></root>");
        var match = new ResourceMatch("ErrorText", "Error occurred", "Strings", resxDoc);

        Assert.Equal("Strings.ErrorText", match.GetAccessExpression());
        Assert.False(match.IsFromAssembly);
    }

    [Fact]
    public void ResourceMatch_FromAssembly_GetAccessExpression_ReturnsCorrectFormat()
    {
        var match = new ResourceMatch("ErrorText", "Error occurred", "Resources", "CommonLib");

        Assert.Equal("CommonLib.Resources.ErrorText", match.GetAccessExpression());
        Assert.True(match.IsFromAssembly);
    }

    [Fact]
    public void ResourceMatch_FromProject_PropertiesAreCorrect()
    {
        var resxDoc = CreateResxDocument("Messages.resx", "<root></root>");
        var match = new ResourceMatch("HelloKey", "Hello World", "Messages", resxDoc);

        Assert.Equal("HelloKey", match.Key);
        Assert.Equal("Hello World", match.Value);
        Assert.Equal("Messages", match.ResourceClassName);
        Assert.Null(match.AssemblyName);
        Assert.NotNull(match.ResxDocument);
    }

    [Fact]
    public void ResourceMatch_FromAssembly_PropertiesAreCorrect()
    {
        var match = new ResourceMatch("HelloKey", "Hello World", "Resources", "SharedLib");

        Assert.Equal("HelloKey", match.Key);
        Assert.Equal("Hello World", match.Value);
        Assert.Equal("Resources", match.ResourceClassName);
        Assert.Equal("SharedLib", match.AssemblyName);
        Assert.Null(match.ResxDocument);
    }

    #endregion

    #region GetResourceClassName Tests

    [Fact]
    public void GetResourceClassName_SimpleFileName_ReturnsName()
    {
        var resxDoc = CreateResxDocument("Resources.resx", "<root></root>");

        string className = ExtractToResourceUtilities.GetResourceClassName(resxDoc);

        Assert.Equal("Resources", className);
    }

    [Fact]
    public void GetResourceClassName_DifferentFileName_ReturnsName()
    {
        var resxDoc = CreateResxDocument("ErrorMessages.resx", "<root></root>");

        string className = ExtractToResourceUtilities.GetResourceClassName(resxDoc);

        Assert.Equal("ErrorMessages", className);
    }

    #endregion

    #region Helper Methods

    private static TextDocument CreateResxDocument(string fileName, string content)
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var projectInfo = ProjectInfo.Create(projectId,
                                             VersionStamp.Default,
                                             "TestProject",
                                             "TestProject",
                                             LanguageNames.CSharp
                                            );
        var solution = workspace.CurrentSolution.AddProject(projectInfo);

        var docId = DocumentId.CreateNewId(projectId);
        solution = solution.AddAdditionalDocument(docId, fileName, SourceText.From(content));
        workspace.TryApplyChanges(solution);

        return workspace.CurrentSolution.GetAdditionalDocument(docId)!;
    }

    private static Project CreateProjectWithAdditionalDocuments(params (string fileName, string content)[] documents)
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var projectInfo = ProjectInfo.Create(projectId,
                                             VersionStamp.Default,
                                             "TestProject",
                                             "TestProject",
                                             LanguageNames.CSharp
                                            );
        var solution = workspace.CurrentSolution.AddProject(projectInfo);

        foreach((string fileName, string content) in documents)
        {
            var docId = DocumentId.CreateNewId(projectId);
            solution = solution.AddAdditionalDocument(docId, fileName, SourceText.From(content));
        }

        workspace.TryApplyChanges(solution);

        return workspace.CurrentSolution.GetProject(projectId)!;
    }

    #endregion
}
