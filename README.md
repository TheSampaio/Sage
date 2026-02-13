# Sage Programming Language

## Overview

**Sage** is a statically typed, compiled programming language designed for high-performance applications and small-scale 2D games.
Currently, the compiler is implemented in **C# (.NET 8)** and acts as a **transpiler**, converting Sage source code (`.sg`) into optimized, standard **C99/C11** code.

The project has been recently refactored to strictly follow **SOLID principles** and **Clean Code** architecture, utilizing design patterns such as the **Visitor Pattern** for AST traversal and code generation.

## Core Features

- **Clean Architecture:** Compiler built with modularity and extensibility in mind (Lexer, Parser, Visitors).
- **C-style Syntax:** Familiar syntax with modern touches like explicit modules.
- **Strong Typing:** Support for explicit types (`i32`, `string`, `none`, etc.).
- **Modularity:** Code organization via `module` blocks and namespaced calls (`math::sum`).
- **C Transpilation:** Generates raw C code (`.c`) for maximum portability and performance.
- **Developer Tooling:** Built-in debug outputs for Tokens (`.tok`) and Abstract Syntax Trees (`.ast`).

## Compiler Pipeline

The Sage compiler follows a robust pipeline to ensure code reliability:

1.  **Lexer:** Converts source code into a stream of strictly typed `Tokens`.
2.  **Parser:** Analyzes the token stream and builds a hierarchical **Abstract Syntax Tree (AST)**.
3.  **Visitors:**
    * **AST Printer:** (Debug only) Visualizes the code structure.
    * **Code Generator:** Traverses the AST to emit optimized C code.

## Project Structure & Usage

The compiler operates in two modes:

### 1. Debug Mode (Development)
Intended for compiler development.
- **Input:** Automatically looks for `Assets/main.sg` in the project root.
- **Outputs (in `Assets/`):**
    - `main.sg.c`: The transpiled C code.
    - `main.sg.tok`: A JSON dump of all lexed tokens.
    - `main.sg.ast`: A visual text representation of the AST hierarchy.

### 2. Release Mode (CLI)
Intended for end-users.
- **Usage:** `Sage.exe <path_to_file.sg>`
- **Output:** Generates only the `.c` file in the same directory.

## Syntax Example

Below is an example of the current syntax features, including modules, typed functions, and string interpolation:

``` Sage
// main.sg
use console;

module math
{
    func sum(i32 a, i32 b): i32
    {
        return a + b;
    }

    func subtract(i32 a, i32 b): i32
    {
        return a - b;
    }

    func multiply(i32 a, i32 b): i32
    {
        return a * b;
    }

    func divide(i32 a, i32 b): i32
    {
        return a / b;
    }
}

func main(): none
{
    i32 number01 = 40;
    i32 number02 = 20;

    // Namespaced calls and string interpolation
    console::print_line("{number01} + {number02} = {math::sum(number01, number02)}");
    console::print_line("{number01} - {number02} = {math::subtract(number01, number02)}");
    console::print_line("{number01} * {number02} = {math::multiply(number01, number02)}");
    console::print_line("{number01} / {number02} = {math::divide(number01, number02)}");
}
```
## Generated C Output

The above Sage code transpiles to the following C code:

``` C
#include <stdio.h>
#include <stdint.h>
// Sage Standard Types
typedef int32_t i32;
typedef void none;
typedef char* string;

// --- Generated Code ---
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
void main()
{
    i32 number01 = 40;
    i32 number02 = 20;
    printf("%d + %d = %d\n", number01, number02, math_sum(number01, number02));
    printf("%d + %d = %d\n", number01, number02, math_subtract(number01, number02));
    printf("%d + %d = %d\n", number01, number02, math_multiply(number01, number02));
    printf("%d + %d = %d\n", number01, number02, math_divide(number01, number02));
}
```

## Project Status

The project is in **Alpha**.  
The architecture is stable, supporting:

- [x] Variable Declarations & Assignments
- [x] Function Declarations with Parameters & Return Types
- [x] Modules & Namespaces (`::`)
- [x] Basic Arithmetic Expressions
- [x] String Interpolation (compiled to `printf`)
- [x] Debug Visualization (Tokens/AST)

---

## License

This project is licensed under the **MIT License**.

© 2026 Kellvyn Sampaio — Sage Language Project