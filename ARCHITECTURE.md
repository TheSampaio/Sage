# Sage Architecture & Technical Documentation

**Sage** is a compiled (transpiled) programming language that utilizes C as its low-level target. Its architecture follows the classic compiler "pipeline" model, prioritizing modularity through the **Visitor Design Pattern**.

## 1. Compiler Architecture

The Sage compilation process is divided into 6 sequential stages. Each stage transforms the code into a more abstract representation or one closer to the hardware.

### A. Frontend (Analysis)

1. **Lexer (Lexical Analysis):** A single-pass scanner that breaks raw source code into strictly typed `Tokens`.
2. **Parser (Syntactic Analysis):** A recursive descent parser that organizes tokens into a hierarchical **Abstract Syntax Tree (AST)**, enforcing operator precedence.

### B. Middle-end (Validation)

3. **Semantic Analyzer:** The "brain" of the compiler. It validates identifier existence, manages nested scopes via the `SymbolTable`, and performs strict **Type Checking**.

### C. Backend (Synthesis)

4. **Code Generator:** Traverses the AST and emits optimized, standards-compliant C11 source code.
5. **Native Compiler (GCC):** The native toolchain transforms the generated C code into a functional standalone binary (`.exe`).
6. **Process Executor:** Manages the secure execution of the final program within the environment.

## 2. Project Structure

* **`/Ast`**: Defines the "backbone" of the language. Nodes are immutable data containers (e.g., `IfNode`, `BinaryExpressionNode`).
* **`/Core`**: Contains the core logic: `Lexer`, `Parser`, `SemanticAnalyzer`, and `CodeGenerator`.
* **`/Enums`**: Centralizes domain constants, primarily `TokenType`.
* **`/Interfaces`**: Defines the `IAstVisitor<T>` contract, allowing behavioral decoupling.
* **`/Utilities`**: Diagnostic and infrastructure tools such as `AstPrinter`, `CompilerLogger`, and `CompilerException`.

## 3. Developer Guide: Adding New Features

To implement a new language construct, follow the **Sage standard workflow**:

### Step 1: The Token

Register the new token in `TokenType.cs` and map it in `Lexer.cs`.

### Step 2: The AST Node

Create a new node class in `/Ast` using C# 12 primary constructors for immutability.

### Step 3: The Visitor Contract

Add the corresponding `Visit` method to `IAstVisitor<T>`. This ensures all visitors (CodeGen, Semantics, Printer) are updated to support the feature.

### Step 4: The Parser

Implement the recursive descent logic in `Parser.cs` to consume the new tokens and produce the AST node.

### Step 5: Semantics and Generation

* **`SemanticAnalyzer.cs`**: Define the "rules" (e.g., an `if` condition must be `b8`).
* **`CodeGenerator.cs`**: Define the C output (e.g., how a Sage `for` loop looks in C11).

## 4. Type System and Scoping

Sage utilizes a **Static Typing** system with a focus on ABI safety and predictable memory behavior.

* **Implicit Promotion:** Automatically allows safe conversions, such as `i32` to `f64`.
* **Explicit Casting (`as`):** Forces a type conversion, translated directly to a C-style cast.

### Scope Management (`SymbolTable`)

The `SymbolTable` uses a `Stack<Dictionary<string, string>>` to represent **Lexical Scoping**.

* **Entering a block** (`{`): Pushes a new dictionary onto the stack.
* **Exiting a block** (`}`): Pops the current scope.
* This mechanism prevents variable name collisions and handles **Shadowing** naturally.

## 5. Code Conventions (Clean Code & SOLID)

1. **Single Responsibility:** Each class has one job (e.g., `Lexer` only knows about characters and tokens).
2. **Open/Closed Principle:** The **Visitor Pattern** allows us to add new operations (like an Optimizer) without modifying the AST nodes.
3. **Standardized Diagnostics:** All errors follow the `file(line,col): error CODE: Message` format for IDE compatibility.
