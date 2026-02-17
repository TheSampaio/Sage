using Sage.Ast;
using Sage.Core;
using Sage.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sage
{
    /// <summary>
    /// The main entry point for the Sage Compiler. 
    /// Orchestrates the lexical, syntax, and semantic analysis phases, 
    /// and manages the C transpilation and native linking toolchain.
    /// </summary>
    internal static class Program
    {
#pragma warning disable
        // --- Windows ANSI Support via P/Invoke ---
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
#pragma warning enable 

        /// <summary>
        /// Main execution loop. Resolves configuration, builds the module dependency graph, 
        /// and executes the two-pass compilation pipeline.
        /// </summary>
        /// <param name="args">Command-line arguments provided by the user.</param>
        private static void Main(string[] args)
        {
            EnableAnsiConsole();

            try
            {
                if (args.Contains("--v") || args.Contains("--version"))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Sage Compiler v0.3.0 (Alpha)");
                    Console.ResetColor();
                    return;
                }

                var config = ResolveConfiguration(args);

                // --- 1. Intelligent Directory Resolution ---
                string exeDir = AppContext.BaseDirectory;
                string sourceDir = Path.GetDirectoryName(config.InputPath)!;
                string projectRoot = sourceDir;

                // Locate Standard Library (std) relative to the compiler or project root
                string stdDir = Path.GetFullPath(Path.Combine(exeDir, "..", "std"));

                if (!Directory.Exists(stdDir))
                {
                    // Fallback logic for development environments where 'src' folder is used
                    if (Path.GetFileName(sourceDir).Equals("src", StringComparison.OrdinalIgnoreCase))
                    {
                        projectRoot = Directory.GetParent(sourceDir)?.FullName ?? sourceDir;
                    }
                    stdDir = Path.Combine(projectRoot, "std");
                }

                string objDir = Path.Combine(projectRoot, "obj");
                string binDir = Path.Combine(projectRoot, "bin");

                // Ensure build artifacts directories exist
                Directory.CreateDirectory(objDir);
                Directory.CreateDirectory(binDir);

                if (config.IsDebugMode)
                {
                    CompilerLogger.LogInfo($"[DEBUG] Compiler Location: {exeDir}");
                    CompilerLogger.LogInfo($"[DEBUG] STL Path: {stdDir}");
                    CompilerLogger.LogInfo($"[DEBUG] Compiling project: {Path.GetFileName(projectRoot)}");
                }

                // --- 2. Build Pipeline (Two-Pass) ---
                var compilationQueue = new Queue<string>();
                var parsedModules = new Dictionary<string, ProgramNode>();
                var generatedCFiles = new List<string>();

                var symbolTable = new SymbolTable();
                var analyzer = new SemanticAnalyzer(symbolTable);

                // Start with the main entry file
                compilationQueue.Enqueue(config.InputPath);

                // Pass 1: Parsing and Symbol Registration
                while (compilationQueue.Count > 0)
                {
                    string currentPath = compilationQueue.Dequeue();
                    string moduleName = Path.GetFileNameWithoutExtension(currentPath);

                    if (parsedModules.ContainsKey(moduleName)) continue;

                    if (config.IsDebugMode)
                        Console.WriteLine($"\n--- [MODULE] Processing: {moduleName} ---");

                    string code = File.ReadAllText(currentPath);
                    var tokens = new Lexer(code).Tokenize();

                    if (config.IsDebugMode)
                    {
                        CompilerLogger.LogStep("1. Tokenizing...");
                        SaveDebugOutput(Path.Combine(objDir, moduleName + ".tok"), TokenPrinter.Print(tokens));
                        CompilerLogger.LogStep("2. Parsing...");
                    }

                    var ast = new Parser(tokens, Path.GetFileName(currentPath)).Parse();

                    // Register function signatures globally before deep analysis
                    analyzer.RegisterModuleSymbols(ast);
                    parsedModules.Add(moduleName, ast);

                    // Queue dependencies found via 'use' keywords
                    foreach (var use in ast.Statements.OfType<UseNode>())
                    {
                        string depPath = Path.Combine(sourceDir, $"{use.Module}.sg");
                        if (!File.Exists(depPath)) depPath = Path.Combine(stdDir, $"{use.Module}.sg");

                        if (File.Exists(depPath)) compilationQueue.Enqueue(depPath);
                    }
                }

                // Pass 2: Semantic Analysis and Transpilation
                foreach (var entry in parsedModules)
                {
                    string moduleName = entry.Key;
                    ProgramNode ast = entry.Value;

                    if (config.IsDebugMode)
                    {
                        Console.WriteLine($"\n--- [MODULE] Finishing: {moduleName} ---");
                        CompilerLogger.LogStep("3. Semantic Analysis...");
                    }

                    analyzer.Analyze(ast);

                    if (config.IsDebugMode)
                    {
                        SaveDebugOutput(Path.Combine(objDir, moduleName + ".ast"), new AstPrinter().Print(ast));
                        CompilerLogger.LogStep("4. Generating C Code...");
                    }

                    // Write generated C source and headers to the object directory
                    File.WriteAllText(Path.Combine(objDir, moduleName + ".h"), new HeaderGenerator().Generate(ast));
                    string cCode = new CodeGenerator().Generate(ast);
                    File.WriteAllText(Path.Combine(objDir, moduleName + ".c"), cCode);

                    generatedCFiles.Add(Path.Combine(objDir, $"{moduleName}.c"));

                    if (config.IsDebugMode)
                        CompilerLogger.LogSuccess($"Generated: {moduleName}.c/h");
                }

                // --- 4. Native Toolchain Integration ---
                if (config.BuildNative)
                {
                    if (config.IsDebugMode)
                    {
                        Console.WriteLine();
                        CompilerLogger.LogStep("Linking binaries...");
                    }

                    string exeName = Path.GetFileNameWithoutExtension(config.InputPath) + ".exe";
                    string exePath = Path.Combine(binDir, exeName);

                    RunNativeLinker(generatedCFiles, exePath, objDir, config);

                    // Execute the resulting application
                    if (config.IsDebugMode) RunApplicationDebug(exePath);
                    else RunApplicationRelease(exePath);
                }
            }
            catch (Exception ex)
            {
                // Global error handler for the compiler
                CompilerLogger.LogFatal(ex);
            }
        }

        /// <summary>
        /// Enables support for ANSI color codes in the Windows Console.
        /// </summary>
        private static void EnableAnsiConsole()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
                if (GetConsoleMode(handle, out uint mode))
                    SetConsoleMode(handle, mode | 0x0004); // ENABLE_VIRTUAL_TERMINAL_PROCESSING
            }
        }

        /// <summary>
        /// Invokes the GCC compiler to link the generated C files into a native executable.
        /// </summary>
        private static void RunNativeLinker(List<string> cFiles, string exeOutputPath, string includePath, CompilerConfig config)
        {
            if (config.IsDebugMode) CompilerLogger.LogInfo("Invoking native toolchain: gcc");

            string sources = string.Join(" ", cFiles.Select(f => $"\"{f}\""));
            string arguments = $"{sources} -o \"{exeOutputPath}\" -std=c11 -I\"{includePath}\"";

            ProcessExecutor.Run("gcc", arguments);

            if (config.IsDebugMode) Console.WriteLine($"Build succeeded -> {exeOutputPath}");
        }

        /// <summary>Executes the generated binary and displays exit codes and debug separators.</summary>
        private static void RunApplicationDebug(string exePath)
        {
            CompilerLogger.LogStep("Running Application...");
            Console.WriteLine("--------------------------------------------------");

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            });

            process?.WaitForExit();
            Console.WriteLine("--------------------------------------------------");
            CompilerLogger.LogInfo($"App exited with code: {process?.ExitCode ?? -1}");
        }

        /// <summary>Executes the generated binary in a clean environment for release builds.</summary>
        private static void RunApplicationRelease(string exePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            })?.WaitForExit();
        }

        /// <summary>Resolves build flags and input paths from command-line arguments.</summary>
        private static CompilerConfig ResolveConfiguration(string[] args)
        {
#if DEBUG
            // Automated configuration for development convenience
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            string inputPath = Path.Combine(projectRoot, "Sandbox", "src", "main.sg");
            return new CompilerConfig(inputPath, true, true);
#else
        if (args.Length == 0) throw new ArgumentException("Usage: Sage <main.sg> [--verbose]");
        bool verbose = args.Contains("--verbose") || args.Contains("-v");
        string file = args.First(a => !a.StartsWith("-"));
        return new CompilerConfig(Path.GetFullPath(file), verbose, true);
#endif
        }

        /// <summary>Helper method to save debugging files (.tok, .ast, etc.) to the disk.</summary>
        private static void SaveDebugOutput(string path, string content) => File.WriteAllText(path, content);
    }
}