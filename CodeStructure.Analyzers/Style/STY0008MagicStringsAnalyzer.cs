// // STY0008MagicStringsAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System;
using System.Collections.Immutable;
using System.Linq;
using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Style;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Sty0008MagicStringsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.STY0008];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Use compilation start to check once if this is a test assembly
        context.RegisterCompilationStartAction(compilationContext =>
                                               {
                                                   // Skip analysis entirely for test assemblies
                                                   if (AnalyzerUtilities.IsTestAssembly(compilationContext.Compilation))
                                                       return;

                                                   compilationContext.RegisterSyntaxNodeAction(AnalyzeLiteralExpression,
                                                            SyntaxKind.StringLiteralExpression
                                                       );
                                               }
                                              );
    }

    private static void AnalyzeLiteralExpression(SyntaxNodeAnalysisContext context)
    {
        var literalExpression = (LiteralExpressionSyntax) context.Node;
        bool isExempt = IsExempt(literalExpression, context);

        if (!isExempt)
        {
            var constantValue = context.SemanticModel.GetConstantValue(literalExpression, context.CancellationToken);

            if (constantValue.HasValue && constantValue.Value is string stringValue)
            {
                bool isAllowed = IsAllowedStringLiteral(stringValue);

                if (!isAllowed)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.STY0008,
                                                       literalExpression.GetLocation(),
                                                       literalExpression.Token.ValueText
                                                      );
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsAllowedStringLiteral(string value)
    {
        // Allow empty strings
        if (string.IsNullOrEmpty(value))
            return true;

        // Allow single character strings (common for delimiters, whitespace, etc.)
        if (value.Length == 1)
            return true;

        // Allow common whitespace strings
        if (value == " " || value == "\t" || value == "\n" || value == "\r\n")
            return true;

        return false;
    }

    private static bool IsExempt(LiteralExpressionSyntax literalExpression, SyntaxNodeAnalysisContext context)
    {
        var result = false;

        if (IsInConstDeclaration(literalExpression))
            result = true;

        if (!result)
        {
            if (IsInStaticReadonlyField(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInAttributeArgument(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInInitializer(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInInterpolatedString(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInLoggingCall(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInExceptionConstructor(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInSwitchCaseLabel(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInParameterDefault(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsLikelyRegexPattern(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (IsInEntityFrameworkConfiguration(literalExpression))
                result = true;
        }

        if (!result)
        {
            if (AnalyzerUtilities.IsTestMethod(context.ContainingSymbol))
                result = true;
        }

        return result;
    }

    private static bool IsInLoggingCall(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        // Check if this is an argument to a logging method
        if (literalExpression.Parent is ArgumentSyntax argument)
        {
            if (argument.Parent?.Parent is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    string methodName = memberAccess.Name.Identifier.Text;

                    // Common logging method names (ILogger, log4net, NLog, Serilog, etc.)
                    // Case-insensitive to handle log.info(), LOG.ERROR(), etc.
                    string[] loggingMethods =
                        [
                            "Log", "LogInformation", "LogWarning", "LogError",
                            "LogDebug", "LogTrace", "LogCritical", "LogException",
                            "Debug", "Info", "Warn", "Warning", "Error", "Fatal", "Trace",
                            "Exception", "Critical",
                            "Write", "WriteLine", "WriteLog", "WriteEntry",
                            "Information", "Verbose"
                        ];

                    result = loggingMethods.Any(m => string.Equals(m, methodName, StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        return result;
    }

    private static bool IsInConstDeclaration(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        if (literalExpression.Parent is EqualsValueClauseSyntax equalsValue)
        {
            if (equalsValue.Parent is VariableDeclaratorSyntax variableDeclarator)
            {
                if (variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration)
                {
                    if (variableDeclaration.Parent is FieldDeclarationSyntax fieldDeclaration)
                    {
                        if (fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
                            result = true;
                    }
                    else
                    {
                        if (variableDeclaration.Parent is LocalDeclarationStatementSyntax localDeclaration)
                        {
                            if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
                                result = true;
                        }
                    }
                }
            }
        }

        return result;
    }

    private static bool IsInStaticReadonlyField(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        if (literalExpression.Parent is EqualsValueClauseSyntax equalsValue)
        {
            if (equalsValue.Parent is VariableDeclaratorSyntax variableDeclarator)
            {
                if (variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration)
                {
                    if (variableDeclaration.Parent is FieldDeclarationSyntax fieldDeclaration)
                    {
                        bool isStatic = fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);
                        bool isReadonly = fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);

                        if (isStatic && isReadonly)
                            result = true;
                    }
                }
            }
        }

        return result;
    }

    private static bool IsInAttributeArgument(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        var current = literalExpression.Parent;

        while (current != null && !result)
        {
            if (current is AttributeArgumentSyntax)
                result = true;

            current = current.Parent;
        }

        return result;
    }

    private static bool IsInInitializer(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        var current = literalExpression.Parent;

        while (current != null && !result)
        {
            if (current is InitializerExpressionSyntax)
                result = true;

            current = current.Parent;
        }

        return result;
    }

    private static bool IsInInterpolatedString(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        var current = literalExpression.Parent;

        while (current != null && !result)
        {
            if (current is InterpolatedStringExpressionSyntax)
                result = true;

            current = current.Parent;
        }

        return result;
    }

    private static bool IsInExceptionConstructor(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        // Check if this is an argument to an exception constructor
        // e.g., throw new ArgumentException("message")
        if (literalExpression.Parent is ArgumentSyntax argument)
        {
            if (argument.Parent?.Parent is ObjectCreationExpressionSyntax objectCreation)
            {
                // Check if the type name ends with "Exception"
                string? typeName = objectCreation.Type switch
                    {
                        IdentifierNameSyntax identifier => identifier.Identifier.Text,
                        QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
                        var _ => null
                    };

                if (typeName != null && typeName.EndsWith("Exception", StringComparison.Ordinal))
                    result = true;
            }
        }

        return result;
    }

    private static bool IsInSwitchCaseLabel(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        var current = literalExpression.Parent;

        while (current != null && !result)
        {
            // Traditional switch case: case "value":
            if (current is CaseSwitchLabelSyntax)
                result = true;

            // Pattern matching: case "value" => or "value" when
            if (current is ConstantPatternSyntax)
                result = true;

            current = current.Parent;
        }

        return result;
    }

    private static bool IsInParameterDefault(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        // Check if this is a default value for a parameter
        // e.g., void Foo(string x = "default")
        if (literalExpression.Parent is EqualsValueClauseSyntax equalsValue)
        {
            if (equalsValue.Parent is ParameterSyntax)
                result = true;
        }

        return result;
    }

    private static bool IsLikelyRegexPattern(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        string value = literalExpression.Token.ValueText;

        // Check for common regex metacharacters
        // If the string contains multiple regex-specific characters, it's likely a pattern
        var regexCharCount = 0;
        char[] regexMetaChars = ['^', '$', '*', '+', '?', '[', ']', '(', ')', '{', '}', '|', '\\'];

        foreach(char c in value)
        {
            if (Array.IndexOf(regexMetaChars, c) >= 0)
                regexCharCount++;
        }

        // If 2 or more regex metacharacters, likely a regex pattern
        if (regexCharCount >= 2)
            result = true;

        // Also check for common regex patterns
        if (!result)
        {
            if (value.StartsWith("^", StringComparison.Ordinal) ||
                value.EndsWith("$", StringComparison.Ordinal) ||
                value.Contains(".*", StringComparison.Ordinal) ||
                value.Contains(".+", StringComparison.Ordinal) ||
                value.Contains("\\d", StringComparison.Ordinal) ||
                value.Contains("\\w", StringComparison.Ordinal) ||
                value.Contains("\\s", StringComparison.Ordinal))
                result = true;
        }

        return result;
    }

    private static bool IsInEntityFrameworkConfiguration(LiteralExpressionSyntax literalExpression)
    {
        var result = false;

        // Check if this is an argument to an EF Core fluent API method
        if (literalExpression.Parent is ArgumentSyntax argument)
        {
            if (argument.Parent?.Parent is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    string methodName = memberAccess.Name.Identifier.Text;

                    // Common EF Core fluent API methods that use string literals
                    string[] efConfigMethods =
                        [
                            // Table and column mapping
                            "ToTable", "HasColumnName", "HasColumnType", "HasColumnOrder",
                            "HasDefaultValueSql", "HasComputedColumnSql", "HasComment",
                            "HasCheckConstraint", "HasConstraintName",
                            // Key and index naming
                            "HasName", "HasPrincipalKey", "HasForeignKey",
                            // Schema
                            "HasSchema", "HasSequence",
                            // Stored procedures
                            "HasTrigger", "ToFunction", "ToStoredProcedure",
                            // Conversion and value generation
                            "HasValueGenerator", "HasField",
                            // Navigation and relationships
                            "HasDiscriminator", "HasValue",
                            // TPH/TPT/TPC
                            "UseTphMappingStrategy", "UseTptMappingStrategy", "UseTpcMappingStrategy",
                            // Database-specific
                            "UseCollation", "HasAnnotation",
                            // JSON columns (EF Core 7+)
                            "ToJson",
                            // Owned types
                            "OwnsOne", "OwnsMany"
                        ];

                    result = efConfigMethods.Any(m => string.Equals(m, methodName, StringComparison.Ordinal));
                }
            }
        }

        // Also check for being inside a lambda in EF configuration
        // e.g., .Property(e => e.Name).HasColumnName("name")
        if (!result)
        {
            var current = literalExpression.Parent;

            while (current != null && !result)
            {
                // Check if we're in a method that's commonly used for EF model configuration
                if (current is MethodDeclarationSyntax methodDeclaration)
                {
                    string methodName = methodDeclaration.Identifier.Text;

                    // Common EF configuration method names
                    if (methodName == "OnModelCreating" ||
                        methodName == "Configure" ||
                        methodName == "Up" ||
                        methodName == "Down" ||
                        methodName == "BuildModel" ||
                        methodName == "BuildTargetModel")
                        result = true;
                }

                // Check if we're in a class that implements IEntityTypeConfiguration
                if (current is ClassDeclarationSyntax classDeclaration)
                {
                    if (classDeclaration.BaseList != null)
                    {
                        foreach(var baseType in classDeclaration.BaseList.Types)
                        {
                            var baseTypeName = baseType.Type.ToString();

                            if (baseTypeName.Contains("IEntityTypeConfiguration") ||
                                baseTypeName.Contains("Migration") ||
                                baseTypeName.Contains("DbContext"))
                                result = true;
                        }
                    }
                }

                current = current.Parent;
            }
        }

        return result;
    }
}
