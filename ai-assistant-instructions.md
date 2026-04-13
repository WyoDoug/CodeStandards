# AI Assistant Coding Standards Instructions

## Primary Directive

1. **Answer ONLY what is explicitly asked** - no more, no less
2. **When next step unclear, STOP and ASK**: "How would you like to proceed?"
3. **NEVER provide unsolicited solutions** - identify problems, wait for direction
4. **NO FLATTERY** - Skip "great idea", "absolutely", praise. Just answer.
5. **Reference the Coding Standards document** for all decisions

---

## Mandatory First Step

**FAMILIARIZE YOURSELF WITH THE CODING STANDARDS DOCUMENT**

- The Coding Standards document is your primary reference
- All code must comply with these standards
- When in doubt, consult the standards document

---

## Critical Enforcement Points - ZERO TOLERANCE

### 1. Single Return Rule (All Languages)

**Reference: Coding Standards - STR0002, STR0003, STR0005**

```csharp
// ✅ CORRECT - Only one return at end
public int Calculate(int x)
{
    int result = 0;
    
    if (x > 0)
    {
        result = x * 2;
    }
    
    return result;  // ONLY return
}

// ❌ WRONG - Multiple returns
public int Calculate(int x)
{
    if (x > 0) return x * 2;  // NO!
    return 0;  // NO!
}
```

**Void functions have NO return statement:**
```csharp
// ✅ CORRECT
public void ProcessData(Data data)
{
    if (data != null)
    {
        // Process
    }
    // Function ends naturally - NO return
}

// ❌ WRONG
public void ProcessData(Data data)
{
    if (data == null)
        return;  // NO early returns in void methods!
}
```

---

### 2. No Continue Statements

**Reference: Coding Standards - STR0001**

```csharp
// ❌ WRONG
foreach (var item in items)
{
    if (item == null)
        continue;  // NEVER use continue
    ProcessItem(item);
}

// ✅ CORRECT
foreach (var item in items)
{
    if (item != null)
    {
        ProcessItem(item);
    }
}
```

---

### 3. No Else-If Chains

**Reference: Coding Standards - STR0004, STR0006**

```csharp
// ❌ WRONG
if (x == 1)
    result = "One";
else if (x == 2)  // PROHIBITED
    result = "Two";
else
    result = "Other";

// ✅ CORRECT - Use switch expression
var result = x switch
{
    1 => "One",
    2 => "Two",
    _ => "Other"
};
```

---

### 4. No Magic Numbers

**Reference: Coding Standards - NUM0002**

```csharp
// ❌ WRONG
int timeout = 7777;  // What does 7777 mean?

// ✅ CORRECT
private const int TimeoutMilliseconds = 7777;
int timeout = TimeoutMilliseconds;
```

**Allowed values:** -10 to 20, powers of 2, common angles, time values (1000, 3600, 86400)

---

### 5. Field Naming Prefixes

**Reference: Coding Standards - STY0004**

| Field Type | Prefix | Example |
|------------|--------|---------|
| Private instance | `m` | `mCustomerName` |
| Private static | `ps` | `psInstanceCount` |
| Private static readonly | `sm` | `smDefaultLogger` |
| Public instance | `pm` | `pmDisplayName` |

```csharp
// ❌ WRONG
private string customerName;
private static int instanceCount;

// ✅ CORRECT
private string mCustomerName;
private static int psInstanceCount;
```

---

### 6. Argument Validation Required

**Reference: Coding Standards - STR0010**

```csharp
// ❌ WRONG - No validation
public void ProcessData(string input, List<int> values)
{
    var result = input.ToUpper();  // Will crash if null
}

// ✅ CORRECT
public void ProcessData(string input, List<int> values)
{
    if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentNullException(nameof(input));
    
    if (values == null)
        throw new ArgumentNullException(nameof(values));
    
    if (values.Count == 0)
        throw new ArgumentException("Values cannot be empty", nameof(values));
    
    var result = input.ToUpper();
}
```

---

### 7. No Floating-Point Equality

**Reference: Coding Standards - NUM0001 (SEVERITY: ERROR)**

```csharp
// ❌ WRONG - NEVER compare floats/doubles directly
if (value == 0.1)  // NUM0001 ERROR
if (a != b)        // NUM0001 ERROR when float/double

// ✅ CORRECT
if (DoubleExtensions.AreEffectivelyEqual(value, 0.1))
if (!DoubleExtensions.AreEffectivelyEqual(a, b))
```

---

### 8. Interface Naming

**Reference: Coding Standards - ENC0004**

```csharp
// ❌ WRONG
public interface Repository { }
public interface Serializable { }

// ✅ CORRECT
public interface IRepository { }
public interface ISerializable { }
```

---

### 9. No Public/Protected Fields

**Reference: Coding Standards - ENC0003**

```csharp
// ❌ WRONG
public class Person
{
    public string Name;  // Use property instead
    protected int mAge;  // Use protected property
}

// ✅ CORRECT
public class Person
{
    public string Name { get; set; }
    
    private int mAge;
    protected int Age
    {
        get => mAge;
        set => mAge = value;
    }
}
```

---

### 10. String Initialization

**Reference: Coding Standards - STY0005**

```csharp
// ❌ WRONG - NEVER initialize non-nullable string to null
string result = null;  // PROHIBITED

// ✅ CORRECT
string result = string.Empty;
string result = "Default";
string? result = null;  // Only if nullable type
```

---

### 11. Comments on Separate Lines

**Reference: Coding Standards - STY0001**

```csharp
// ❌ WRONG - End-of-line comment
int timeout = 5000; // Timeout in milliseconds

// ✅ CORRECT - Comment above
// Timeout in milliseconds
int timeout = 5000;
```

---

### 12. One Type Per File

**Reference: Coding Standards - STR0008**

```csharp
// ❌ WRONG - Multiple types in one file
public class Customer { }
public class Order { }

// ✅ CORRECT - Separate files
// Customer.cs
public class Customer { }

// Order.cs  
public class Order { }
```

---

### 13. Nesting Depth

**Reference: Coding Standards - STR0009**

- **Soft limit:** 3 levels
- **Hard limit:** 6 levels

```csharp
// ❌ WRONG - Too deep
if (a)
{
    if (b)
    {
        if (c)
        {
            if (d)  // 4 levels - refactor!
            {
            }
        }
    }
}

// ✅ CORRECT - Extract methods or flatten
if (!MeetsConditions(a, b, c))
    return;

ProcessD(d);
```

---

### 14. Avoid Dynamic Keyword

**Reference: Coding Standards - STY0003**

```csharp
// ❌ WRONG
dynamic value = GetValue();
var result = value.SomeMethod();

// ✅ CORRECT
object value = GetValue();
if (value is ITransformable transformable)
{
    var result = transformable.SomeMethod();
}
```

---

### 15. Avoid Null-Forgiving Operator

**Reference: Coding Standards - STY0002**

```csharp
// ❌ WRONG
string name = GetName()!;

// ✅ CORRECT
if (GetName() is { } name)
{
    // Use name
}
```

---

## Analyzer Rule Quick Reference

| ID | Name | Fix? |
|----|------|------|
| STR0001 | No continue statements | No |
| STR0002 | Multiple return statements | Yes |
| STR0003 | Return in void method | Yes |
| STR0004 | If-else chains | Yes |
| STR0005 | Return in nested block | Yes |
| STR0006 | No else if | Yes |
| STR0007 | Suspicious regions | No |
| STR0008 | One type per file | No |
| STR0009 | Nesting depth exceeded | No |
| STR0010 | Missing argument validation | No |
| STY0001 | End-of-line comments | Yes |
| STY0002 | Null-forgiving operator | No |
| STY0003 | Avoid dynamic keyword | No |
| STY0004 | Field naming convention | Yes |
| STY0005 | Non-nullable string = null | Yes |
| STY0006 | Method naming convention | Yes |
| STY0007 | Region naming convention | No |
| NUM0001 | Floating-point equality | No |
| NUM0002 | Magic numbers | No |
| ENC0001 | Avoid hiding with 'new' | Yes |
| ENC0002 | Direct inherited field access | No |
| ENC0003 | Public/protected fields | Yes |
| ENC0004 | Interface I prefix | Yes |

---

## Common Violations Checklist

When reviewing or writing code, check for:

- [ ] Multiple return statements
- [ ] Early returns in methods
- [ ] Continue statements in loops
- [ ] Else-if chains (use switch)
- [ ] Magic numbers without constants
- [ ] Missing field prefixes (m, ps, sm, pm)
- [ ] Missing argument validation
- [ ] Float/double equality comparisons
- [ ] Interfaces without I prefix
- [ ] Public/protected fields
- [ ] End-of-line comments
- [ ] Non-nullable strings initialized to null
- [ ] Multiple types in one file
- [ ] Deep nesting (> 3 levels)
- [ ] Dynamic keyword usage
- [ ] Null-forgiving operator (!)

---

## Response Format for Issues

When identifying issues:

```
Issues found (violates Coding Standards):
1. Multiple return statements at lines X, Y (STR0002)
2. Magic number '42' at line Z (NUM0002)
3. Missing argument validation for 'data' (STR0010)
4. Field 'name' should be 'mName' (STY0004)
```

Always reference the specific rule ID.

---

## Workflow Enforcement

- When implementing, **STOP after each step** and ASK how to proceed unless explicitly told to continue
- **NO PRAISE** or validation comments - just provide the answer
- If user lacks knowledge in an area (CSS, printing, etc), provide that expertise WITHOUT deviating from these standards
- Validate all inputs at method/property entry points
- Use nullable reference types appropriately

---

## Code Rejected If:

- Any early return found
- Multiple return statements exist
- Continue statements used
- Else-if chains present
- Magic numbers found
- Wrong field prefixes
- Missing argument validation
- Float/double direct comparison
- Interface missing I prefix
- Public/protected fields
- Inline comments found
- Nesting exceeds 3 levels (warning) / 6 levels (error)

**NO WIGGLE ROOM. NO INTERPRETATION. FOLLOW EXACTLY.**
