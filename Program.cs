using Sage.Ast;
using Sage.Core;
using Sage.Utilities;
using System.Diagnostics;

namespace Sage
{
    /// <summary>
    /// The entry point for the Sage Compiler CLI.
    /// Handles command-line arguments and orchestrates the compilation pipeline.
    /// </summary>
    internal static class Program
    {
        private static void Main(string[] args)
        {
            ConsoleHelper.EnableAnsiSupport();

            try
            {
                if (HandleCliCommands(args)) return;

                // If no specific CLI command handled it, proceed to compilation
                RunCompilationPipeline(args);
            }
            catch (Exception ex)
            {
                CompilerLogger.LogFatal(ex);
            }
        }

        /// <summary>
        /// Handles meta-commands like --version or 'new'. 
        /// Returns true if execution should stop here.
        /// </summary>
        private static bool HandleCliCommands(string[] args)
        {
            // 1. Version Command
            if (args.Contains("--version") || args.Contains("--v"))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Sage Compiler v0.4.0 (Alpha)");
                Console.WriteLine("  \"Knowledge is Power\"");
                Console.ResetColor();
                return true;
            }

            // 2. New Project Command
            if (args.Length >= 1 && args[0].Equals("new", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length < 2)
                {
                    CompilerLogger.LogError("Missing project name.");
                    Console.WriteLine("\nUsage:\n  sage new <project_name>");
                    return true;
                }
                ProjectInitializer.Initialize(args[1]);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Orchestrates the main compilation process: Setup -> Parse -> Analyze -> Link.
        /// </summary>
        private static void RunCompilationPipeline(string[] args)
        {
            var config = ResolveConfiguration(args);
            var env = SetupEnvironment(config);

            if (config.IsDebugMode)
            {
                CompilerLogger.LogInfo($"Root: {env.ProjectRoot}");
                CompilerLogger.LogInfo($"Entry: {config.InputPath}");
                CompilerLogger.LogDebug($"Compiler Location: {env.ExeDir}");
                CompilerLogger.LogDebug($"STL Path: {env.StdDir}");
            }

            // --- Compilation State ---
            var compilationQueue = new Queue<string>();
            var parsedModules = new Dictionary<string, ProgramNode>();
            var generatedCFiles = new List<string>();

            var symbolTable = new SymbolTable();
            var analyzer = new SemanticAnalyzer(symbolTable);

            compilationQueue.Enqueue(config.InputPath);

            // --- Phase 1: Parsing & Discovery ---
            PerformParsingPhase(compilationQueue, parsedModules, config, env, analyzer);

            // --- Phase 2: Analysis & Code Generation ---
            PerformGenerationPhase(parsedModules, generatedCFiles, config, env, analyzer);

            // --- Phase 3: Native Linking ---
            if (config.BuildNative)
            {
                PerformLinkingPhase(generatedCFiles, config, env);
            }
        }

        private static void PerformParsingPhase(
            Queue<string> queue,
            Dictionary<string, ProgramNode> parsedModules,
            CompilerConfig config,
            CompilationEnvironment env,
            SemanticAnalyzer analyzer)
        {
            while (queue.Count > 0)
            {
                string currentPath = queue.Dequeue();
                string moduleName = Path.GetFileNameWithoutExtension(currentPath);

                if (parsedModules.ContainsKey(moduleName)) continue;

                if (config.IsDebugMode)
                    Console.WriteLine($"\n--- [MODULE] Processing: {moduleName} ---");

                string code = File.ReadAllText(currentPath);
                var tokens = new Lexer(code).Tokenize();

                if (config.IsDebugMode)
                {
                    CompilerLogger.LogStep("1. Tokenizing...");
                    File.WriteAllText(Path.Combine(env.ObjDir, moduleName + ".tok"), TokenPrinter.Print(tokens));
                    CompilerLogger.LogStep("2. Parsing...");
                }

                var ast = new Parser(tokens, Path.GetFileName(currentPath)).Parse();
                analyzer.RegisterModuleSymbols(ast);
                parsedModules.Add(moduleName, ast);

                // Resolve Dependencies
                foreach (var use in ast.Statements.OfType<UseNode>())
                {
                    string localDep = Path.Combine(env.SourceDir, $"{use.Module}.sg");
                    string stdDep = Path.Combine(env.StdDir, $"{use.Module}.sg");

                    if (File.Exists(localDep)) queue.Enqueue(localDep);
                    else if (File.Exists(stdDep)) queue.Enqueue(stdDep);
                }
            }
        }

        private static void PerformGenerationPhase(
            Dictionary<string, ProgramNode> modules,
            List<string> generatedCFiles,
            CompilerConfig config,
            CompilationEnvironment env,
            SemanticAnalyzer analyzer)
        {
            foreach (var entry in modules)
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
                    File.WriteAllText(Path.Combine(env.ObjDir, moduleName + ".ast"), new AstPrinter().Print(ast));
                    CompilerLogger.LogStep("4. Generating C Code...");
                }

                // Generate Headers and C Source
                File.WriteAllText(Path.Combine(env.ObjDir, moduleName + ".h"), new HeaderGenerator().Generate(ast));

                string cCode = new CodeGenerator().Generate(ast);
                string cFilePath = Path.Combine(env.ObjDir, moduleName + ".c");
                File.WriteAllText(cFilePath, cCode);

                generatedCFiles.Add(cFilePath);

                if (config.IsDebugMode)
                    CompilerLogger.LogSuccess($"Generated: {moduleName}.c/h");
            }
        }

        private static void PerformLinkingPhase(List<string> cFiles, CompilerConfig config, CompilationEnvironment env)
        {
            if (config.IsDebugMode)
            {
                Console.WriteLine();
                CompilerLogger.LogStep("Linking binaries...");
            }

            string exeName = Path.GetFileNameWithoutExtension(config.InputPath);
            if (exeName.Equals("main", StringComparison.OrdinalIgnoreCase))
            {
                exeName = new DirectoryInfo(env.ProjectRoot).Name;
            }
            exeName += ".exe";

            string exePath = Path.Combine(env.BinDir, exeName);

            RunNativeLinker(cFiles, exePath, env.ObjDir, config);

            if (config.RunOnSuccess)
            {
                if (config.IsDebugMode) RunApplicationDebug(exePath);
                else RunApplicationRelease(exePath);
            }
            else
            {
                Console.WriteLine();
                CompilerLogger.LogSuccess($"Build complete: {exePath}");
            }
        }

        private static CompilationEnvironment SetupEnvironment(CompilerConfig config)
        {
            string exeDir = AppContext.BaseDirectory;
            string sourceDir = Path.GetDirectoryName(config.InputPath)!;
            string projectRoot = sourceDir;

            if (Path.GetFileName(sourceDir).Equals("src", StringComparison.OrdinalIgnoreCase))
            {
                projectRoot = Directory.GetParent(sourceDir)?.FullName ?? sourceDir;
            }

            string stdDir = Path.GetFullPath(Path.Combine(exeDir, "..", "std"));
            if (!Directory.Exists(stdDir)) stdDir = Path.Combine(projectRoot, "std");

            string objDir = Path.Combine(projectRoot, "obj");
            string binDir = Path.Combine(projectRoot, "bin");

            Directory.CreateDirectory(objDir);
            Directory.CreateDirectory(binDir);

            return new CompilationEnvironment(projectRoot, sourceDir, objDir, binDir, stdDir, exeDir);
        }

        private static void RunNativeLinker(List<string> cFiles, string exeOutputPath, string includePath, CompilerConfig config)
        {
            if (config.IsDebugMode) CompilerLogger.LogInfo("Invoking native toolchain: gcc");

            string sources = string.Join(" ", cFiles.Select(f => $"\"{f}\""));
            string arguments = $"{sources} -o \"{exeOutputPath}\" -std=c11 -I\"{includePath}\"";

            ProcessExecutor.Run("gcc", arguments);

            if (config.IsDebugMode) CompilerLogger.LogSuccess($"Build succeeded -> {exeOutputPath}");
        }

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

        private static void RunApplicationRelease(string exePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            })?.WaitForExit();
        }

        private static CompilerConfig ResolveConfiguration(string[] args)
        {
#if DEBUG
            // Dev Environment Sandbox Fallback
            if (args.Length == 0)
            {
                string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
                string sandboxPath = Path.Combine(projectRoot, "Sandbox", "src", "main.sg");

                if (!Directory.Exists(Path.GetDirectoryName(sandboxPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(sandboxPath)!);
                    File.WriteAllText(sandboxPath, "use console;\nfunc main() : none { console::print_line(\"Debug Mode Working!\"); }");
                }
                return new CompilerConfig(sandboxPath, true, true, true);
            }
#endif
            if (args.Length == 0) throw new ArgumentException("Usage: sage <command> OR sage <file>");

            bool verbose = args.Contains("--verbose") || args.Contains("-v");
            string command = args.FirstOrDefault(a => !a.StartsWith('-')) ?? "";
            string autoPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "main.sg");

            if (command.Equals("run", StringComparison.OrdinalIgnoreCase))
            {
                ValidateEntry(autoPath);
                return new CompilerConfig(autoPath, verbose, true, true);
            }

            if (command.Equals("build", StringComparison.OrdinalIgnoreCase))
            {
                ValidateEntry(autoPath);
                return new CompilerConfig(autoPath, verbose, true, false);
            }

            string explicitFile = Path.GetFullPath(command);
            if (!File.Exists(explicitFile))
                throw new FileNotFoundException($"Source file not found: {explicitFile}");

            return new CompilerConfig(explicitFile, verbose, true, true);
        }

        private static void ValidateEntry(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Entry point not found at '{path}'. Are you inside a Sage project root?");
        }

        // Helper record to pass path context around
        private record CompilationEnvironment(string ProjectRoot, string SourceDir, string ObjDir, string BinDir, string StdDir, string ExeDir);
    }
}