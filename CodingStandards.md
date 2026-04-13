# Coding Standards & Best Practices

**Version:** 1.0  
**License:** MIT

---

## Table of Contents

1. [Philosophy & Core Principles](#philosophy--core-principles)
2. [Analyzer Rules Reference](#analyzer-rules-reference)
3. [Structure Rules (STR)](#structure-rules-str)
4. [Style Rules (STY)](#style-rules-sty)
5. [Numeric Rules (NUM)](#numeric-rules-num)
6. [Encapsulation Rules (ENC)](#encapsulation-rules-enc)
7. [Universal Standards](#universal-standards)
8. [C# Specific Standards](#c-specific-standards)
9. [Enforcement & Tooling](#enforcement--tooling)

---

## Philosophy & Core Principles

### Consistency Above Convention

Code consistency across all languages takes precedence over language-specific conventions. A developer switching between C#, C++, and Python should find familiar patterns and naming conventions, reducing cognitive load and improving maintainability.

### Why These Standards Exist

1. **Reduced Context Switching**: Developers work across multiple languages daily. Using identical naming patterns eliminates mental translation.

2. **Unified Debugging Experience**: The single return rule provides debugging predictability—set one breakpoint to see all exit values.

3. **Cross-Team Collaboration**: Engineers can immediately contribute to any codebase without relearning naming conventions or patterns.

4. **Error Prevention**: Consistent standards eliminate entire classes of errors.

### Core Principles

1. **Single Return Rule**: Every function/method must have exactly one return statement at the end
2. **No Magic Numbers**: All numeric literals must be named constants or well-documented
3. **Argument Validation**: All public methods must validate their arguments
4. **Prefer Immutability**: Make data structures immutable by default
5. **Comprehensive Documentation**: All public APIs must be documented

---

## Analyzer Rules Reference

### Quick Reference Table

| ID | Name | Severity | Category | Code Fix |
|----|------|----------|----------|----------|
| STR0001 | No continue statements | Warning | Structure | No |
| STR0002 | Multiple return statements | Warning | Structure | Yes |
| STR0003 | Return in void method | Warning | Structure | Yes |
| STR0004 | If-else chains | Warning | Structure | Yes |
| STR0005 | Return in nested block | Warning | Structure | Yes |
| STR0006 | No else if | Warning | Structure | Yes |
| STR0007 | Suspicious regions | Warning | Structure | No |
| STR0008 | One type per file | Warning | Structure | No |
| STR0009 | Nesting depth exceeded | Warning | Structure | No |
| STR0010 | Missing argument validation | Warning | Structure | No |
| STY0001 | End-of-line comments | Warning | Style | Yes |
| STY0002 | Null-forgiving operator | Warning | Style | No |
| STY0003 | Avoid dynamic keyword | Warning | Style | No |
| STY0004 | Field naming convention | Warning | Style | Yes |
| STY0005 | Non-nullable string initialized to null | Warning | Style | Yes |
| STY0006 | Method naming convention | Warning | Style | Yes |
| STY0007 | Region naming convention | Warning | Style | No |
| NUM0001 | Floating-point equality | Error | Numeric | No |
| NUM0002 | Magic numbers | Warning | Numeric | No |
| ENC0001 | Avoid hiding with 'new' | Warning | Encapsulation | Yes |
| ENC0002 | Avoid direct access to inherited fields | Warning | Encapsulation | No |
| ENC0003 | Avoid public/protected fields | Warning | Encapsulation | Yes |
| ENC0004 | Missing interface I prefix | Warning | Encapsulation | Yes |

---

## Structure Rules (STR)

### STR0001: No continue statements

**Severity:** Warning  
**Category:** Structure  
**Code Fix:** No

Avoid using `continue` statements in loops. They make control flow harder to follow.

**Problem:**
```csharp
foreach (var item in items)
{
    if (item == null)
        continue; // STR0001
    
    ProcessItem(item);
}
```

**Solution:**
```csharp
foreach (var item in items)
{
    if (item != null)
    {
        ProcessItem(item);
    }
}
```

**Rationale:**
- Continue statements create non-obvious control flow
- Makes debugging and reasoning about loops harder
- Restructuring often leads to cleaner code

**Suppression:**
```csharp
#pragma warning disable STR0001
#pragma warning restore STR0001
```

---

### STR0002: Multiple return statements

**Severity:** Warning  
**Category:** Structure  
**Code Fix:** Yes

Methods should have a single exit point (one return statement) at the end.

**Problem:**
```csharp
public int Calculate(int x)
{
    if (x < 0)
        return -1; // STR0002
    
    if (x == 0)
        return 0; // STR0002
    
    return x * 2;
}
```

**Solution:**
```csharp
public int Calculate(int x)
{
    int result = x * 2;
    
    if (x < 0)
    {
        result = -1;
    }
    else if (x == 0)
    {
        result = 0;
    }
    
    return result;
}
```

**Rationale:**
- Single exit point makes debugging predictable
- Set one breakpoint to see all return values
- Easier to add logging or cleanup code

**Suppression:**
```csharp
#pragma warning disable STR0002
#pragma warning restore STR0002
```

---

### STR0003: Return in void method

**Severity:** Warning  
**Category:** Structure  
**Code Fix:** Yes

Void methods should not use return statements to exit early. Use structured control flow instead.

**Problem:**
```csharp
public void ProcessData(Data data)
{
    if (data == null)
        return; // STR0003
    
    // Process data
}
```

**Solution:**
```csharp
public void ProcessData(Data data)
{
    if (data != null)
    {
        // Process data
    }
}
```

**Rationale:**
- Consistent control flow patterns
- Easier to add additional conditions
- More explicit logic structure

**Suppression:**
```csharp
#pragma warning disable STR0003
#pragma warning restore STR0003
```

---

### STR0004: If-else chains

**Severity:** Warning  
**Category:** Structure  
**Code Fix:** Yes

Excessive if-else chains should be refactored. Consider using switch expressions, pattern matching, or lookup tables.

**Problem:**
```csharp
if (status == Status.Active)
{
    result = "Active";
}
else if (status == Status.Pending)
{
    result = "Pending";
}
else if (status == Status.Completed)
{
    result = "Completed";
}
else
{
    result = "Unknown";
}
```

**Solution:**
```csharp
var result = status switch
{
    Status.Active => "Active",
    Status.Pending => "Pending",
    Status.Completed => "Completed",
    _ => "Unknown"
};
```

**Suppression:**
```csharp
#pragma warning disable STR0004
#pragma warning restore STR0004
```

---

### STR0005: Return in nested block

**Severity:** Warning  
**Category:** Structure  
**Code Fix:** Yes

Return statements inside nested blocks (loops, conditions) make control flow harder to follow. Use result variables instead.

**Problem:**
```csharp
public int FindIndex(int[] items, int target)
{
    for (int i = 0; i < items.Length; i++)
    {
        if (items[i] == target)
            return i; // STR0005
    }
    return -1;
}
```

**Solution:**
```csharp
public int FindIndex(int[] items, int target)
{
    int result = -1;
    bool found = false;
    
    for (int i = 0; i < items.Length && !found; i++)
    {
        if (items[i] == target)
        {
            result = i;
            found = true;
        }
    }
    
    return result;
}
```

**Suppression:**
```csharp
#pragma warning disable STR0005
#pragma warning restore STR0005
```

---

### STR0006: No else if

**Severity:** Warning  
**Category:** Structure  
**Code Fix:** Yes

Avoid using else-if chains. Use switch expressions or restructure the logic.

**Problem:**
```csharp
if (x == 1)
{
    result = "One";
}
else if (x == 2) // STR0006
{
    result = "Two";
}
else
{
    result = "Other";
}
```

**Solution:**
```csharp
var result = x switch
{
    1 => "One",
    2 => "Two",
    _ => "Other"
};
```

**Suppression:**
```csharp
#pragma warning disable STR0006
#pragma warning restore STR0006
```

---

### STR0007: Suspicious regions

**Severity:** Warning  
**Category:** Structure  
**Code Fix:** No

Regions that may indicate code organization issues. Large regions or regions with suspicious names may need refactoring.

**Problem:**
```csharp
#region Hacks // STR0007
// Code that should be refactored
#endregion
```

**Solution:**

Refactor the code instead of hiding it in a region. Consider extracting to separate classes or methods.

**Suppression:**
```csharp
#pragma warning disable STR0007
#pragma warning restore STR0007
```

---

### STR0008: One type per file

**Severity:** Warning  
**Category:** Structure  
**Code Fix:** No

Each file should contain only one type (class, struct, interface, etc.). This improves code organization and makes files easier to find.

**Problem:**
```csharp
// In a single file:
public class Customer // STR0008
{
}

public class Order // STR0008
{
}
```

**Solution:**

Split into separate files:

`Customer.cs`:
```csharp
public class Customer
{
}
```

`Order.cs`:
```csharp
public class Order
{
}
```

**Exceptions:**
- Nested classes are allowed within their parent class file

**Suppression:**
```csharp
#pragma warning disable STR0008
#pragma warning restore STR0008
```

---

### STR0009: Nesting depth exceeded

**Severity:** Warning  
**Category:** Structure  
**Code Fix:** No

Code nesting should not exceed 3 levels (soft limit) or 6 levels (hard limit). Deep nesting indicates complex logic that should be refactored.

**Problem:**
```csharp
if (condition1)
{
    if (condition2)
    {
        if (condition3)
        {
            if (condition4) // STR0009 - 4 levels
            {
                // Too deep
            }
        }
    }
}
```

**Solution:**
```csharp
if (!condition1 || !condition2 || !condition3)
{
    return;
}

if (condition4)
{
    // Logic here
}
```

Or extract to separate methods:
```csharp
if (ShouldProcess(condition1, condition2, condition3))
{
    ProcessCondition4(condition4);
}
```

**Rationale:**
- At 6 levels (24 character indent), code is too complex to reason about
- Deep nesting indicates logic that should be extracted

**Suppression:**
```csharp
#pragma warning disable STR0009
#pragma warning restore STR0009
```

---

### STR0010: Missing argument validation

**Severity:** Warning  
**Category:** Structure  
**Code Fix:** No

Public methods must validate their arguments before processing.

**Problem:**
```csharp
public void ProcessData(string input, List<int> values) // STR0010
{
    // No validation - goes straight to processing
    var result = input.ToUpper();
}
```

**Solution:**
```csharp
public void ProcessData(string input, List<int> values)
{
    if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentNullException(nameof(input));
    
    if (values == null)
        throw new ArgumentNullException(nameof(values));
    
    if (values.Count == 0)
        throw new ArgumentException("Values cannot be empty", nameof(values));
    
    // Now process
    var result = input.ToUpper();
}
```

**Rationale:**
- Fail fast with clear error messages
- Documents method contracts
- Prevents null reference exceptions deep in code

**Suppression:**
```csharp
#pragma warning disable STR0010
#pragma warning restore STR0010
```

---

## Style Rules (STY)

### STY0001: End-of-line comments

**Severity:** Warning  
**Category:** Style  
**Code Fix:** Yes

End-of-line comments make code harder to read and maintain. Place comments on their own line above the code they describe.

**Problem:**
```csharp
int timeout = 5000; // Timeout in milliseconds // STY0001
```

**Solution:**
```csharp
// Timeout in milliseconds
int timeout = 5000;
```

**Suppression:**
```csharp
#pragma warning disable STY0001
#pragma warning restore STY0001
```

---

### STY0002: Null-forgiving operator

**Severity:** Warning  
**Category:** Style  
**Code Fix:** No

The null-forgiving operator (!) suppresses null warnings but can hide null reference bugs. Use proper null checks or assertions instead.

**Problem:**
```csharp
string name = GetName()!; // STY0002
```

**Solution:**
```csharp
var name = GetName();
if (name != null)
{
    // Use name
}

// Or with pattern matching
if (GetName() is { } name)
{
    // Use name
}
```

**Suppression:**
```csharp
#pragma warning disable STY0002
#pragma warning restore STY0002
```

---

### STY0003: Avoid dynamic keyword

**Severity:** Warning  
**Category:** Style  
**Code Fix:** No

The `dynamic` keyword bypasses compile-time type checking and should be avoided. It can lead to runtime errors that would otherwise be caught at compile time.

**Problem:**
```csharp
dynamic value = GetValue();
var result = value.SomeMethod(); // No compile-time checking!

public dynamic Process(dynamic input) // STY0003
{
    return input.Transform();
}
```

**Solution:**
```csharp
object value = GetValue();
if (value is ITransformable transformable)
{
    var result = transformable.SomeMethod();
}

// Or use generics
public T Process<T>(T input) where T : ITransformable
{
    return input.Transform();
}
```

**Why Avoid Dynamic?**
- No compile-time type checking: Errors only appear at runtime
- No IntelliSense: IDE cannot help with method/property names
- Performance overhead: Dynamic dispatch is slower than static dispatch
- Harder to refactor: Renaming members won't update dynamic code
- Testing burden: Requires more runtime testing to catch type errors

**When Dynamic Might Be Necessary:**
- COM interop with late-bound Office automation
- Interop with dynamic languages (Python, JavaScript via scripting)
- Reflection-heavy scenarios where types are truly unknown

**Suppression:**
```csharp
#pragma warning disable STY0003 // Required for COM interop with Excel
dynamic excelApp = Activator.CreateInstance(Type.GetTypeFromProgID("Excel.Application"));
#pragma warning restore STY0003
```

---

### STY0004: Field naming convention

**Severity:** Warning  
**Category:** Style  
**Code Fix:** Yes

Fields must follow the naming convention with appropriate prefixes.

| Field Type | Prefix | Example |
|------------|--------|---------|
| Private instance | `m` | `mCustomerName` |
| Private static | `ps` | `psInstanceCount` |
| Private static readonly | `sm` | `smDefaultLogger` |
| Public instance | `pm` | `pmDisplayName` |

**Problem:**
```csharp
private string customerName; // STY0004 - missing 'm' prefix
private static int instanceCount; // STY0004 - missing 'ps' prefix
```

**Solution:**
```csharp
private string mCustomerName;
private static int psInstanceCount;
private static readonly ILogger smDefaultLogger;
```

**Rationale:**
- Instant scope recognition in large classes
- Prevents shadowing bugs
- Thread safety awareness (e.g., static vs instance)

**Suppression:**
```csharp
#pragma warning disable STY0004
#pragma warning restore STY0004
```

---

### STY0005: Non-nullable string initialized to null

**Severity:** Warning  
**Category:** Style  
**Code Fix:** Yes

Non-nullable strings must not be initialized to null. Use `string.Empty`, a meaningful default, or make the type nullable.

**Problem:**
```csharp
string result = null; // STY0005
```

**Solution:**
```csharp
// Option 1: Use string.Empty
string result = string.Empty;

// Option 2: Use meaningful default
string result = "Unknown";

// Option 3: Make it nullable if null has semantic meaning
string? result = null;
```

**Suppression:**
```csharp
#pragma warning disable STY0005
#pragma warning restore STY0005
```

---

### STY0006: Method naming convention

**Severity:** Warning  
**Category:** Style  
**Code Fix:** Yes

Methods must use PascalCase naming.

**Problem:**
```csharp
public void processData() // STY0006
{
}

public int calculate_total() // STY0006
{
}
```

**Solution:**
```csharp
public void ProcessData()
{
}

public int CalculateTotal()
{
}
```

**Suppression:**
```csharp
#pragma warning disable STY0006
#pragma warning restore STY0006
```

---

### STY0007: Region naming convention

**Severity:** Warning  
**Category:** Style  
**Code Fix:** No

Regions must follow specific naming patterns and encapsulate complete functional units.

**Prohibited patterns:**
```csharp
#region Constructors  // STY0007 - too generic
#region Properties    // STY0007 - too generic
#region Methods       // STY0007 - too generic
#region Fields        // STY0007 - too generic
```

**Required patterns:**
```csharp
#region SaveCommand command
// Command field, property, Execute, CanExecute all together
#endregion

#region Name property
// Backing field, property, change handlers all together
#endregion

#region CustomerChanged event
// Event field, event property, raise method all together
#endregion

#region Equality members
// Equals, GetHashCode together
#endregion

#region IDisposable implementation
// Interface implementation together
#endregion
```

**Suppression:**
```csharp
#pragma warning disable STY0007
#pragma warning restore STY0007
```

---

## Numeric Rules (NUM)

### NUM0001: Floating-point equality

**Severity:** Error  
**Category:** Numeric  
**Code Fix:** No

Direct equality comparisons between floating-point numbers (float, double) are unreliable due to precision issues. Use tolerance-based comparison methods instead.

**Problem:**
```csharp
if (value == 0.1) // NUM0001
{
    // ...
}

if (a != b) // NUM0001 when a or b is float/double
{
    // ...
}
```

**Solution:**
```csharp
if (DoubleExtensions.AreEffectivelyEqual(value, 0.1))
{
    // ...
}

if (!DoubleExtensions.AreEffectivelyEqual(a, b))
{
    // ...
}
```

**Exemptions:**
- Comparisons with integer types are allowed
- Comparisons with string types are allowed
- Test methods (marked with `[TestMethod]`, `[Fact]`, `[Theory]`, etc.) are exempt

**Suppression:**
```csharp
#pragma warning disable NUM0001
#pragma warning restore NUM0001
```

---

### NUM0002: Magic numbers

**Severity:** Warning  
**Category:** Numeric  
**Code Fix:** No

Numeric literals (magic numbers) make code harder to understand and maintain. Extract them to named constants with descriptive names.

**Problem:**
```csharp
int timeout = 7777; // NUM0002 - What does 7777 mean?

if (retryCount > 42) // NUM0002 - Why 42?
{
    // ...
}
```

**Solution:**
```csharp
private const int TimeoutMilliseconds = 7777;
private const int MaxRetryCount = 42;

int timeout = TimeoutMilliseconds;

if (retryCount > MaxRetryCount)
{
    // ...
}
```

**Allowed Integer Values:**
- Small integers: -10 to 20
- Common values: 24, 25, 30, 40, 50, 75
- Angle values: 45, 60, 90, 120, 135, 180, 225, 270, 315, 360
- Powers of 2: 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768
- Byte/word boundaries: 255, 65535
- Time-related: 1000, 2000, 3000, 5000, 60000, 120000, 3600, 86400
- Powers of 10: -10000, -1000, -100, 100, 10000, 100000, 1000000
- Buffer/collection sizes: 500, 8000
- MAX_PATH (Windows): 260
- Calendar: 365, 366
- Hash code primes: 23, 31, 37, 397

**Allowed Float Values:**
- Zero and one: 0.0, 1.0
- Common fractions: 0.01, 0.02, 0.05, 0.1, 0.2, 0.25, 0.33, 0.5, 0.67, 0.75 (and negatives)
- Percentage thresholds: 0.8, 0.85, 0.9, 0.95, 0.99, 0.999
- Small integers as doubles: -10.0 to 10.0
- Powers of 10 as doubles: -1000.0, -100.0, 100.0, 1000.0
- Angle values as doubles: 45.0, 60.0, 90.0, 180.0, 270.0, 360.0
- Scientific notation tolerances: 1e-10, 1e-9, 1e-8, 1e-7, 1e-6, 1e-5, 1e-4, 1e-3
- Pi-related: 3.14159, pi, 2pi

**Exemptions:**
- `const` declarations - Named constants are the solution
- `static readonly` fields - Named constants
- Enum values - Already semantic
- Attribute arguments - Often required to be compile-time constants
- Array and collection initializers - Data definitions
- Test methods - Test data is expected
- `GetHashCode` methods - Hash code calculations commonly use primes
- Hex literals (0x prefix) - Typically intentional bit masks or flags

**Suppression:**
```csharp
#pragma warning disable NUM0002
int specialValue = 12345; // Has a specific meaning documented elsewhere
#pragma warning restore NUM0002
```

---

## Encapsulation Rules (ENC)

### ENC0001: Avoid hiding with 'new'

**Severity:** Warning  
**Category:** Encapsulation  
**Code Fix:** Yes (when base is virtual)

Using the `new` keyword to hide an inherited member can cause confusion and unexpected behavior.

**Problem:**
```csharp
public class Animal
{
    public int GetSpeed() => 10;
}

public class Dog : Animal
{
    public new int GetSpeed() => 30; // ENC0001
}

// Confusing behavior:
Dog dog = new Dog();
Animal animal = dog;
dog.GetSpeed();    // Returns 30
animal.GetSpeed(); // Returns 10 - unexpected!
```

**Solution:**
```csharp
public class Animal
{
    public virtual int GetSpeed() => 10;
}

public class Dog : Animal
{
    public override int GetSpeed() => 30;
}

// Consistent behavior:
Dog dog = new Dog();
Animal animal = dog;
dog.GetSpeed();    // Returns 30
animal.GetSpeed(); // Returns 30 - as expected!
```

**Why Is This a Problem?**
- Polymorphism breaks: Base class references don't see the derived implementation
- Confusing behavior: Same object behaves differently based on reference type
- Hard to debug: Issues may only appear in certain code paths
- Code smell: Often indicates design issues in the class hierarchy

**Suppression:**
```csharp
#pragma warning disable ENC0001 // Intentional hiding - legacy base class cannot be modified
public new string GetData() => base.GetData().ToUpper();
#pragma warning restore ENC0001
```

---

### ENC0002: Avoid direct access to inherited fields

**Severity:** Warning  
**Category:** Encapsulation  
**Code Fix:** No

Derived classes should not directly access non-readonly fields inherited from base classes. Access to parent class state should be through properties or methods only.

**Problem:**
```csharp
public class BaseClass
{
    protected int mValue; // Mutable protected field
}

public class DerivedClass : BaseClass
{
    public void UpdateValue()
    {
        mValue = 42; // ENC0002: Direct access to inherited field
    }
}
```

**Solution:**
```csharp
public class BaseClass
{
    private int mValue;
    
    protected int Value
    {
        get => mValue;
        set => mValue = value;
    }
}

public class DerivedClass : BaseClass
{
    public void UpdateValue()
    {
        Value = 42; // Compliant: Using property
    }
}
```

**Readonly Fields Are Allowed:**
```csharp
public class BaseClass
{
    protected readonly int SmMaxValue = 100; // Readonly is safe
}

public class DerivedClass : BaseClass
{
    public int GetMax()
    {
        return SmMaxValue; // Compliant: Readonly field
    }
}
```

**Rationale:**
- Base classes can add validation logic to field modifications later
- Base classes can add notification/event logic when fields change
- The internal implementation of base classes can be changed without breaking derived classes

**Suppression:**
```csharp
#pragma warning disable ENC0002 // Direct field access required for performance
mValue = newValue;
#pragma warning restore ENC0002
```

---

### ENC0003: Avoid public/protected fields

**Severity:** Warning  
**Category:** Encapsulation  
**Code Fix:** Yes

Public and protected fields break encapsulation. Fields should be private; expose state through properties instead.

**Problem:**
```csharp
public class Person
{
    public string Name; // ENC0003: Should be a property
    public int Age; // ENC0003: Should be a property
    protected double mSalary; // ENC0003: Should be a property
}
```

**Solution:**
```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    private double mSalary;
    protected double Salary
    {
        get => mSalary;
        set => mSalary = value;
    }
}
```

**Exemptions:**
1. **Structs** - Value types commonly use public fields for performance and interop
2. **Records** - Use compiler-generated properties by design
3. **Types with `[StructLayout]` attribute** - Interop scenarios requiring specific memory layout
4. **WPF types** - Classes deriving from `DependencyObject`, `FrameworkElement`, etc.
5. **Readonly/const fields** - Immutable fields are safe to expose
6. **Static fields** - Class-level state (different encapsulation concern)

**Suppression:**
```csharp
#pragma warning disable ENC0003 // Public field required for legacy serialization compatibility
public string LegacyData;
#pragma warning restore ENC0003
```

---

### ENC0004: Missing interface I prefix

**Severity:** Warning  
**Category:** Encapsulation  
**Code Fix:** Yes

Interfaces must be prefixed with `I`.

**Problem:**
```csharp
public interface Repository // ENC0004
{
    void Save();
}

public interface Serializable // ENC0004
{
    string Serialize();
}
```

**Solution:**
```csharp
public interface IRepository
{
    void Save();
}

public interface ISerializable
{
    string Serialize();
}
```

**Rationale:**
- Instant recognition that you're dealing with a contract, not an implementation
- Prevents naming collisions (e.g., `Repository` interface vs `Repository` class)
- IDE benefits: All interfaces sort together in alphabetical listings

**Suppression:**
```csharp
#pragma warning disable ENC0004
#pragma warning restore ENC0004
```

---

## Universal Standards

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Types | PascalCase | `CustomerOrder`, `IDataService` |
| Interfaces | I + PascalCase | `IRepository`, `ILogger` |
| Methods/Functions | PascalCase | `CalculateTotal()`, `GetCustomerName()` |
| Properties | PascalCase | `FirstName`, `TotalAmount` |
| Parameters | lowerCamelCase | `customerName`, `orderDate` |
| Local Variables | lowerCamelCase | `totalAmount`, `isValid` |
| Constants | PascalCase | `MaxRetryAttempts`, `DefaultTimeout` |
| Enums | PascalCase | `OrderStatus`, `ColorType` |
| Enum Values | PascalCase | `Active`, `Pending`, `Completed` |

### Field Naming

| Field Type | Prefix | Example |
|------------|--------|---------|
| Private instance | `m` | `mCustomerName` |
| Private static | `ps` | `psInstanceCount` |
| Private static readonly | `sm` | `smDefaultLogger` |
| Public instance | `pm` | `pmDisplayName` |

### File Organization

1. **One Type Per File**: Each class, interface, struct, or enum gets its own file
2. **File Name Matches Type**: `CustomerOrder.cs` contains `class CustomerOrder`
3. **Logical Directory Structure**: Group related types by feature/module

### Documentation Standards

- All public types must be documented
- All public methods and properties must be documented
- Complex algorithms must be documented regardless of visibility

---

## C# Specific Standards

### var Usage

Use `var` when type is evident:

```csharp
// Good - type is obvious
var list = new List<string>();
var count = GetCount();

// Bad - type not evident
var data = GetData();
var result = Process(input);
```

### Nullable Reference Types

- Enable nullable reference types project-wide
- Use nullable annotations appropriately
- Never initialize non-nullable strings to null

```csharp
// Correct patterns
string name = string.Empty;
string label = "Default";
string? optional = null; // Only if nullable

// Wrong - never do this
string result = null; // PROHIBITED
```

### Pattern Matching

Prefer pattern matching when it improves readability:

```csharp
// Good
if (obj is Customer { IsActive: true, Orders.Count: > 0 } customer)
{
    ProcessCustomer(customer);
}

// Avoid confusing empty braces
if (obj is Customer { } customer) // What does {} mean?
```

### Properties

- Prefer auto-properties for simple cases
- Use backing fields only when logic is needed
- Expression-bodied members for simple getters

```csharp
public string Name { get; set; }
public string FullName => $"{FirstName} {LastName}";
```

---

## Enforcement & Tooling

### EditorConfig Integration

All rules can be configured via `.editorconfig`:

```ini
# Disable a rule
dotnet_diagnostic.STR0001.severity = none

# Change to error
dotnet_diagnostic.NUM0001.severity = error

# Change to suggestion
dotnet_diagnostic.STY0004.severity = suggestion
```

### Suppression Patterns

```csharp
// Single line
#pragma warning disable STR0001
code here
#pragma warning restore STR0001

// With explanation (recommended)
#pragma warning disable STR0001 // Required for legacy compatibility
code here
#pragma warning restore STR0001
```

### Code Review Checklist

- [ ] Single return rule followed
- [ ] No magic numbers
- [ ] Arguments validated
- [ ] Public APIs documented
- [ ] Naming conventions followed
- [ ] No continue statements
- [ ] No else-if chains
- [ ] Field prefixes correct
- [ ] Interfaces have I prefix

---

## License

MIT License

Copyright (c) 2025

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
