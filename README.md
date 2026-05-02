# Csp

**Csp introduces compile-time language extensions for C#.**

It allows new syntax constructs that are transformed into standard C# code during compilation.

---

# 🎯 Goal

Csp enables writing **more expressive C# code** by introducing lightweight syntax extensions that are resolved at compile time.

---

# ✨ Current Feature: `def`

`def` defines a **compile-time expression replacement rule**.

---

## 🔹 Basic usage

```csharp id="m7q8zv"
def print = Console.WriteLine;

print("Hello");
```

### ↓ becomes

```csharp id="x2n4kd"
Console.WriteLine("Hello");
```

---

## 🔹 Expression support

`def` works with arbitrary expressions:

```csharp id="q9k1pm"
def now = DateTime.Now.ToString;
def add = () => 1 + 2;
```

---

## 🔹 Usage

```csharp id="r4v8dn"
Console.WriteLine(now());
Console.WriteLine(add());
```

---

## 🔹 Scoped behavior

```csharp id="z1k7qp"
def log = Console.WriteLine;

log("global");

{
    def log = Debug.WriteLine;
    log("inner");
}
```

---

## 🔹 Restrictions

* Forward references are not allowed
* Circular references are not allowed
* Expansion happens at compile time
* Generated output is valid C#

---

# 🧠 Important Note

Csp does not change runtime semantics.

It only transforms syntax before compilation.

---

# 🚀 Future extensions

Csp may introduce additional compile-time syntax constructs beyond `def`.

These will follow the same principle:

> **compile-time transformation into valid C#**

---

# 📦 CLI

```bash
csp input.csp
```
or
```bash
csp input.csp -o output.cs
```
or
```bash
csp input.csp -o ./out/output.cs
```
---

## 📄 License

MIT
