// // NumericAnalyzerTests.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Numeric;

#endregion

namespace CodeStructure.Analyzers.Tests;

public sealed class NumericAnalyzerTests
{
    [Fact]
    public async Task NUM0001_FlagsFloatingPointEquality()
    {
        var source = @"
class C
{
    bool M(double value)
    {
        return value {|#0:==|} 0.1;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0001FloatingPointEqualityAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.NUM0001, locationIndex: 0)
            );
    }

    [Fact]
    public async Task NUM0001_AllowsComparisonToZeroInTests()
    {
        var source = @"
using System;

class FactAttribute : Attribute { }

class C
{
    [Fact]
    bool M(double value)
    {
        return value == 0;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0001FloatingPointEqualityAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0001_AllowsComparisonToZero()
    {
        var source = @"
class C
{
    bool M(double value)
    {
        return value == 0;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0001FloatingPointEqualityAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0001_AllowsComparisonToOne()
    {
        var source = @"
class C
{
    bool M(double value)
    {
        return value == 1;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0001FloatingPointEqualityAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0001_AllowsComparisonToNegativeOne()
    {
        var source = @"
class C
{
    bool M(float value)
    {
        return value != -1;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0001FloatingPointEqualityAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0001_AllowsEqualityInTestMethod()
    {
        var source = @"
using System;

public sealed class Fact : Attribute { }

class C
{
    [Fact]
    bool M(double value)
    {
        return value == 0.1;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0001FloatingPointEqualityAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0002_FlagsMagicNumber()
    {
        var source = @"
class C
{
    int M()
    {
        return {|#0:777|};
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0002MagicNumbersAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.NUM0002, locationIndex: 0, "777")
            );
    }

    [Fact]
    public async Task NUM0002_AllowsConstAndReadonly()
    {
        var source = @"
class C
{
    private const int Max = 777;
    private static readonly int Limit = 777;

    int M()
    {
        return Max + Limit;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0002MagicNumbersAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0002_AllowsEnumAndAttribute()
    {
        var source = @"
using System;

class NumberAttribute : Attribute
{
    public NumberAttribute(int value) { }
}

enum E
{
    A = 777
}

class C
{
    [Number(777)]
    int M()
    {
        return (int)E.A;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0002MagicNumbersAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0002_AllowsInitializerAndHashCode()
    {
        var source = @"
class C
{
    private int[] mValues = new[] { 777 };

    public override int GetHashCode()
    {
        return 777;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0002MagicNumbersAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0002_AllowsHexLiteral()
    {
        var source = @"
class C
{
    int M()
    {
        return 0x7FFFFFFF;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0002MagicNumbersAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0001_SkipsTestAssemblies()
    {
        // This code would normally trigger NUM0001, but should be skipped
        // because the test assembly has xUnit references
        var source = @"
class C
{
    bool M(double value)
    {
        return value == 0.1;
    }
}
";

        // No diagnostics expected because this is a test assembly
        await AnalyzerTestUtilities.VerifyAnalyzerInTestAssemblyAsync<Num0001FloatingPointEqualityAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0002_SkipsTestAssemblies()
    {
        // This code would normally trigger NUM0002, but should be skipped
        // because the test assembly has xUnit references
        var source = @"
class C
{
    int M()
    {
        return 777;
    }
}
";

        // No diagnostics expected because this is a test assembly
        await AnalyzerTestUtilities.VerifyAnalyzerInTestAssemblyAsync<Num0002MagicNumbersAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0002_AllowsEFCoreFluentApiMethods()
    {
        var source = @"
class PropertyBuilder
{
    public PropertyBuilder HasMaxLength(int length) => this;
    public PropertyBuilder HasPrecision(int precision, int scale) => this;
    public PropertyBuilder HasColumnOrder(int order) => this;
    public PropertyBuilder HasDefaultValue(object value) => this;
}

class C
{
    void Configure(PropertyBuilder builder)
    {
        builder
            .HasMaxLength(100)
            .HasPrecision(18, 2)
            .HasColumnOrder(5)
            .HasDefaultValue(42);
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0002MagicNumbersAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0002_AllowsEFCoreOnModelCreating()
    {
        var source = @"
class MyDbContext
{
    protected void OnModelCreating()
    {
        int maxLength = 255;
        int precision = 18;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0002MagicNumbersAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0002_AllowsEFCoreMigration()
    {
        var source = @"
class Migration
{
}

class CreateUsersTable : Migration
{
    void Up()
    {
        int columnSize = 500;
    }

    void Down()
    {
        int timeout = 30;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0002MagicNumbersAnalyzer>(source);
    }

    [Fact]
    public async Task NUM0002_AllowsIEntityTypeConfiguration()
    {
        var source = @"
interface IEntityTypeConfiguration<T> { }

class UserConfiguration : IEntityTypeConfiguration<object>
{
    public void Configure()
    {
        int maxLength = 200;
        int scale = 4;
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Num0002MagicNumbersAnalyzer>(source);
    }
}
