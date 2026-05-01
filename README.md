# Csp

**Csp** is a lightweight compile-time extension layer for C#, built on top of Roslyn.

It introduces new syntax (such as `alias`) that enables **scoped, zero-cost code transformations**, while still producing valid standard C#.

---

## ✨ Overview

Csp works as a **source-to-source compiler**:

```text
.csp → transform → .cs
```

All features are resolved at compile time.
The generated output is pure C# with no runtime dependency.

---

## 🎯 Motivation

C# provides a powerful and expressive programming model.
In practice, there are scenarios where developers may want:

* shorter names for long expressions
* localized, intention-revealing aliases
* lightweight DSL-like structures
* compile-time substitution without runtime cost

Csp explores these patterns as an optional layer on top of C#.

---

## ✨ Features

### 🔹 `alias` — Scoped Compile-Time Substitution

Define a name that is replaced at compile time.

```csharp
alias print = Console.WriteLine;

print("Hello");
```

↓

```csharp
Console.WriteLine("Hello");
```

---

### 🔹 Scoped & Shadowable

```csharp
alias print = Console.WriteLine;

print("User log");

{
    alias print = Debug.WriteLine;
    print("Debug log");
}
```

↓

```csharp
Console.WriteLine("User log");

{
    Debug.WriteLine("Debug log");
}
```

---

### 🔹 Expression-Level Mapping

```csharp
alias now = DateTime.Now.ToString;

Console.WriteLine(now());
```

↓

```csharp
Console.WriteLine(DateTime.Now.ToString());
```

---

## 🚀 Getting Started

### 1. Create a `.csp` file

```csharp
using System.Diagnostics;

public static class Demo
{
    public static void PrintDemo(string text)
    {
        alias print = Console.WriteLine;
        print($"[Button] {text}");
    }

    public static void CallDemo()
    {
        alias now = DateTime.Now.ToString;

        Console.WriteLine(now());
        Console.WriteLine(now());
    }

    public static void ScopeDemo()
    {
        alias print = Console.WriteLine;

        print("User log");

        {
            alias print = Debug.WriteLine;
            print("Debug log");
        }
    }
}
```

---

### 2. Run the compiler

```bash
csp Sample.csp
```

or specify output:

```bash
csp Sample.csp -o Sample.cs
```

---

### 3. Generated output

```csharp
using System.Diagnostics;

public static class Demo
{
    public static void PrintDemo(string text)
    {
        Console.WriteLine($"[Button] {text}");
    }

    public static void CallDemo()
    {
        Console.WriteLine(DateTime.Now.ToString());
        Console.WriteLine(DateTime.Now.ToString());
    }

    public static void ScopeDemo()
    {
        Console.WriteLine("User log");
        {
            Debug.WriteLine("Debug log");
        }
    }
}
```

---

## 💡 What Makes `alias` Different?

| Feature                  | alias   | variable  | delegate  | using    |
| ------------------------ | ------- | --------- | --------- | -------- |
| Scope                    | ✔ local | ✔ local   | ✔ local   | ✖ global |
| Runtime cost             | ✔ none  | ✖ yes     | ✖ yes     | ✔ none   |
| Works for expressions    | ✔ yes   | ✔ limited | ✔ limited | ✖ no     |
| Compile-time replacement | ✔ yes   | ✖ no      | ✖ no      | ✖ no     |

---

## 🛠 How It Works

1. Parse `.csp` using Roslyn
2. Collect `alias` declarations into scoped maps
3. Rewrite syntax tree (identifier / invocation)
4. Remove `alias` statements
5. Format and output valid C#

---

## ⚠️ Limitations

* `alias` is not valid C# syntax (requires preprocessing)
* Currently syntax-level only (no semantic analysis)
* Recursive alias not yet supported
* Not all expression positions are covered yet

---

## 🔮 Future Plans

* Recursive alias expansion
* Full expression replacement
* Project integration (`.csproj`)
* Watch mode
* IDE support (Roslyn analyzer)

---

## 📦 CLI

```bash
csp input.csp
csp input.csp -o output.cs
csp input.csp -o ./out/
```

---

## 📄 License

MIT
