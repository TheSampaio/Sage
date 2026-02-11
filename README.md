# Sage Programming Language

## Overview

**Sage** is a statically typed, compiled programming language designed for high-performance, small-scale 2D games.  
Currently, its compiler is implemented in **C#** and acts as a **transpiler**, converting Sage source code (`.sg`) into optimized **C++** code.

The ultimate goal of Sage is to provide a syntax similar to C# and C++ with features tailored for game development, initially supporting procedural programming with plans for Object-Oriented Programming (OOP) in the future.

## Core Features

- **C-style Syntax:** Familiar and powerful syntax for developers.
- **Strong Typing:** Support for explicit types like `i32`, `u32`, `string`, etc.
- **String Interpolation:** Native support for complex string formatting (e.g., `"Result: {val}"`).
- **C++ Transpilation:** Generates native C++ code for maximum performance in games.
- **Semantic Validation:** Built-in symbol table and analyzer to catch errors before compilation.

## Compiler Pipeline

The Sage compiler follows a classic architecture to ensure code reliability:

1. **Lexer:** Tokenizes source code, handling complex symbols like `::` and `->`.
2. **Parser:** Builds an Abstract Syntax Tree (AST) representing the program structure.
3. **Semantic Analyzer:** Validates variable declarations, scopes, and types.
4. **Code Generator:** Translates the validated AST into standard C++17 code.

## Syntax Example

Below is a preview of how a Sage program looks:

``` cpp
// Main.sg
use Console;


function Main() -> i32
{
    // Example of how to print something to the console
    Console::Print("Hello World! Welcome to the Sage programming language!");


    // Example of declaring variables and operations
    i32 number01 = 5;
    i32 number02 = 5;
    i32 result = number01 + number02;


    // Example of printing the result to the console
    Console::Print("The result of {number01} and {number02} is: {result}");


    return 0;
}
```

## Project Status

The project is currently in its **Alpha** stage.  
The front-end (Lexer/Parser) and the semantic layer are fully functional for procedural code.  
The back-end successfully generates C++ boilerplate and logic.

---

## License

This project is licensed under the **MIT License**.

© 2026 Kellvyn Sampaio — Sage Language Project
