# Sage Programming Language

**Sage** is a statically typed, compiled programming language designed for high-performance applications and systems development. 
The compiler is implemented in **C# (.NET 8)** and operates as a full toolchain: it **transpiles** Sage source code (`.sg`) into optimized **C11** code and then orchestrates a native C compiler (GCC) to produce a standalone **Windows Executable (`.exe`)**.

The project strictly follows **SOLID principles** and **Clean Code** architecture, utilizing the **Visitor Pattern** for AST traversal, semantic analysis, and code generation.

## Core Features

* **Battery Included:** Handles the entire build process from source to binary (`.exe`).
* **Control Flow:** Full support for `if/else` branching, `while` loops, and C-style `for` loops.
* **Modern Type System:** Explicit types (`i32`, `f64`, `str`, `b8`) with support for **Constants** (`const`) and **Type Promotion**.
* **Boolean Logic:** Native `b8` type with logical operators (`&&`, `||`, `!`) and comparisons.
* **Modularity:** Code organization via `module` blocks and namespaced calls (`math::sum`).
* **Developer Tooling:** Standardized **JSON** debug outputs for Tokens (`.tok.json`) and Abstract Syntax Trees (`.ast.json`).

## Dependencies

Sage relies on an external C toolchain to finalize the build process.

* **GCC (GNU Compiler Collection):** **Required**
* **Installation:** Ensure you have **MinGW-w64** installed and added to your system's `PATH`.

## Compiler Pipeline

1. **Lexer:** Converts source code into a sequence of strictly typed `Tokens`.
2. **Parser:** Builds a hierarchical **Abstract Syntax Tree (AST)** using recursive descent.
3. **Semantic Analyzer:** Manages scopes via `SymbolTable`, validates mutability (`const`), and performs strict **Type Checking**.
4. **Code Generator:** Traverses the AST to emit optimized, human-readable C11 code.
5. **Native Toolchain:** Invokes GCC to compile the intermediate C code into a final binary.

## Syntax Example (Control Flow & Logic)

```rust
use console;

func main(): none
{
    var loops: i32 = 0;
    const max: i32 = 5;

    while (loops < max)
    {
        if (loops % 2 == 0)
        {
            console::print_line("Even: {loops}");
        }
        else
        {
            console::print_line("Odd: {loops}");
        }
        loops++;
    }
}
```

## Project Status

The project is currently in **v0.2.0 (Alpha)**.

* [x] **Variable Declarations** (`var`) & **Constants** (`const`)
* [x] **Explicit Casting** (`as`)
* [x] **Control Flow** (`if`, `else`, `while`, `for`)
* [x] **Logical & Comparison Operators** (`&&`, `||`, `!`, `==`, `!=`, etc.)
* [x] **Modules & Namespaces** (`::`)
* [x] **String Interpolation** (compiled to `printf`)
* [x] **Native Compilation** (GCC Integration)
* [ ] **Arrays and Pointers** (Next Milestone)

## License

This project is licensed under the **MIT License**.

© 2026 Kellvyn Sampaio — Sage Language Project