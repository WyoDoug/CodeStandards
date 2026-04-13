# CodeStructure.Analyzers

Roslyn analyzers and code fixes for the CodingStandards ruleset.

## Installation

Add the NuGet package to any project in your solution:
- `CodeStructure.Analyzers`

The analyzer and code-fix assemblies are delivered via the `analyzers/dotnet/cs` folder in the package.

## .editorconfig support (Visual Studio + ReSharper)

This package ships a `.editorconfig` and can copy it into the solution directory when a solution is present, otherwise the project directory. The file includes:
- Analyzer severities (`STR*`, `STY*`, `NUM*`, `ENC*`)
- C# and .NET formatting preferences used by Visual Studio
- ReSharper formatting and inspection preferences

### Customization

Copying is opt-in. Enable it and/or override the destination:

```xml
<PropertyGroup>
  <CodeStructureEditorConfigRoot>$(SolutionDir)</CodeStructureEditorConfigRoot>
  <CodeStructureEditorConfigCopyEnabled>true</CodeStructureEditorConfigCopyEnabled>
</PropertyGroup>
```

If `.editorconfig` already exists at the destination, the package does not overwrite it.
