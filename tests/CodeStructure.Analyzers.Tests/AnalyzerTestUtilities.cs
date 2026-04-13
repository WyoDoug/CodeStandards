// // AnalyzerTestUtilities.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

#endregion

namespace CodeStructure.Analyzers.Tests;

public static class AnalyzerTestUtilities
{
    public static async Task VerifyAnalyzerAsync<TAnalyzer>(string source, params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>();
        test.TestCode = source;
        test.ReferenceAssemblies = GetReferenceAssemblies();

        foreach(var diagnostic in expected)
            test.ExpectedDiagnostics.Add(diagnostic);

        await test.RunAsync(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
    }

    public static async Task VerifyAnalyzerWithConfigAsync<TAnalyzer>(string source,
                                                                      string editorConfig,
                                                                      params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>();
        test.TestCode = source;
        test.ReferenceAssemblies = GetReferenceAssemblies();
        test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", editorConfig));

        foreach(var diagnostic in expected)
            test.ExpectedDiagnostics.Add(diagnostic);

        await test.RunAsync(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
    }

    public static async Task VerifyAnalyzerWithOutputKindAsync<TAnalyzer>(string source,
                                                                          OutputKind outputKind,
                                                                          params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>();
        test.TestCode = source;
        test.ReferenceAssemblies = GetReferenceAssemblies();
        test.TestState.OutputKind = outputKind;

        foreach(var diagnostic in expected)
            test.ExpectedDiagnostics.Add(diagnostic);

        await test.RunAsync(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
    }

    public static async Task VerifyCodeFixAsync<TAnalyzer, TCodeFix>(string source,
                                                                     string fixedSource,
                                                                     params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test =
            new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>();
        test.TestCode = source;
        test.FixedCode = fixedSource;
        test.ReferenceAssemblies = GetReferenceAssemblies();

        foreach(var diagnostic in expected)
            test.ExpectedDiagnostics.Add(diagnostic);

        await test.RunAsync(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    ///     Verifies a code fix with additional documents (e.g., .resx files).
    /// </summary>
    public static async Task VerifyCodeFixWithAdditionalDocumentsAsync<TAnalyzer, TCodeFix>(string source,
        string fixedSource,
        (string fileName, string content)[] additionalDocuments,
        (string fileName, string content)[]? fixedAdditionalDocuments,
        int codeFixIndex,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test =
            new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>();
        test.TestCode = source;
        test.FixedCode = fixedSource;
        test.ReferenceAssemblies = GetReferenceAssemblies();
        test.CodeActionIndex = codeFixIndex;

        // Disable fix-all validation for tests with additional documents
        // as they require more complex setup
        test.NumberOfFixAllIterations = 0;
        test.NumberOfFixAllInDocumentIterations = 0;
        test.NumberOfFixAllInProjectIterations = 0;

        foreach((string fileName, string content) in additionalDocuments)
            test.TestState.AdditionalFiles.Add((fileName, content));

        if (fixedAdditionalDocuments != null)
        {
            foreach((string fileName, string content) in fixedAdditionalDocuments)
                test.FixedState.AdditionalFiles.Add((fileName, content));
        }
        else
        {
            // If no fixed additional documents specified, use the same as input
            foreach((string fileName, string content) in additionalDocuments)
                test.FixedState.AdditionalFiles.Add((fileName, content));
        }

        foreach(var diagnostic in expected)
            test.ExpectedDiagnostics.Add(diagnostic);

        await test.RunAsync(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
    }

    public static DiagnosticResult CreateResult(DiagnosticDescriptor descriptor, int line, int column)
    {
        var result = new DiagnosticResult(descriptor).WithLocation(line, column);
        return result;
    }

    public static DiagnosticResult CreateResult(DiagnosticDescriptor descriptor, int locationIndex)
    {
        var result = new DiagnosticResult(descriptor).WithLocation(locationIndex);
        return result;
    }

    public static DiagnosticResult CreateResult(DiagnosticDescriptor descriptor,
                                                int line,
                                                int column,
                                                params object[] messageArguments)
    {
        var result = new DiagnosticResult(descriptor).WithLocation(line, column).WithArguments(messageArguments);
        return result;
    }

    public static DiagnosticResult CreateResult(DiagnosticDescriptor descriptor,
                                                int locationIndex,
                                                params object[] messageArguments)
    {
        var result = new DiagnosticResult(descriptor).WithLocation(locationIndex).WithArguments(messageArguments);
        return result;
    }

    /// <summary>
    ///     Verifies that the analyzer produces no diagnostics when running in a test assembly context.
    ///     Adds xUnit references to simulate a test project.
    /// </summary>
    public static async Task VerifyAnalyzerInTestAssemblyAsync<TAnalyzer>(string source,
                                                                          params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>();
        test.TestCode = source;
        test.ReferenceAssemblies = GetTestAssemblyReferences();

        foreach(var diagnostic in expected)
            test.ExpectedDiagnostics.Add(diagnostic);

        await test.RunAsync(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
    }

    private static ReferenceAssemblies GetReferenceAssemblies()
    {
        var referenceAssemblies = ReferenceAssemblies.Net.Net80;
        string? nugetConfigPath = FindNuGetConfigPath();

        if (!string.IsNullOrWhiteSpace(nugetConfigPath))
            referenceAssemblies = referenceAssemblies.WithNuGetConfigFilePath(nugetConfigPath);

        return referenceAssemblies;
    }

    private static ReferenceAssemblies GetTestAssemblyReferences()
    {
        var referenceAssemblies = ReferenceAssemblies.Net.Net80
                                                     .AddPackages([new PackageIdentity("xunit", "2.6.1")]);

        string? nugetConfigPath = FindNuGetConfigPath();

        if (!string.IsNullOrWhiteSpace(nugetConfigPath))
            referenceAssemblies = referenceAssemblies.WithNuGetConfigFilePath(nugetConfigPath);

        return referenceAssemblies;
    }

    private static string? FindNuGetConfigPath()
    {
        string? result = null;
        string currentPath = AppContext.BaseDirectory;
        var currentDirectory = new DirectoryInfo(currentPath);

        while (currentDirectory != null && result == null)
        {
            string candidate = Path.Combine(currentDirectory.FullName, "NuGet.config");

            if (File.Exists(candidate))
                result = candidate;
            else
                currentDirectory = currentDirectory.Parent;
        }

        return result;
    }
}
