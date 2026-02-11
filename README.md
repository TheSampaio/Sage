<body>
    <header>
        <h1>Sage Programming Language</h1>
        <p></p>
    </header>
    <section id="overview">
        <h2>Overview</h2>
        <p>
            <strong>Sage</strong> is a statically typed, compiled programming language designed for high-performance, small-scale 2D games. Currently, its compiler is implemented in <strong>C#</strong> 
            and acts as a <strong>transpiler</strong>, converting Sage source code (.sg) into optimized <strong>C++</strong> code.
        </p>
        <p>
            The ultimate goal of Sage is to provide a syntax similar to C# and C++ with features tailored for game development, 
            initially supporting procedural programming with plans for Object-Oriented Programming (OOP) in the future.
        </p>
    </section>
    <section id="features">
        <h2>Core Features</h2>
        <ul>
            <li><strong>C-style Syntax:</strong> Familiar and powerful syntax for developers.</li>
            <li><strong>Strong Typing:</strong> Support for explicit types like <code>i32</code>, <code>u32</code>, <code>string</code>, etc.</li>
            <li><strong>String Interpolation:</strong> Native support for complex string formatting (e.g., <code>"Result: {val}"</code>).</li>
            <li><strong>C++ Transpilation:</strong> Generates native C++ code for maximum performance in games.</li>
            <li><strong>Semantic Validation:</strong> Built-in symbol table and analyzer to catch errors before compilation.</li>
        </ul>
    </section>
    <section id="compiler-pipeline">
        <h2>Compiler Pipeline</h2>
        <p>The Sage compiler follows a classic architecture to ensure code reliability:</p>
        <ol>
            <li><strong>Lexer:</strong> Tokenizes source code, handling complex symbols like <code>::</code> and <code>-></code>.</li>
            <li><strong>Parser:</strong> Builds an Abstract Syntax Tree (AST) representing the program structure.</li>
            <li><strong>Semantic Analyzer:</strong> Validates variable declarations, scopes, and types.</li>
            <li><strong>Code Generator:</strong> Translates the validated AST into standard C++17 code.</li>
        </ol>
    </section>
    <section id="syntax-example">
        <h2>Syntax Example</h2>
        <p>Below is a preview of how a Sage program looks:</p>
        <pre><code>
// Main.sg
use Console;

function Main() -> i32
{
    // String interpolation and console output
    Console::Print("Hello World! Welcome to the Sage programming language!");

    i32 number01 = 5;
    i32 number02 = 5;
    i32 result = number01 + number02;

    // The compiler handles the interpolation logic internally
    Console::Print("The result of {number01} and {number02} is: {result}");

    return 0;
}
        </code></pre>
    </section>
    <section id="current-status">
        <h2>Project Status</h2>
        <p>
            The project is currently in its <strong>Alpha</strong> stage. The front-end (Lexer/Parser) and the semantic layer 
            are fully functional for procedural code. The back-end successfully generates C++ boilerplate and logic.
        </p>
    </section>
    <hr>
    <footer>
        <h3>License</h3>
        <p>This project is licensed under the <strong>MIT License</strong>.</p>
        <p>&copy; 2026 Kellvyn Sampaio - Sage Language Project</p>
    </footer>
</body>