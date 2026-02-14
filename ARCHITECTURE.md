# 🌿 Sage Programming Language: Technical Documentation

**Sage** is a compiled (transpiled) programming language that utilizes C as its low-level target. Its architecture follows the classic compiler "pipeline" model, prioritizing modularity through the **Visitor Design Pattern**.

## 1. Compiler Architecture

The Sage compilation process is divided into 6 sequential stages. Each stage transforms the code into a more abstract representation or one closer to the hardware.

### A. Frontend (Analysis)

1. **Lexer (Lexical Analysis):** Breaks the raw source code string into meaningful units called `Tokens`.
2. **Parser (Syntactic Analysis):** Organizes tokens into a hierarchical tree structure called the **AST (Abstract Syntax Tree)**.

### B. Middle-end (Validation)

3. **Semantic Analyzer:** The "brain" of the compiler. It validates whether variables were declared, ensures types are compatible (Type Checking), and manages scopes via the `SymbolTable`.

### C. Backend (Synthesis)

4. **Code Generator:** Traverses the AST and emits equivalent C source code.
5. **Native Compiler (GCC):** The native toolchain transforms the generated C code into a functional binary (`.exe`).
6. **Process Executor:** Manages the execution of the final program within the Sandbox environment.

## 2. Project Structure

* **`/Ast`**: Defines the "backbone" of the language. Each class represents a specific construct (e.g., `BinaryExpressionNode`).
* **`/Core`**: Contains the heavy logic: `Lexer`, `Parser`, `SemanticAnalyzer`, and `CodeGenerator`.
* **`/Enums`**: Centralizes token categories (`TokenType`).
* **`/Interfaces`**: Defines the `IAstVisitor` contract, allowing new features (like an interpreter or optimizer) to be added without modifying the AST classes.
* **`/Utilities`**: Diagnostic tools such as `AstPrinter` and `CompilerLogger`.

## 3. Developer Guide: Adding New Features

To add a new feature (e.g., an `if` statement), follow this standard workflow:

### Step 1: The Token

Add the new token type to `TokenType.cs`.

```csharp
Keyword_If,
Keyword_Else,
```

Then, map it in `Lexer.cs` within the `Keywords` dictionary.

### Step 2: The AST Node

Create a new class in `/Ast` (e.g., `IfStatementNode.cs`) inheriting from `AstNode`. It should store the condition and the associated code blocks.

### Step 3: The Contract (IAstVisitor)

Add the `Visit` method for your new node to the `IAstVisitor<T>` interface.

> **Note:** This will trigger compilation errors in all visitors, which is intentional! It ensures you implement the logic across the entire pipeline.

### Step 4: The Parser

In `Parser.cs`, create a `ParseIfStatement` method. It should consume the `if` token, the `(` parenthesis, the expression, the `)` parenthesis, and the `{}` block.

### Step 5: Semantics and Generation

* In `SemanticAnalyzer.cs`, validate that the `if` condition evaluates to a boolean (or a supported type).
* In `CodeGenerator.cs`, emit the corresponding `if` statement in C.

## 4. Type System and Casting

Sage utilizes a **Static Typing** system with support for **Implicit Promotion** and **Explicit Casting**.

* **Promotion:** The compiler automatically allows an `i32` to be treated as an `f64` to prevent common errors in mathematical operations.
* **Casting (`as`):** Allows for forced conversion. At the C level, this is translated into a direct type cast: `(target_type)value`.

### SymbolTable

Unlike a simple lookup table, the Sage `SymbolTable` uses a `Stack<Dictionary<string, string>>`.

* Each dictionary represents a **Scope** (Global, Function, or Block).
* This allows variables with the same name to exist in different functions without conflict (Shadowing).

## 5. Code Conventions (Clean Code & SOLID)

1. **Single Responsibility:** The `Lexer` only handles text. The `Parser` only handles tokens. Logic is never mixed.
2. **Visitor Pattern:** AST classes are "data-only" nodes. All behavioral logic resides in the visitors (`CodeGenerator`, `SemanticAnalyzer`).
3. **Immutability:** Wherever possible, use `get-only` properties in AST nodes to ensure the tree is not accidentally modified after parsing.

## 6. Development Roadmap

1. **Booleans and Comparisons:** Implement `==`, `!=`, `<`, `>` tokens and the `b8` type.
2. **Control Flow:** Implement `if/else` logic and `while` loops.
3. **Function Table:** Create a global function registry to validate argument counts and return types across different modules.
4. **Arrays and Pointers:** Basic support for data collections and memory referencing.
