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

<body style="background:#1e1e1e; color:#d4d4d4; font-family:Consolas, Monaco, 'Courier New', monospace; padding:20px;">

<div style="position:relative; max-width:900px;">
    <button
    onclick="copySageCode(this)"
    style="
        position:absolute;
        top:8px;
        right:8px;
        background:#2d2d2d;
        color:#d4d4d4;
        border:1px solid #3c3c3c;
        padding:4px 8px;
        border-radius:4px;
        cursor:pointer;
        display:flex;
        align-items:center;
        gap:6px;
    "
    >
        <svg
    aria-hidden="true"
    width="16"
    height="16"
    viewBox="0 0 16 16"
    xmlns="http://www.w3.org/2000/svg"
    fill="#ffffff"
>
    <path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"></path>
    <path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"></path>
</svg>
    </button>
    <pre
        id="sage-code"
        style="
            background:#1e1e1e;
            padding:32px 16px 16px 16px;
            border-radius:6px;
            overflow-x:auto;
        "
    ><span style="color:#6A9955;">// Main.sg</span>
<span style="color:#569CD6;">use</span> <span style="color:#9CDCFE;">Console</span>;
<br>
<span style="color:#569CD6;">function</span> <span style="color:#DCDCAA;">Main</span>() -&gt; <span style="color:#4EC9B0;">i32</span>
{
    <span style="color:#6A9955;">// Example of how to print something to the console</span>
    <span style="color:#9CDCFE;">Console</span>::<span style="color:#DCDCAA;">Print</span>(<span style="color:#CE9178;">"Hello World! Welcome to the Sage programming language!"</span>);
<br>
    <span style="color:#6A9955;">// Example of declaring variables and operations</span>
    <span style="color:#4EC9B0;">i32</span> number01 = <span style="color:#B5CEA8;">5</span>;
    <span style="color:#4EC9B0;">i32</span> number02 = <span style="color:#B5CEA8;">5</span>;
    <span style="color:#4EC9B0;">i32</span> result = number01 + number02;
<br>
    <span style="color:#6A9955;">// Example of printing the result to the console</span>
    <span style="color:#9CDCFE;">Console</span>::<span style="color:#DCDCAA;">Print</span>(<span style="color:#CE9178;">"The result of {<span style="color:#cccccc;">number01</span>} and {<span style="color:#cccccc;">number02</span>} is: {<span style="color:#cccccc;">result</span>}"</span>);
<br>
    <span style="color:#569CD6;">return</span> <span style="color:#B5CEA8;">0</span>;
}
    </pre>
</div>

<script>
function copySageCode(button) {
    const code = document.getElementById("sage-code").innerText;
    navigator.clipboard.writeText(code);

    const label = button.querySelector("span");
    const original = label.innerText;
    label.innerText = "Copied";
    setTimeout(() => label.innerText = original, 1200);
}
</script>

</body>


## Project Status

The project is currently in its **Alpha** stage.  
The front-end (Lexer/Parser) and the semantic layer are fully functional for procedural code.  
The back-end successfully generates C++ boilerplate and logic.

---

## License

This project is licensed under the **MIT License**.

© 2026 Kellvyn Sampaio — Sage Language Project
