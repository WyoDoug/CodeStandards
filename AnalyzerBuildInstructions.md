# Code Analyzer Build Specification

This document provides specifications for building a Roslyn-based code analyzer. Use this as instructions for an AI assistant (Claude) to implement the analyzer.

**License:** MIT

---

## Project Overview

Create a Roslyn analyzer NuGet package called `CodeStructure.Analyzers` that enforces 23 coding standard rules across 4 categories.

### Technology Stack
- .NET Standard 2.0 (for analyzer compatibility)
- Microsoft.CodeAnalysis.CSharp 4.8.0+
- Microsoft.CodeAnalysis.CSharp.Workspaces 4.8.0+ (for code fixes)

### Solution Structure
```
CodeStructure.Analyzers/
├── src/
│   ├── CodeStructure.Analyzers/           # Analyzer project
│   └── CodeStructure.Analyzers.CodeFixes/ # Code fix providers
├── tests/
│   └── CodeStructure.Analyzers.Tests/     # Unit tests
└── package/
    └── CodeStructure.Analyzers.Package/   # NuGet packaging
```

---

## Rule Definitions

### Category: Structure (STR)

| ID | Name | Severity | Code Fix | Description |
|----|------|----------|----------|-------------|
| STR0001 | No continue statements | Warning | No | Flag all `continue` statements in loops |
| STR0002 | Multiple return statements | Warning | Yes | Methods must have exactly one return at the end |
| STR0003 | Return in void method | Warning | Yes | Void methods should not use `return;` |
| STR0004 | If-else chains | Warning | Yes | Flag if-else chains with more than 3 branches |
| STR0005 | Return in nested block | Warning | Yes | Flag returns inside loops, if statements, try/catch |
| STR0006 | No else if | Warning | Yes | Flag all `else if` constructs |
| STR0007 | Suspicious regions | Warning | No | Flag regions named "hack", "temp", "todo", etc. |
| STR0008 | One type per file | Warning | No | Flag files containing multiple type declarations |
| STR0009 | Nesting depth exceeded | Warning | No | Flag nesting > 3 levels (warning), > 6 levels (error) |
| STR0010 | Missing argument validation | Warning | No | Flag public methods without null/validation checks |

### Category: Style (STY)

| ID | Name | Severity | Code Fix | Description |
|----|------|----------|----------|-------------|
| STY0001 | End-of-line comments | Warning | Yes | Flag comments at end of code lines |
| STY0002 | Null-forgiving operator | Warning | No | Flag use of `!` operator |
| STY0003 | Avoid dynamic keyword | Warning | No | Flag use of `dynamic` type |
| STY0004 | Field naming convention | Warning | Yes | Enforce m/ps/sm/pm prefixes |
| STY0005 | Non-nullable string = null | Warning | Yes | Flag `string x = null;` |
| STY0006 | Method naming convention | Warning | Yes | Enforce PascalCase for methods |
| STY0007 | Region naming convention | Warning | No | Flag generic region names like "Properties", "Methods" |

### Category: Numeric (NUM)

| ID | Name | Severity | Code Fix | Description |
|----|------|----------|----------|-------------|
| NUM0001 | Floating-point equality | **Error** | No | Flag `==` or `!=` with float/double operands |
| NUM0002 | Magic numbers | Warning | No | Flag numeric literals not in allowed list |

### Category: Encapsulation (ENC)

| ID | Name | Severity | Code Fix | Description |
|----|------|----------|----------|-------------|
| ENC0001 | Avoid hiding with 'new' | Warning | Yes | Flag `new` modifier on members |
| ENC0002 | Direct inherited field access | Warning | No | Flag access to non-readonly inherited fields |
| ENC0003 | Public/protected fields | Warning | Yes | Flag non-private fields (with exemptions) |
| ENC0004 | Interface I prefix | Warning | Yes | Flag interfaces not starting with "I" |

---

## Detailed Rule Specifications

### STR0001: No Continue Statements
- **Trigger:** Any `ContinueStatementSyntax`
- **Message:** "Avoid using continue statements; restructure the logic instead"
- **No exemptions**

### STR0002: Multiple Return Statements
- **Trigger:** Method/local function with more than one `ReturnStatementSyntax`
- **Message:** "Method has multiple return statements; use a single return at the end"
- **Report on:** All returns except the last one
- **Code Fix:** Convert to single-return pattern using result variable

### STR0003: Return in Void Method
- **Trigger:** `return;` (no expression) inside void method
- **Message:** "Void methods should not use return statements"
- **Code Fix:** Invert condition and remove return

### STR0004: If-Else Chains
- **Trigger:** If statement with > 3 branches (including final else)
- **Message:** "If-else chain has {0} branches; consider using switch expression"
- **Only report on:** Top-level if (not nested else-if)
- **Code Fix:** Convert to switch expression

### STR0005: Return in Nested Block
- **Trigger:** Return inside: for, foreach, while, do-while, if, switch case, try, catch, finally
- **Message:** "Return statement inside {0}; use a result variable instead"
- **Code Fix:** Introduce result variable

### STR0006: No Else If
- **Trigger:** `ElseClauseSyntax` where statement is `IfStatementSyntax`
- **Message:** "Avoid else-if chains; use switch expressions or pattern matching"
- **Code Fix:** Convert to switch expression

### STR0007: Suspicious Regions
- **Trigger:** Region directive with name containing: hack, workaround, todo, fixme, temp, temporary, delete, remove, deprecated, obsolete, broken, bug, ugly, bad
- **Message:** "Region '{0}' has a suspicious name"
- **Case insensitive matching**

### STR0008: One Type Per File
- **Trigger:** File containing multiple top-level type declarations
- **Message:** "File contains multiple types; each type should be in its own file"
- **Exemption:** Nested types are allowed

### STR0009: Nesting Depth Exceeded
- **Trigger:** Block with nesting depth > 3 (warning) or > 6 (error)
- **Count:** if, for, foreach, while, do, switch, try, using, lock
- **Message:** "Nesting depth of {0} exceeds the limit of {1}"
- **Stop counting at:** Method/local function boundary

### STR0010: Missing Argument Validation
- **Trigger:** Public method with reference type parameters but no null checks or argument validation
- **Message:** "Public method '{0}' does not validate parameter '{1}'"
- **Detection:** Look for `if (param == null)`, `ArgumentNullException`, `is null` checks, or `ThrowIfNull`
- **Exemption:** Parameters with default values, nullable types (`?`)

---

### STY0001: End-of-Line Comments
- **Trigger:** `SingleLineCommentTrivia` in trailing trivia of a token (not on its own line)
- **Message:** "Move comment to line above"
- **Code Fix:** Move comment to separate line above the code

### STY0002: Null-Forgiving Operator
- **Trigger:** `SuppressNullableWarningExpression` (the `!` postfix)
- **Message:** "Avoid null-forgiving operator; use proper null checks"

### STY0003: Avoid Dynamic
- **Trigger:** `IdentifierNameSyntax` with text "dynamic" in type position
- **Message:** "Avoid dynamic keyword; use strong typing"
- **Exemption:** COM interop scenarios (check for `[ComImport]` on containing type)

### STY0004: Field Naming Convention
- **Trigger:** Field not matching required prefix pattern
- **Prefixes:**
  - Private instance: `m` + PascalCase (e.g., `mCustomerName`)
  - Private static: `ps` + PascalCase (e.g., `psInstanceCount`)
  - Private static readonly: `sm` + PascalCase (e.g., `smDefaultLogger`)
  - Public/protected/internal instance: `pm` + PascalCase (e.g., `pmDisplayName`)
- **Message:** "Field '{0}' should be named '{1}'"
- **Exemption:** `const` fields (they use PascalCase without prefix)
- **Code Fix:** Rename field with correct prefix

### STY0005: Non-Nullable String Initialized to Null
- **Trigger:** `string identifier = null;` (non-nullable string type)
- **Message:** "Non-nullable string should not be initialized to null"
- **Code Fix Options:**
  1. Change to `string.Empty`
  2. Change to `string?` (nullable)

### STY0006: Method Naming Convention
- **Trigger:** Method name not PascalCase
- **Message:** "Method '{0}' should use PascalCase naming"
- **Detection:** First character not uppercase, or contains underscore
- **Exemption:** Methods with `[DllImport]`, `[LibraryImport]` attributes
- **Code Fix:** Rename to PascalCase

### STY0007: Region Naming Convention
- **Trigger:** Region with generic names: "Constructors", "Properties", "Methods", "Fields", "Events", "Private Members", "Public Members"
- **Message:** "Region name '{0}' is too generic; use feature-based names"
- **Allowed patterns:** "[Name] property", "[Name] command", "[Name] event", "Equality members", "IDisposable implementation"

---

### NUM0001: Floating-Point Equality
- **Severity:** ERROR (not warning)
- **Trigger:** `==` or `!=` where either operand is `float` or `double`
- **Message:** "Direct equality comparison of floating-point values is unreliable"
- **Exemptions:**
  - Test methods (`[TestMethod]`, `[Fact]`, `[Theory]`, `[Test]`, `[TestCase]`)
  - Comparison with integer literal that converts exactly (0, 1, -1)

### NUM0002: Magic Numbers
- **Trigger:** Numeric literal not in allowed list
- **Message:** "Magic number '{0}' should be extracted to a named constant"

**Allowed Integers:**
```
0, 1, -10 to 20, 24, 25, 30, 40, 50, 75,
45, 60, 90, 120, 135, 180, 225, 270, 315, 360,
32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768,
255, 65535, 0x7FFFFFFF,
1000, 2000, 3000, 5000, 60000, 120000, 3600, 86400,
-10000, -1000, -100, 100, 10000, 100000, 1000000,
500, 8000, 260, 365, 366, 23, 31, 37, 397
```

**Allowed Floats:**
```
0.0, 1.0, -0.5, -0.25, -0.1, -0.01,
0.01, 0.02, 0.05, 0.1, 0.2, 0.25, 0.33, 0.5, 0.67, 0.75,
0.8, 0.85, 0.9, 0.95, 0.99, 0.999,
-10.0 to 10.0 (integers as doubles),
-1000.0, -100.0, 100.0, 1000.0,
45.0, 60.0, 90.0, 180.0, 270.0, 360.0,
1e-10, 1e-9, 1e-8, 1e-7, 1e-6, 1e-5, 1e-4, 1e-3,
3.14159, 3.141592653589793, 6.283185307179586
```

**Exemptions:**
- `const` declarations
- `static readonly` fields
- Enum values
- Attribute arguments
- Array/collection initializers
- Test methods
- `GetHashCode()` methods
- Hex literals (`0x` prefix)

---

### ENC0001: Avoid Hiding with 'new'
- **Trigger:** Member with `new` modifier
- **Message:** "Member '{0}' hides inherited member; use override if base is virtual"
- **Code Fix:** If base member is virtual, change `new` to `override`

### ENC0002: Direct Inherited Field Access
- **Trigger:** Access to field declared in base class that is:
  - `protected`, `internal`, or `protected internal`
  - NOT `readonly` or `const`
  - NOT `static`
- **Message:** "Direct access to inherited field '{0}'; use property instead"

### ENC0003: Public/Protected Fields
- **Trigger:** Field with `public`, `protected`, `internal`, or `protected internal` modifier
- **Message:** "Field '{0}' should be converted to a property"
- **Exemptions:**
  - Structs
  - Records
  - Types with `[StructLayout]` attribute
  - WPF types (inheriting `DependencyObject`, `FrameworkElement`, etc.)
  - `readonly` or `const` fields
  - `static` fields
- **Code Fix:** Convert to auto-property

### ENC0004: Interface I Prefix
- **Trigger:** Interface name not starting with "I" followed by uppercase letter
- **Message:** "Interface '{0}' should be named 'I{0}'"
- **Code Fix:** Add "I" prefix

---

## Code Fix Summary

| Rule | Fix Available | Fix Description |
|------|---------------|-----------------|
| STR0002 | Yes | Introduce result variable, single return at end |
| STR0003 | Yes | Invert condition, wrap in if block |
| STR0004 | Yes | Convert to switch expression |
| STR0005 | Yes | Introduce result variable, use flag for loop exit |
| STR0006 | Yes | Convert to switch expression |
| STY0001 | Yes | Move comment to line above |
| STY0004 | Yes | Rename with correct prefix |
| STY0005 | Yes | Change to `string.Empty` or `string?` |
| STY0006 | Yes | Rename to PascalCase |
| ENC0001 | Yes | Change `new` to `override` (if base is virtual) |
| ENC0003 | Yes | Convert field to auto-property |
| ENC0004 | Yes | Add "I" prefix |

---

## Configuration

All rules should support configuration via `.editorconfig`:

```ini
# Disable a rule
dotnet_diagnostic.STR0001.severity = none

# Change to error
dotnet_diagnostic.NUM0002.severity = error

# Change to suggestion
dotnet_diagnostic.STY0004.severity = suggestion
```

---

## Testing Requirements

Each analyzer should have unit tests covering:
1. Positive cases (violations detected)
2. Negative cases (no false positives)
3. Exemption scenarios
4. Edge cases

Each code fix should have tests verifying:
1. Correct transformation
2. Preserved semantics
3. Proper formatting

---

## Implementation Notes

1. Use `context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None)` to skip generated code
2. Use `context.EnableConcurrentExecution()` for performance
3. All field names in analyzer code should follow the standards (m, ps, sm prefixes)
4. Analyzer class names: `{RuleId}{ShortName}Analyzer` (e.g., `STR0001NoContinueAnalyzer`)
5. Code fix class names: `{RuleId}{ShortName}CodeFix` (e.g., `STR0002MultipleReturnsCodeFix`)
