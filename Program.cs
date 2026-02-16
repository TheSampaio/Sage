using Sage.Ast;
using Sage.Core;
using Sage.Interfaces;
using Sage.Utilities;
using System.Diagnostics;

namespace Sage
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var config = ResolveConfiguration(args);

                // --- 1. Intelligent Directory Resolution ---
                string sourceDir = Path.GetDirectoryName(config.InputPath)!;
                string projectRoot = sourceDir;

                if (Path.GetFileName(sourceDir).Equals("src", StringComparison.OrdinalIgnoreCase))
                {
                    projectRoot = Directory.GetParent(sourceDir)?.FullName ?? sourceDir;
                }

                string objDir = Path.Combine(projectRoot, "obj");
                string binDir = Path.Combine(projectRoot, "bin");

                Directory.CreateDirectory(objDir);
                Directory.CreateDirectory(binDir);

                // Professional minimal message
                if (config.IsDebugMode)
                {
                    CompilerLogger.LogInfo($"[DEBUG] Compiling project: {Path.GetFileName(projectRoot)}");
                }
                else
                {
                    Console.WriteLine($"Compiling {Path.GetFileName(projectRoot)}...");
                }

                // --- 2. Build Pipeline ---
                var compilationQueue = new Queue<string>();
                var compiledModules = new HashSet<string>();
                var generatedCFiles = new List<string>();

                compilationQueue.Enqueue(config.InputPath);

                while (compilationQueue.Count > 0)
                {
                    string currentFilePath = compilationQueue.Dequeue();
                    string moduleName = Path.GetFileNameWithoutExtension(currentFilePath);

                    if (compiledModules.Contains(moduleName)) continue;
                    compiledModules.Add(moduleName);

                    // Only show module headers in Debug mode
                    if (config.IsDebugMode)
                    {
                        Console.WriteLine($"\n--- [MODULE] Processing: {moduleName} ---");
                    }

                    // Compile and discover dependencies
                    var newDependencies = CompileModule(currentFilePath, moduleName, sourceDir, objDir, config);
                    generatedCFiles.Add(Path.Combine(objDir, $"{moduleName}.c"));

                    foreach (var dep in newDependencies)
                    {
                        if (!compiledModules.Contains(dep))
                        {
                            string depPath = Path.Combine(sourceDir, $"{dep}.sg");
                            if (File.Exists(depPath))
                            {
                                compilationQueue.Enqueue(depPath);
                            }
                            else
                            {
                                CompilerLogger.LogWarning($"Module '{dep}' not found at {depPath}");
                            }
                        }
                    }
                }

                if (config.IsDebugMode) Console.WriteLine();

                // --- 3. Linking ---
                if (config.BuildNative)
                {
                    if (config.IsDebugMode) CompilerLogger.LogStep("Linking binaries...");

                    string exeName = Path.GetFileNameWithoutExtension(config.InputPath) + ".exe";
                    string exePath = Path.Combine(binDir, exeName);

                    RunNativeLinker(generatedCFiles, exePath, objDir, config);

                    // --- 4. Execution ---
                    // In release mode, we just run it silently like 'dotnet run'
                    if (config.IsDebugMode)
                    {
                        RunApplicationDebug(exePath);
                    }
                    else
                    {
                        // Mimic 'dotnet run': just execute
                        RunApplicationRelease(exePath);
                    }
                }
            }
            catch (Exception ex)
            {
                CompilerLogger.LogFatal(ex);
            }
        }

        private static List<string> CompileModule(string filePath, string moduleName, string outputDir, string objDir, CompilerConfig config)
        {
            var dependencies = new List<string>();
            string sourceCode = File.ReadAllText(filePath);

            // 1. Lexer
            if (config.IsDebugMode) CompilerLogger.LogStep("1. Tokenizing...");
            ILexer lexer = new Lexer(sourceCode);
            var tokens = lexer.Tokenize();

            if (config.IsDebugMode)
                SaveDebugOutput(Path.Combine(objDir, moduleName + ".tok"), TokenPrinter.Print(tokens));

            // 2. Parser
            if (config.IsDebugMode) CompilerLogger.LogStep("2. Parsing...");
            IParser parser = new Parser(tokens);
            ProgramNode ast = parser.Parse();

            foreach (var stmt in ast.Statements.OfType<UseNode>())
            {
                if (stmt.Module != "console") dependencies.Add(stmt.Module);
            }

            // 3. Semantics
            if (config.IsDebugMode) CompilerLogger.LogStep("3. Semantic Analysis...");
            new SemanticAnalyzer().Analyze(ast);

            if (config.IsDebugMode)
                SaveDebugOutput(Path.Combine(objDir, moduleName + ".ast"), new AstPrinter().Print(ast));

            // 4. Code Gen
            if (config.IsDebugMode) CompilerLogger.LogStep("4. Generating C Code...");

            File.WriteAllText(Path.Combine(objDir, moduleName + ".h"), new HeaderGenerator().Generate(ast));
            string cCode = new CodeGenerator().Generate(ast);
            File.WriteAllText(Path.Combine(objDir, moduleName + ".c"), cCode);

            if (config.IsDebugMode)
                CompilerLogger.LogSuccess($"Generated: {moduleName}.c");

            return dependencies;
        }

        private static void RunNativeLinker(List<string> cFiles, string exeOutputPath, string includePath, CompilerConfig config)
        {
            if (config.IsDebugMode) CompilerLogger.LogInfo("Invoking native toolchain: gcc");

            string sources = string.Join(" ", cFiles.Select(f => $"\"{f}\""));

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "gcc",
                    Arguments = $"{sources} -o \"{exeOutputPath}\" -std=c11 -I\"{includePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string errors = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"GCC Linking Failed:\n{errors}");

            // In Release, we show a clean success message like typical build tools
            if (config.IsDebugMode)
                CompilerLogger.LogSuccess($"Native binary built: {exeOutputPath}");
            else
                Console.WriteLine($"Build succeeded -> {exeOutputPath}");
        }

        /// <summary>
        /// Runs the app with verbose separators and exit codes (for Compiler Devs).
        /// </summary>
        private static void RunApplicationDebug(string exePath)
        {
            CompilerLogger.LogStep("6. Running Application...");
            Console.WriteLine("--------------------------------------------------");

            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                })!;

                process.WaitForExit();

                Console.WriteLine("--------------------------------------------------");
                CompilerLogger.LogInfo($"App exited with code: {process.ExitCode}");
            }
            catch (Exception ex)
            {
                CompilerLogger.LogError($"Execution failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs the app cleanly, outputting only what the app outputs (for Users).
        /// </summary>
        private static void RunApplicationRelease(string exePath)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                })!;

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running application: {ex.Message}");
            }
        }

        private static CompilerConfig ResolveConfiguration(string[] args)
        {
#if DEBUG
            // In VS Debug mode, we force IsDebugMode = true to see the traces
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            string inputPath = Path.Combine(projectRoot, "Sandbox", "src", "main.sg");

            if (!File.Exists(inputPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(inputPath)!);
                File.WriteAllText(inputPath, "func main(): none { }");
            }
            return new CompilerConfig(inputPath, true, true);
#else
            // In Release mode (CLI usage), IsDebugMode defaults to false unless a flag is passed
            if (args.Length == 0) throw new ArgumentException("Usage: Sage <main.sg>");
            
            // Simple flag check for verbose mode
            bool verbose = args.Contains("--verbose") || args.Contains("-v");
            string file = args.First(a => !a.StartsWith("-"));

            return new CompilerConfig(Path.GetFullPath(file), verbose, true);
#endif
        }

        private static void SaveDebugOutput(string path, string content)
        {
            File.WriteAllText(path, content);
            CompilerLogger.LogDebug($"Debug info saved to: {path}");
        }
    }
}