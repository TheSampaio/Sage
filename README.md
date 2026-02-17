# Sage Programming Language

<p align="center">
<img src="Assets/Images/sage-programming-language-logo.png" alt="Sage Logo">
</p>

**Sage** is a statically typed, compiled programming language designed for high-performance applications and systems development.
The compiler is implemented in **C# (.NET 9)** and operates as a full toolchain: it **transpiles** Sage source code (`.sg`) into optimized **C11** code and then orchestrates a native C compiler (GCC) to produce a standalone **Windows Executable (`.exe`)**.

The project strictly follows **SOLID principles** and **Clean Code** architecture, utilizing the **Visitor Pattern** for AST traversal, semantic analysis, and code generation.

## Core Features

* **Native Compilation:** Handles the entire build process from source to optimized binary (`.exe`).
* **Foreign Function Interface (FFI):** Support for `extern` declarations, allowing direct interoperability with C standard libraries and external files.
* **Tree-Shaking Code Gen:** Smart header management that only includes necessary C headers (`stdio.h`, `stdint.h`, etc.) based on actual code usage.
* **Modern Type System:** Explicit types (`i32`, `f64`, `str`, `b8`) with support for **Constants** (`const`) and **Explicit Casting** (`as`).
* **Modularity:** First-class support for `module` blocks and namespaced calls (`math::sum`) to manage large codebases.
* **Developer Tooling:** Standardized **JSON** debug outputs for Tokens (`.tok.json`) and Abstract Syntax Trees (`.ast.json`).

## Dependencies

Sage acts as a transpiler (Source-to-Source), converting your `.sg` code into optimized C. To finalize the build into a native executable, it relies on an external C toolchain.

* **GCC (GNU Compiler Collection):** [Official Website]() — **Required**

### Linux (Debian/Ubuntu) Installation

On Debian-based systems, the easiest way to get everything you need is by installing the `build-essential` package.

1. Open your terminal.
2. Run the following command:
```bash
sudo apt update && sudo apt install build-essential
```

3. Verify the installation by typing `gcc --version`.

### Windows (MinGW-w64) Installation

For Windows, we recommend using **MinGW-w64** via MSYS2 for a modern and stable environment.

#### 1. Install MSYS2

* Download and run the installer from [msys2.org]().
* Once finished, open the **MSYS2 UCRT64** terminal and run:
```bash
pacman -S mingw-w64-ucrt-x86_64-gcc
```

#### 2. Adding to System PATH

To run `gcc` from any terminal (PowerShell, CMD, or VS Code), you must add it to your Environment Variables:

1. Press `Win + S` and search for **"Edit the system environment variables"**.
2. Click **Environment Variables** (bottom right).
3. Under **System variables**, find the **Path** variable and click **Edit**.
4. Click **New** and paste the path to your MinGW bin folder (usually `C:\msys64\ucrt64\bin`).
5. Click **OK** on all windows.
6. **Restart your terminal** and type `gcc --version` to confirm.

## Compiler Pipeline

1. **Lexer:** Converts source code into a sequence of strictly typed `Tokens`.
2. **Parser:** Builds a hierarchical **Abstract Syntax Tree (AST)** using recursive descent.
3. **Semantic Analyzer:** Manages scopes via `SymbolTable`, validates mutability (`const`), and performs strict **Type Checking**.
4. **Code Generator:** Traverses the AST to emit optimized C11 code, handling ABI compliance and C-style type mapping.
5. **Native Toolchain:** Invokes GCC to compile the intermediate C code into a final binary.

## Syntax Example

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

The project is currently in **v0.4.0 (Alpha)**.

- [x] **CLI & Project System** (`new`, `build`, `run`, `--version`)
- [x] **Variable Declarations** (`var`) & **Constants** (`const`)
- [x] **Control Flow** (`if`, `else`, `while`, `for`)
- [x] **FFI / External Functions** (`extern`)
- [x] **Header Tree-Shaking** (Optimized C output)
- [x] **Logical & Comparison Operators** (`&&`, `||`, `!`, `==`, etc.)
- [x] **Modules & Namespaces** (`::`)
- [x] **String Interpolation** (compiled to `printf`)
- [x] **Native Compilation** (GCC Integration)
- [ ] **Arrays and Pointers** (Next Milestone)
- [ ] **Structs and Custom Types** (Planned)

## License

This project is licensed under the **Apache License 2.0**.

### Why Apache 2.0?
Sage is built for the community. This license allows you to:
* **Use for any purpose:** Commercial or private.
* **Modify and Distribute:** You can fork and change the code.
* **Attribution:** You must keep the original copyright notice and credit Kellvyn Sampaio.

© 2026 Kellvyn Sampaio — Sage Language Project