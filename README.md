# Sage Programming Language

## Overview

**Sage** is a statically typed, compiled programming language designed for high-performance applications and small-scale 2D games.
The compiler is implemented in **C# (.NET 8)** and operates as a full toolchain: it **transpiles** Sage source code (`.sg`) into optimized **C11** code, and then orchestrates a native C compiler (GCC) to produce a standalone **Windows Executable (`.exe`)**.

The project strictly follows **SOLID principles** and **Clean Code** architecture, utilizing design patterns such as the **Visitor Pattern** for AST traversal and code generation, ensuring the codebase remains modular and testable.

## Core Features

* **Battery Included:** Handles the entire build process from source to binary (`.exe`).
* **Standard Project Layout:** Enforces a clean `src`, `obj`, and `bin` directory structure.
* **C-style Syntax:** Familiar syntax with modern touches like explicit modules and arrow return types (`->`).
* **Strong Typing:** Support for explicit types (`i32`, `string`, `void`, etc.).
* **Modularity:** Code organization via `module` blocks and namespaced calls (`math::sum`).
* **Native Performance:** Leverages GCC optimization (`-O2`) for maximum runtime speed.
* **Developer Tooling:** Built-in debug outputs for Tokens (`.tok`) and Abstract Syntax Trees (`.ast`).

## Dependencies

Sage relies on an external C toolchain to finalize the build process.

* **GCC (GNU Compiler Collection):** **Required**
* **Why?** Sage operates as a transpiler. It converts `.sg` source code into optimized intermediate C code (`.c`) and automatically invokes `gcc` to compile and link the final executable (`.exe`).
* **Installation:** Ensure you have **MinGW-w64** (or a similar GCC distribution) installed and added to your system's `PATH`.
* **Verification:** Run `gcc --version` in your terminal to ensure the compiler is accessible.

## Compiler Pipeline

The Sage compiler follows a multi-stage pipeline to ensure reliability and performance:

1. **Lexer:** Converts source code into a stream of strictly typed `Tokens`.
2. **Parser:** Analyzes the token stream and builds a hierarchical **Abstract Syntax Tree (AST)**.
3. **Semantic Analyzer:** Validates types, scopes, and variable existence.
4. **Code Generator:** Traverses the AST to emit optimized C11 code (`.c`).
5. **Native Toolchain:** Invokes the system GCC to compile intermediate C code into a final binary.

## Project Structure & Usage

Sage enforces a standard directory layout to keep projects clean.

```text
MyProject/
   |-- bin/
   |     |-- main.exe     (Final Application)
   |-- obj/
   |     |-- main.sg.c    (Transpiled code)
   |     |-- main.sg.tok  (Debug tokens)
   |     |-- main.sg.ast  (Debug tree)
   |-- src/
   |     |-- main.sg      (Source code)
```

The compiler operates in two modes:

### 1. Sandbox Mode (Internal Development)

Intended for compiler development and "dogfooding".

* **Workflow:** Simply press `F5` (Debug) in Visual Studio.
* **Behavior:** The compiler automatically detects the internal `Sage/Sandbox/src` folder, compiles the code, and **immediately executes** the resulting binary for rapid feedback.

### 2. Release Mode (CLI)

Intended for end-users.

* **Usage:** `sage <path_to_file.sg>`
* **Behavior:**
1. Detects if the file is inside a `src` folder.
2. Automatically creates `../bin` and `../obj` directories relative to the project root.
3. Compiles the executable to `bin/`.

## Syntax Example

Below is an example of the current syntax features, including modules, typed functions, and string interpolation:

```rust
// src/main.sg
// Import the standard console module for text output
use console;

// Math module
// Groups related arithmetic functions under a single namespace,
// avoiding global symbol pollution and improving code organization.
module math
{
    // Returns the sum of two 32-bit integers
    func sum(a: i32, b: i32): i32
    {
        return a + b;
    }

    // Returns the subtraction result of two 32-bit integers
    func subtract(a: i32, b: i32): i32
    {
        return a - b;
    }

    // Returns the multiplication result of two 32-bit integers
    func multiply(a: i32, b: i32): i32
    {
        return a * b;
    }

    // Returns the integer division result of two 32-bit integers
    func divide(a: i32, b: i32): i32
    {
        return a / b;
    }
}

// Program entry point
// Execution starts here.
func main(): none
{
    // Local variables with explicit static types
    x: i32 = 40;
    y: i32 = 20;

    // Console output using namespaced function calls
    // String interpolation is resolved at compile time.
    console::print_line("Sum: {x} + {y} = {math::sum(x, y)}");
    console::print_line("Subtraction: {x} - {y} = {math::subtract(x, y)}");
    console::print_line("Multiplication: {x} * {y} = {math::multiply(x, y)}");
    console::print_line("Division: {x} / {y} = {math::divide(x, y)}");
}
```

## Generated C Output

The above Sage code transpiles to the following C code (intermediate artifact in `obj/`):

```c
/* --- Generated by Sage Compiler --- */
#include <stdio.h>
#include <stdint.h>
#include <stdbool.h>
#include <uchar.h>

/* --- Sage Type Definitions --- */
typedef int8_t i8;
typedef int16_t i16;
typedef int32_t i32;
typedef int64_t i64;
typedef uint8_t u8;
typedef uint16_t u16;
typedef uint32_t u32;
typedef uint64_t u64;
typedef float f32;
typedef double f64;
typedef bool b8;
typedef char c8;
typedef char16_t c16;
typedef char32_t c32;
typedef char* str;
typedef void none;

/* --- Generated Logic --- */
// use console;
// Module: math
i32 math_sum(i32 a, i32 b)
{
    return (a + b);
}
i32 math_subtract(i32 a, i32 b)
{
    return (a - b);
}
i32 math_multiply(i32 a, i32 b)
{
    return (a * b);
}
i32 math_divide(i32 a, i32 b)
{
    return (a / b);
}
none main()
{
    i32 x = 40;
    i32 y = 20;
    printf("Sum: %d + %d = %d\n", x, y, math_sum(x, y));
    printf("Subtraction: %d - %d = %d\n", x, y, math_subtract(x, y));
    printf("Multiplication: %d * %d = %d\n", x, y, math_multiply(x, y));
    printf("Division: %d / %d = %d\n", x, y, math_divide(x, y));
}
```

## Project Status

The project is in **Alpha**.

The architecture is stable, supporting:

* [x] Variable Declarations (`var`) & Assignments
* [x] Function Declarations with Arrow Syntax (`->`)
* [x] Modules & Namespaces (`::`)
* [x] Basic Arithmetic Expressions
* [x] String Interpolation (compiled to `printf`)
* [x] **Native Compilation (GCC Integration)**
* [x] **Sandbox Environment**

## License

This project is licensed under the **MIT License**.

© 2026 Kellvyn Sampaio — Sage Language Project