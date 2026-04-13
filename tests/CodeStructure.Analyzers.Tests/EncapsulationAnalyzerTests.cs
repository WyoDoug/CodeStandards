// // EncapsulationAnalyzerTests.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Encapsulation;

#endregion

namespace CodeStructure.Analyzers.Tests;

public sealed class EncapsulationAnalyzerTests
{
    [Fact]
    public async Task ENC0001_FlagsNewModifier()
    {
        var source = @"
class Base
{
    public virtual void M() { }
}

class Derived : Base
{
    {|#0:new void M() { }|}
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Enc0001AvoidHidingWithNewAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.ENC0001, locationIndex: 0, "M")
            );
    }

    [Fact]
    public async Task ENC0002_FlagsInheritedFieldAccess()
    {
        var source = @"
class Base
{
    protected int mValue;
}

class Derived : Base
{
    int M()
    {
        return {|#0:mValue|};
    }
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Enc0002DirectInheritedFieldAccessAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.ENC0002, locationIndex: 0, "mValue")
            );
    }

    [Fact]
    public async Task ENC0003_FlagsPublicField()
    {
        var source = @"
class C
{
    public int {|#0:pmValue|};
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Enc0003PublicProtectedFieldsAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.ENC0003, locationIndex: 0, "pmValue")
            );
    }

    [Fact]
    public async Task ENC0003_AllowsStructFields()
    {
        var source = @"
struct C
{
    public int mValue;
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Enc0003PublicProtectedFieldsAnalyzer>(source);
    }

    [Fact]
    public async Task ENC0004_FlagsInterfaceNaming()
    {
        var source = @"
interface {|#0:Repository|}
{
}
";

        await AnalyzerTestUtilities.VerifyAnalyzerAsync<Enc0004InterfacePrefixAnalyzer>(source,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.ENC0004, locationIndex: 0, "Repository")
            );
    }
}
