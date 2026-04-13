// // CodeFixTests.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using CodeStructure.Analyzers.CodeFixes.Encapsulation;
using CodeStructure.Analyzers.CodeFixes.Numeric;
using CodeStructure.Analyzers.CodeFixes.Structure;
using CodeStructure.Analyzers.CodeFixes.Style;
using CodeStructure.Analyzers.Diagnostics;
using CodeStructure.Analyzers.Encapsulation;
using CodeStructure.Analyzers.Numeric;
using CodeStructure.Analyzers.Structure;
using CodeStructure.Analyzers.Style;

#endregion

namespace CodeStructure.Analyzers.Tests;

public sealed class CodeFixTests
{
    [Fact]
    public async Task STR0002_CodeFix_ConvertsToSingleReturn()
    {
        var source = @"
class C
{
    int M(int value)
    {
        if (value == 0)
        {
            {|#0:return 1;|}
        }

        return 2;
    }
}
";

        var fixedSource = @"
class C
{
    int M(int value)
    {
        var result = default(int);
        if (value == 0)
        {
            {
                result = 1;
                goto ReturnLabel;
            }
        }

        {
            result = 2;
            goto ReturnLabel;
        }

    ReturnLabel:
        return result;
    }
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Str0002MultipleReturnsAnalyzer, Str0002MultipleReturnsCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STR0002, locationIndex: 0)
                );
    }

    [Fact]
    public async Task STR0002_CodeFix_UsesUniqueResultName()
    {
        var source = @"
class C
{
    int M(int value)
    {
        int result = 0;

        if (value == 0)
        {
            {|#0:return 1;|}
        }

        return 2;
    }
}
";

        var fixedSource = @"
class C
{
    int M(int value)
    {
        var result1 = default(int);
        int result = 0;

        if (value == 0)
        {
            {
                result1 = 1;
                goto ReturnLabel;
            }
        }

        {
            result1 = 2;
            goto ReturnLabel;
        }

    ReturnLabel:
        return result1;
    }
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Str0002MultipleReturnsAnalyzer, Str0002MultipleReturnsCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STR0002, locationIndex: 0)
                );
    }

    [Fact]
    public async Task STR0003_CodeFix_RemovesReturn()
    {
        var source = @"
class C
{
    void M(int value)
    {
        if (value == 0)
        {
            {|#0:return;|}
        }
    }
}
";

        var fixedSource = @"
class C
{
    void M(int value)
    {
        if (value == 0)
        {
        }
    }
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Str0003ReturnInVoidMethodAnalyzer, Str0003ReturnInVoidMethodCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STR0003, locationIndex: 0)
                );
    }

    [Fact]
    public async Task STR0004_CodeFix_ConvertsToSwitchExpression()
    {
        var source = @"
class C
{
    int M(int value)
    {
        {|#0:if|} (value == 0)
        {
            return 1;
        }
        else if (value == 1)
        {
            return 2;
        }
        else if (value == 2)
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }
}
";

        var fixedSource = @"
class C
{
    int M(int value)
    {
        return value switch
        {
            0 => 1,
            1 => 2,
            2 => 3,
            _ => 4
        };
    }
}
";

        await AnalyzerTestUtilities.VerifyCodeFixAsync<Str0004IfElseChainsAnalyzer, Str0004IfElseChainsCodeFix>(source,
                 fixedSource,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STR0004, locationIndex: 0, "4")
            );
    }

    [Fact]
    public async Task STR0005_CodeFix_ConvertsToSingleReturn()
    {
        var source = @"
class C
{
    int M(int value)
    {
        if (value == 0)
        {
            {|#0:return 1;|}
        }

        return 2;
    }
}
";

        var fixedSource = @"
class C
{
    int M(int value)
    {
        var result = default(int);
        if (value == 0)
        {
            {
                result = 1;
                goto ReturnLabel;
            }
        }

        {
            result = 2;
            goto ReturnLabel;
        }

    ReturnLabel:
        return result;
    }
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Str0005ReturnInNestedBlockAnalyzer, Str0005ReturnInNestedBlockCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STR0005, locationIndex: 0, "if statement")
                );
    }

    [Fact]
    public async Task STR0006_CodeFix_ConvertsToSwitchExpression()
    {
        var source = @"
class C
{
    int M(int value)
    {
        if (value == 0)
        {
            return 1;
        }
        {|#0:else|} if (value == 1)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }
}
";

        var fixedSource = @"
class C
{
    int M(int value)
    {
        return value switch
        {
            0 => 1,
            1 => 2,
            _ => 3
        };
    }
}
";

        await AnalyzerTestUtilities.VerifyCodeFixAsync<Str0006NoElseIfAnalyzer, Str0006NoElseIfCodeFix>(source,
                 fixedSource,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STR0006, locationIndex: 0)
            );
    }

    [Fact]
    public async Task STY0001_CodeFix_MovesComment()
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

        var fixedSource = @"
class C
{
    void M()
    {
        // comment
        int value = 1;
    }
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Sty0001EndOfLineCommentsAnalyzer, Sty0001EndOfLineCommentsCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0001, locationIndex: 0)
                );
    }

    [Fact]
    public async Task STY0001_CodeFix_MovesCommentWithTabs()
    {
        var source =
            "\r\nclass C\r\n{\r\n    void M()\r\n    {\r\n        int value = 1;\t {|#0:// comment|}\r\n    }\r\n}\r\n";

        var fixedSource =
            "\r\nclass C\r\n{\r\n    void M()\r\n    {\r\n        // comment\r\n        int value = 1;\r\n    }\r\n}\r\n";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Sty0001EndOfLineCommentsAnalyzer, Sty0001EndOfLineCommentsCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0001, locationIndex: 0)
                );
    }

    [Fact]
    public async Task STY0004_CodeFix_RenamesField()
    {
        var source = @"
class C
{
    private string {|#0:customerName|};

    string M()
    {
        return customerName;
    }
}
";

        var fixedSource = @"
class C
{
    private string mCustomerName;

    string M()
    {
        return mCustomerName;
    }
}
";

        await AnalyzerTestUtilities.VerifyCodeFixAsync<Sty0004FieldNamingAnalyzer, Sty0004FieldNamingCodeFix>(source,
                 fixedSource,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0004,
                                                    locationIndex: 0,
                                                    "customerName",
                                                    "mCustomerName"
                                                   )
            );
    }

    [Fact]
    public async Task STY0004_CodeFix_RenamesStaticReadonlyField()
    {
        var source = @"
class C
{
    private static readonly string {|#0:defaultName|} = """";

    string M()
    {
        return defaultName;
    }
}
";

        var fixedSource = @"
class C
{
    private static readonly string smDefaultName = """";

    string M()
    {
        return smDefaultName;
    }
}
";

        await AnalyzerTestUtilities.VerifyCodeFixAsync<Sty0004FieldNamingAnalyzer, Sty0004FieldNamingCodeFix>(source,
                 fixedSource,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0004,
                                                    locationIndex: 0,
                                                    "defaultName",
                                                    "smDefaultName"
                                                   )
            );
    }

    [Fact]
    public async Task STY0005_CodeFix_ReplacesWithStringEmpty()
    {
        var source = @"
class C
{
    void M()
    {
        string value = {|#0:null|};
    }
}
";

        var fixedSource = @"
class C
{
    void M()
    {
        string value = string.Empty;
    }
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Sty0005NonNullableStringNullAnalyzer, Sty0005NonNullableStringNullCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0005, locationIndex: 0)
                );
    }

    [Fact]
    public async Task STY0006_CodeFix_RenamesMethod()
    {
        var source = @"
class C
{
    void {|#0:do_work|}()
    {
    }

    void M()
    {
        do_work();
    }
}
";

        var fixedSource = @"
class C
{
    void DoWork()
    {
    }

    void M()
    {
        DoWork();
    }
}
";

        await AnalyzerTestUtilities.VerifyCodeFixAsync<Sty0006MethodNamingAnalyzer, Sty0006MethodNamingCodeFix>(source,
                 fixedSource,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0006, locationIndex: 0, "do_work")
            );
    }

    [Fact]
    public async Task STY0006_CodeFix_RenamesMethodWithMultipleUnderscores()
    {
        var source = @"
class C
{
    void {|#0:do_more_work|}()
    {
    }

    void M()
    {
        do_more_work();
    }
}
";

        var fixedSource = @"
class C
{
    void DoMoreWork()
    {
    }

    void M()
    {
        DoMoreWork();
    }
}
";

        await AnalyzerTestUtilities.VerifyCodeFixAsync<Sty0006MethodNamingAnalyzer, Sty0006MethodNamingCodeFix>(source,
                 fixedSource,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0006, locationIndex: 0, "do_more_work")
            );
    }

    [Fact]
    public async Task ENC0001_CodeFix_ReplacesNewWithOverride()
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

        var fixedSource = @"
class Base
{
    public virtual void M() { }
}

class Derived : Base
{
    public override void M() { }
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Enc0001AvoidHidingWithNewAnalyzer, Enc0001AvoidHidingWithNewCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.ENC0001, locationIndex: 0, "M")
                );
    }

    [Fact]
    public async Task ENC0001_CodeFix_UsesProtectedOverride()
    {
        var source = @"
class Base
{
    protected virtual void M() { }
}

class Derived : Base
{
    {|#0:new protected void M() { }|}
}
";

        var fixedSource = @"
class Base
{
    protected virtual void M() { }
}

class Derived : Base
{
    protected override void M() { }
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Enc0001AvoidHidingWithNewAnalyzer, Enc0001AvoidHidingWithNewCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.ENC0001, locationIndex: 0, "M")
                );
    }

    [Fact]
    public async Task ENC0001_CodeFix_UsesProtectedInternalOverride()
    {
        var source = @"
class Base
{
    protected internal virtual void M() { }
}

class Derived : Base
{
    {|#0:new protected internal void M() { }|}
}
";

        var fixedSource = @"
class Base
{
    protected internal virtual void M() { }
}

class Derived : Base
{
    protected internal override void M() { }
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Enc0001AvoidHidingWithNewAnalyzer, Enc0001AvoidHidingWithNewCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.ENC0001, locationIndex: 0, "M")
                );
    }

    [Fact]
    public async Task ENC0003_CodeFix_ConvertsFieldToProperty()
    {
        var source = @"
class C
{
    public int {|#0:pmValue|} = 1;
}
";

        var fixedSource = @"
class C
{
    public int pmValue { get; set; } = 1;
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Enc0003PublicProtectedFieldsAnalyzer, Enc0003PublicProtectedFieldsCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.ENC0003, locationIndex: 0, "pmValue")
                );
    }

    [Fact]
    public async Task ENC0004_CodeFix_RenamesInterface()
    {
        var source = @"
interface {|#0:Repository|}
{
}

class C : Repository
{
}
";

        var fixedSource = @"
interface IRepository
{
}

class C : IRepository
{
}
";

        await AnalyzerTestUtilities
            .VerifyCodeFixAsync<Enc0004InterfacePrefixAnalyzer, Enc0004InterfacePrefixCodeFix>(source,
                     fixedSource,
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.ENC0004, locationIndex: 0, "Repository")
                );
    }

    [Fact]
    public async Task STY0008_CodeFix_ExtractsMagicStringToConstant()
    {
        var source = @"
class C
{
    void M()
    {
        string value = {|#0:""active""|};
    }
}
";

        var fixedSource = @"
class C
{
    private const string Active = ""active"";
    void M()
    {
        string value = Active;
    }
}
";

        await AnalyzerTestUtilities.VerifyCodeFixAsync<Sty0008MagicStringsAnalyzer, Sty0008MagicStringsCodeFix>(source,
                 fixedSource,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0008, locationIndex: 0, "active")
            );
    }

    [Fact]
    public async Task STY0008_CodeFix_ExtractsStringInComparison()
    {
        var source = @"
class C
{
    bool M(string status)
    {
        return status == {|#0:""pending""|};
    }
}
";

        var fixedSource = @"
class C
{
    private const string Pending = ""pending"";
    bool M(string status)
    {
        return status == Pending;
    }
}
";

        await AnalyzerTestUtilities.VerifyCodeFixAsync<Sty0008MagicStringsAnalyzer, Sty0008MagicStringsCodeFix>(source,
                 fixedSource,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0008, locationIndex: 0, "pending")
            );
    }

    [Fact]
    public async Task NUM0002_CodeFix_ExtractsMagicNumberToConstant()
    {
        var source = @"
class C
{
    void M()
    {
        int value = {|#0:42|};
    }
}
";

        var fixedSource = @"
class C
{
    private const int Value42 = 42;
    void M()
    {
        int value = Value42;
    }
}
";

        await AnalyzerTestUtilities.VerifyCodeFixAsync<Num0002MagicNumbersAnalyzer, Num0002MagicNumbersCodeFix>(source,
                 fixedSource,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.NUM0002, locationIndex: 0, "42")
            );
    }

    [Fact]
    public async Task NUM0002_CodeFix_ExtractsNegativeNumber()
    {
        var source = @"
class C
{
    void M()
    {
        double value = {|#0:3.14|};
    }
}
";

        var fixedSource = @"
class C
{
    private const double Value3Point14 = 3.14;
    void M()
    {
        double value = Value3Point14;
    }
}
";

        await AnalyzerTestUtilities.VerifyCodeFixAsync<Num0002MagicNumbersAnalyzer, Num0002MagicNumbersCodeFix>(source,
                 fixedSource,
                 AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.NUM0002, locationIndex: 0, "3.14")
            );
    }

    // Skip: Complex end-to-end test with additional documents has issues with Roslyn test framework's
    // fix-all validation. The underlying resource utilities are tested separately in ExtractToResourceUtilitiesTests.
    [Fact(Skip = "Roslyn test framework fix-all validation incompatible with Solution-level code fixes")]
    public async Task STY0008_CodeFix_UsesExistingResourceFromResx()
    {
        // Include a mock Resources class so the fixed code compiles
        // Use const to avoid triggering STY0008 on the mock class
        var source = @"
static class Resources
{
    private const string WelcomeMessageValue = ""Welcome to the application"";
    public static string WelcomeMessage => WelcomeMessageValue;
}

class C
{
    void M()
    {
        string msg = {|#0:""Welcome to the application""|};
    }
}
";

        var fixedSource = @"
static class Resources
{
    private const string WelcomeMessageValue = ""Welcome to the application"";
    public static string WelcomeMessage => WelcomeMessageValue;
}

class C
{
    void M()
    {
        string msg = Resources.WelcomeMessage;
    }
}
";

        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""WelcomeMessage"" xml:space=""preserve"">
    <value>Welcome to the application</value>
  </data>
</root>";

        // Code fix index 0 should be "Use existing 'Resources.WelcomeMessage'"
        await AnalyzerTestUtilities
            .VerifyCodeFixWithAdditionalDocumentsAsync<Sty0008MagicStringsAnalyzer, Sty0008MagicStringsCodeFix>(source,
                     fixedSource,
                         [("Resources.resx", resxContent)],
                     fixedAdditionalDocuments: null, // resx unchanged
                     codeFixIndex: 0,                // first code fix (use existing resource)
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0008,
                                                        locationIndex: 0,
                                                        "Welcome to the application"
                                                       )
                );
    }

    [Fact(Skip = "Roslyn test framework fix-all validation incompatible with Solution-level code fixes")]
    public async Task STY0008_CodeFix_AddsNewResourceToResx()
    {
        // Include a mock Resources class so the fixed code compiles
        // Use const to avoid triggering STY0008 on the mock class
        var source = @"
static class Resources
{
    private const string ExistingKeyValue = ""Existing Value"";
    private const string NewMessageValue = ""New message"";
    public static string ExistingKey => ExistingKeyValue;
    public static string NewMessage => NewMessageValue;
}

class C
{
    void M()
    {
        string msg = {|#0:""New message""|};
    }
}
";

        var fixedSource = @"
static class Resources
{
    private const string ExistingKeyValue = ""Existing Value"";
    private const string NewMessageValue = ""New message"";
    public static string ExistingKey => ExistingKeyValue;
    public static string NewMessage => NewMessageValue;
}

class C
{
    void M()
    {
        string msg = Resources.NewMessage;
    }
}
";

        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""ExistingKey"" xml:space=""preserve"">
    <value>Existing Value</value>
  </data>
</root>";

        // The fixed resx should have the new entry added
        // Note: XDocument.ToString() doesn't include XML declaration
        var fixedResxContent = @"<root>
  <data name=""ExistingKey"" xml:space=""preserve"">
    <value>Existing Value</value>
  </data>
  <data name=""NewMessage"" xml:space=""preserve"">
    <value>New message</value>
  </data>
</root>";

        // Code fix index 0 should be "Add to Resources as 'NewMessage'"
        await AnalyzerTestUtilities
            .VerifyCodeFixWithAdditionalDocumentsAsync<Sty0008MagicStringsAnalyzer, Sty0008MagicStringsCodeFix>(source,
                     fixedSource,
                         [("Resources.resx", resxContent)],
                         [("Resources.resx", fixedResxContent)],
                     codeFixIndex: 0, // first code fix (add to resources)
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0008, locationIndex: 0, "New message")
                );
    }

    [Fact(Skip = "Roslyn test framework fix-all validation incompatible with Solution-level code fixes")]
    public async Task STY0008_CodeFix_ExtractToConstantWithResxPresent()
    {
        // When resx exists but is empty, extract to constant should be available
        // The order is: add to resources (single), add to resources (all), extract to constant
        // But since there's only 1 occurrence, "replace all" won't appear
        var source = @"
class C
{
    void M()
    {
        string msg = {|#0:""Some value""|};
    }
}
";

        var fixedSource = @"
class C
{
    private const string SomeValue = ""Some value"";
    void M()
    {
        string msg = SomeValue;
    }
}
";

        var resxContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
</root>";

        // With 1 occurrence: index 0 = add to resources, index 1 = extract to constant
        await AnalyzerTestUtilities
            .VerifyCodeFixWithAdditionalDocumentsAsync<Sty0008MagicStringsAnalyzer, Sty0008MagicStringsCodeFix>(source,
                     fixedSource,
                         [("Resources.resx", resxContent)],
                     fixedAdditionalDocuments: null,
                     codeFixIndex: 1, // second code fix (extract to constant, after single add to resource)
                     AnalyzerTestUtilities.CreateResult(DiagnosticDescriptors.STY0008, locationIndex: 0, "Some value")
                );
    }
}
