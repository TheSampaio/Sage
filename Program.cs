using Sage.Ast;
using Sage.Core;
using Sage.Interfaces;
using Sage.Utilities;
using System;
using System.Diagnostics; // Required for Process
using System.IO;

namespace Sage
{
    /// <summary>
    /// The entry point for the Sage Compiler. 
    /// Manages the build pipeline, project structure, and debug execution.
    /// </summary>
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                // 1. Setup Configuration
                var config = ResolveConfiguration(args);

                if (!File.Exists(config.InputPath))
                {
                    CompilerLogger.LogError($"File not found: {config.InputPath}");
                    return;
                }

                // --- Intelligent Directory Resolution (Sandbox / Project) ---
                string sourceDir = Path.GetDirectoryName(config.InputPath) ?? Directory.GetCurrentDirectory();
                string projectRoot = sourceDir;

                // If inside "src", step back to root
                if (Path.GetFileName(sourceDir).Equals("src", StringComparison.OrdinalIgnoreCase))
                {
                    projectRoot = Directory.GetParent(sourceDir)?.FullName ?? sourceDir;
                }

                string objDir = Path.Combine(projectRoot, "obj");
                string binDir = Path.Combine(projectRoot, "bin");

                Directory.CreateDirectory(objDir);
                Directory.CreateDirectory(binDir);

                string fileName = Path.GetFileName(config.InputPath);
                string baseName = Path.GetFileNameWithoutExtension(fileName);

                CompilerLogger.LogInfo($"Compiling project: {Path.GetFileName(projectRoot)}");

                // 2. Read Source
                string sourceCode = File.ReadAllText(config.InputPath);

                // 3. Frontend: Lexer
                CompilerLogger.LogStep("1. Tokenizing...");
                ILexer lexer = new Lexer(sourceCode);
                var tokens = lexer.Tokenize();

                if (config.IsDebugMode)
                {
                    SaveDebugOutput(Path.Combine(objDir, fileName + ".tok"), TokenPrinter.Print(tokens));
                }

                // 4. Middle-end: Parser
                CompilerLogger.LogStep("2. Parsing...");
                IParser parser = new Parser(tokens);
                ProgramNode ast = parser.Parse();

                // 5. Middle-end: Semantics
                CompilerLogger.LogStep("3. Semantic Analysis...");
                new SemanticAnalyzer().Analyze(ast);
                CompilerLogger.LogDebug("Semantic checks passed.");

                if (config.IsDebugMode)
                {
                    SaveDebugOutput(Path.Combine(objDir, fileName + ".ast"), new AstPrinter().Print(ast));
                }

                // 6. Backend: Code Generation
                CompilerLogger.LogStep("4. Generating C Code...");
                string cCode = new CodeGenerator().Generate(ast);
                string cFilePath = Path.Combine(objDir, fileName + ".c");
                File.WriteAllText(cFilePath, cCode);

                CompilerLogger.LogSuccess($"Intermediate C code generated: {cFilePath}");

                // 7. Native Build & Execution
                if (config.BuildNative)
                {
                    CompilerLogger.LogStep("5. Building Native Executable...");
                    string exePath = Path.Combine(binDir, baseName + ".exe");

                    // Compile C -> Exe
                    RunNativeBuild(cFilePath, exePath);

                    if (config.IsDebugMode)
                    {
                        RunSandboxApplication(exePath);
                    }
                }
            }
            catch (Exception ex)
            {
                CompilerLogger.LogFatal(ex);
            }
        }

        /// <summary>
        /// Executes the compiled binary immediately for feedback.
        /// Only runs in Debug mode.
        /// </summary>
        private static void RunSandboxApplication(string exePath)
        {
            CompilerLogger.LogStep("6. Running Sandbox Application...");
            Console.WriteLine("--------------------------------------------------");

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = false, // Keeps output in the current console window
                        WorkingDirectory = Path.GetDirectoryName(exePath) // Run inside /bin
                    }
                };

                process.Start();
                process.WaitForExit();

                Console.WriteLine("--------------------------------------------------");
                CompilerLogger.LogInfo($"App exited with code: {process.ExitCode}");
            }
            catch (Exception ex)
            {
                CompilerLogger.LogError($"Failed to run application: {ex.Message}");
            }
        }

        private static void RunNativeBuild(string cSourcePath, string exeOutputPath)
        {
            // Assuming NativeCompiler throws on error, which gets caught by Main
            new NativeCompiler().CompileToExecutable(cSourcePath, exeOutputPath);
        }

        #region Helpers

        private static CompilerConfig ResolveConfiguration(string[] args)
        {
            string inputPath;
            bool isDebug = false;
            bool buildNative = true;

#if DEBUG
            isDebug = true;
            // Points to Sage/Sandbox/src/main.sg relative to bin/Debug/net8.0
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            inputPath = Path.Combine(projectRoot, "Sandbox", "src", "main.sg");

            // Create Sandbox file if missing (DX improvement)
            if (!File.Exists(inputPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(inputPath)!);
                File.WriteAllText(inputPath, "func main() -> i32 { return 0; }");
            }
#else
            if (args.Length == 0) throw new ArgumentException("Usage: Sage <filename.sg>");
            inputPath = Path.GetFullPath(args[0]);
#endif
            return new CompilerConfig(inputPath, isDebug, buildNative);
        }

        private static void SaveDebugOutput(string path, string content)
        {
            File.WriteAllText(path, content);
            CompilerLogger.LogDebug($"Debug info saved to: {path}");
        }

        #endregion
    }
}