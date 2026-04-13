// // StyleAnalyzerTests.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Style;

#endregion

namespace CodeStructure.Analyzers.Tests;

public sealed class StyleAnalyzerTests
{
    [Fact]
    public async Task STY0001_FlagsEndOfLineComment()
    {
        var source = @"
class C
{
    void M()
    {
        int value = 1; {|#0:// comment|}
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0001EndOfLineCommentsAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0001, locationIndex: 0)
            );
    }

    [Fact]
    public async Task STY0001_FlagsEndOfLineCommentWithTabs()
    {
        var source = "\nclass C\n{\n    void M()\n    {\n        int value = 1;\t {|#0:// comment|}\n    }\n}\n";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0001EndOfLineCommentsAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0001, locationIndex: 0)
            );
    }

    [Fact]
    public async Task STY0001_FlagsEndOfLineCommentInSingleLineBlock()
    {
        var source = @"
class C
{
    void M()
    {
        if (true) { int value = 1; {|#0:// comment|}
        }
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0001EndOfLineCommentsAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0001, locationIndex: 0)
            );
    }

    [Fact]
    public async Task STY0002_FlagsNullForgivingOperator()
    {
        var source = @"
class C
{
    string M(string value)
    {
        string result = {|#0:value!|};
        return result;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0002NullForgivingOperatorAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0002, locationIndex: 0)
            );
    }

    [Fact]
    public async Task STY0003_FlagsDynamicUsage()
    {
        var source = @"
class C
{
    void M()
    {
        {|#0:dynamic|} value = 1;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0003AvoidDynamicAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0003, locationIndex: 0)
            );
    }

    [Fact]
    public async Task STY0003_AllowsComImportDynamic()
    {
        var source = @"
using System;
using System.Runtime.InteropServices;

[ComImport]
[Guid(""00000000-0000-0000-0000-000000000001"")]
class C
{
    public dynamic mValue;
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0003AvoidDynamicAnalyzer>(source);
    }

    [Fact]
    public async Task STY0004_FlagsFieldNaming()
    {
        var source = @"
class C
{
    private string {|#0:customerName|};
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0004FieldNamingAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0004,
                                                    locationIndex: 0,
                                                    "customerName",
                                                    "mCustomerName"
                                                   )
            );
    }

    [Fact]
    public async Task STY0004_AllowsCorrectPrefixes()
    {
        var source = @"
class C
{
    private string mCustomerName;
    private static int psCount;
    private static readonly string smDefault = """";
    public int pmValue;
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0004FieldNamingAnalyzer>(source);
    }

    [Fact]
    public async Task STY0005_FlagsNonNullableStringNull()
    {
        var source = @"
class C
{
    void M()
    {
        string name = {|#0:null|};
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0005NonNullableStringNullAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0005, locationIndex: 0)
            );
    }

    [Fact]
    public async Task STY0006_FlagsMethodNaming()
    {
        var source = @"
class C
{
    void {|#0:do_work|}()
    {
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0006MethodNamingAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0006, locationIndex: 0, "do_work")
            );
    }

    [Fact]
    public async Task STY0006_AllowsPascalCase()
    {
        var source = @"
class C
{
    void DoWork()
    {
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0006MethodNamingAnalyzer>(source);
    }

    [Fact]
    public async Task STY0007_FlagsGenericRegionName()
    {
        var source = @"
class C
{
    {|#0:#region Methods|}
    void M() { }
    #endregion
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0007RegionNamingAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0007, locationIndex: 0, "Methods")
            );
    }

    [Fact]
    public async Task STY0008_FlagsMagicStringInComparison()
    {
        var source = @"
class C
{
    void M(string status)
    {
        if (status == {|#0:""Active""|})
        {
        }
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0008, locationIndex: 0, "Active")
            );
    }

    [Fact]
    public async Task STY0008_FlagsMagicStringInAssignment()
    {
        var source = @"
class C
{
    void M()
    {
        string status = {|#0:""Pending""|};
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0008, locationIndex: 0, "Pending")
            );
    }

    [Fact]
    public async Task STY0008_AllowsEmptyString()
    {
        var source = @"
class C
{
    void M()
    {
        string value = """";
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsSingleCharString()
    {
        var source = @"
class C
{
    void M()
    {
        string delimiter = "","";
        string space = "" "";
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsConstDeclaration()
    {
        var source = @"
class C
{
    const string ActiveStatus = ""Active"";

    void M()
    {
        const string PendingStatus = ""Pending"";
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsStaticReadonlyField()
    {
        var source = @"
class C
{
    static readonly string DefaultName = ""DefaultValue"";
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsAttributeArgument()
    {
        var source = @"
using System;

[Obsolete(""Use NewMethod instead"")]
class C
{
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsArrayInitializer()
    {
        var source = @"
class C
{
    void M()
    {
        string[] items = { ""Item1"", ""Item2"", ""Item3"" };
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsLoggingMessages()
    {
        var source = @"
using System;

class Logger
{
    public void LogInformation(string message) { }
    public void LogError(string message) { }
    public void LogException(string message) { }
    public void Exception(string message) { }
    public void Info(string message) { }
    public void Warn(string message) { }
    public void Debug(string message) { }
    public void Write(string message) { }
}

class C
{
    Logger logger = new Logger();

    void M()
    {
        logger.LogInformation(""User logged in successfully"");
        logger.LogError(""Failed to connect to database"");
        logger.LogException(""Exception occurred"");
        logger.Exception(""Unhandled exception"");
        logger.Info(""Processing started"");
        logger.Warn(""Low memory warning"");
        logger.Debug(""Debug: Processing item"");
        logger.Write(""Log entry"");
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsLoggingMessagesCaseInsensitive()
    {
        var source = @"
using System;

class Logger
{
    public void log(string message) { }
    public void INFO(string message) { }
    public void warning(string message) { }
}

class C
{
    Logger logger = new Logger();

    void M()
    {
        logger.log(""lowercase log method"");
        logger.INFO(""UPPERCASE info method"");
        logger.warning(""mixed case warning"");
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_SkipsTestAssemblies()
    {
        // This code would normally trigger STY0008, but should be skipped
        // because the test assembly has xUnit references
        var source = @"
class C
{
    void M(string status)
    {
        if (status == ""Active"")
        {
        }
    }
}
";

        // No diagnostics expected because this is a test assembly
        await AnalyzerTestUtilities.VerifyAnalyzerInTestAssemblyAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0002_SkipsTestAssemblies()
    {
        // This code would normally trigger STY0002, but should be skipped
        // because the test assembly has xUnit references
        var source = @"
class C
{
    string M(string value)
    {
        string result = value!;
        return result;
    }
}
";

        // No diagnostics expected because this is a test assembly
        await AnalyzerTestUtilities.VerifyAnalyzerInTestAssemblyAsync<Sty0002NullForgivingOperatorAnalyzer>(source);
    }

    [Fact]
    public async Task STY0003_SkipsTestAssemblies()
    {
        // This code would normally trigger STY0003, but should be skipped
        // because the test assembly has xUnit references
        var source = @"
class C
{
    void M()
    {
        dynamic value = 1;
    }
}
";

        // No diagnostics expected because this is a test assembly
        await AnalyzerTestUtilities.VerifyAnalyzerInTestAssemblyAsync<Sty0003AvoidDynamicAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsExceptionMessage()
    {
        var source = @"
using System;

class C
{
    void M()
    {
        throw new ArgumentException(""Invalid argument provided"");
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsSwitchCaseLabel()
    {
        var source = @"
class C
{
    int M(string status)
    {
        switch (status)
        {
            case ""Active"":
                return 1;
            case ""Pending"":
                return 2;
            default:
                return 0;
        }
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsSwitchExpression()
    {
        var source = @"
class C
{
    int M(string status)
    {
        return status switch
        {
            ""Active"" => 1,
            ""Pending"" => 2,
            _ => 0
        };
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsParameterDefault()
    {
        var source = @"
class C
{
    void M(string status = ""Active"")
    {
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsRegexPattern()
    {
        var source = @"
using System.Text.RegularExpressions;

class C
{
    void M(string input)
    {
        var regex1 = new Regex(""^[a-z]+$"");
        var regex2 = new Regex(""\\d{3}-\\d{4}"");
        var regex3 = new Regex("".*test.*"");
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsEFCoreFluentApiMethods()
    {
        var source = @"
class EntityBuilder
{
    public EntityBuilder ToTable(string name) => this;
    public EntityBuilder HasColumnName(string name) => this;
    public EntityBuilder HasColumnType(string type) => this;
    public EntityBuilder HasDefaultValueSql(string sql) => this;
    public EntityBuilder HasSchema(string schema) => this;
    public EntityBuilder HasComment(string comment) => this;
}

class C
{
    void Configure(EntityBuilder builder)
    {
        builder
            .ToTable(""Users"")
            .HasColumnName(""user_name"")
            .HasColumnType(""nvarchar(100)"")
            .HasDefaultValueSql(""GETDATE()"")
            .HasSchema(""dbo"")
            .HasComment(""The user table"");
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsEFCoreOnModelCreating()
    {
        var source = @"
class MyDbContext
{
    protected void OnModelCreating()
    {
        string tableName = ""Products"";
        string columnName = ""product_id"";
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsEFCoreMigration()
    {
        var source = @"
class Migration
{
}

class CreateUsersTable : Migration
{
    void Up()
    {
        string sql = ""CREATE TABLE Users"";
    }

    void Down()
    {
        string sql = ""DROP TABLE Users"";
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }

    [Fact]
    public async Task STY0008_AllowsIEntityTypeConfiguration()
    {
        var source = @"
interface IEntityTypeConfiguration<T> { }

class UserConfiguration : IEntityTypeConfiguration<object>
{
    public void Configure()
    {
        string tableName = ""Users"";
        string columnName = ""user_id"";
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Sty0008MagicStringsAnalyzer>(source);
    }
}
