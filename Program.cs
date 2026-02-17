using Sage.Ast;
using Sage.Core;
using Sage.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sage
{
    internal static class Program
    {
        // --- Código para habilitar ANSI no Windows (Silencioso) ---
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private static void Main(string[] args)
        {
            // Ativa o processamento de caracteres ANSI (\x1B) no terminal
            var handle = GetStdHandle(-11);
            if (GetConsoleMode(handle, out uint mode))
                SetConsoleMode(handle, mode | 0x0004);

            try
            {
                var config = ResolveConfiguration(args);

                // --- 1. Intelligent Directory Resolution ---
                string exeDir = AppContext.BaseDirectory;
                string sourceDir = Path.GetDirectoryName(config.InputPath)!;
                string projectRoot = sourceDir;

                string stdDir = Path.GetFullPath(Path.Combine(exeDir, "..", "std"));

                if (!Directory.Exists(stdDir))
                {
                    if (Path.GetFileName(sourceDir).Equals("src", StringComparison.OrdinalIgnoreCase))
                    {
                        projectRoot = Directory.GetParent(sourceDir)?.FullName ?? sourceDir;
                    }
                    stdDir = Path.Combine(projectRoot, "std");
                }

                string objDir = Path.Combine(projectRoot, "obj");
                string binDir = Path.Combine(projectRoot, "bin");

                Directory.CreateDirectory(objDir);
                Directory.CreateDirectory(binDir);

                if (config.IsDebugMode)
                {
                    CompilerLogger.LogInfo($"[DEBUG] Compiler Location: {exeDir}");
                    CompilerLogger.LogInfo($"[DEBUG] STL Path: {stdDir}");
                    CompilerLogger.LogInfo($"[DEBUG] Compiling project: {Path.GetFileName(projectRoot)}");
                }
                // REMOVIDO: O else que imprimia "Compiling..." no modo Release

                // --- 2. Build Pipeline (Two-Pass) ---
                var compilationQueue = new Queue<string>();
                var parsedModules = new Dictionary<string, ProgramNode>();
                var generatedCFiles = new List<string>();

                var symbolTable = new SymbolTable();
                var analyzer = new SemanticAnalyzer(symbolTable);

                compilationQueue.Enqueue(config.InputPath);

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

                    analyzer.RegisterModuleSymbols(ast);
                    parsedModules.Add(moduleName, ast);

                    foreach (var use in ast.Statements.OfType<UseNode>())
                    {
                        string depPath = Path.Combine(sourceDir, $"{use.Module}.sg");
                        if (!File.Exists(depPath)) depPath = Path.Combine(stdDir, $"{use.Module}.sg");

                        if (File.Exists(depPath)) compilationQueue.Enqueue(depPath);
                    }
                }

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

                    File.WriteAllText(Path.Combine(objDir, moduleName + ".h"), new HeaderGenerator().Generate(ast));
                    string cCode = new CodeGenerator().Generate(ast);
                    File.WriteAllText(Path.Combine(objDir, moduleName + ".c"), cCode);

                    generatedCFiles.Add(Path.Combine(objDir, $"{moduleName}.c"));

                    if (config.IsDebugMode)
                        CompilerLogger.LogSuccess($"Generated: {moduleName}.c/h");
                }

                // --- 4. Linking ---
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

                    if (config.IsDebugMode) RunApplicationDebug(exePath);
                    else RunApplicationRelease(exePath);
                }
            }
            catch (Exception ex)
            {
                // Erros e Warnings continuam sendo exibidos em ambos os modos
                CompilerLogger.LogFatal(ex);
            }
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
            if (process.ExitCode != 0) throw new Exception($"GCC Linking Failed:\n{errors}");

            // AJUSTADO: Agora só imprime sucesso se estiver em modo Debug
            if (config.IsDebugMode) Console.WriteLine($"Build succeeded -> {exeOutputPath}");
        }

        private static void RunApplicationDebug(string exePath)
        {
            CompilerLogger.LogStep("Running Application...");
            Console.WriteLine("--------------------------------------------------");
            var process = Process.Start(new ProcessStartInfo { FileName = exePath, UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(exePath) })!;
            process.WaitForExit();
            Console.WriteLine("--------------------------------------------------");
            CompilerLogger.LogInfo($"App exited with code: {process.ExitCode}");
        }

        private static void RunApplicationRelease(string exePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            })!.WaitForExit();
        }

        private static CompilerConfig ResolveConfiguration(string[] args)
        {
#if DEBUG
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            string inputPath = Path.Combine(projectRoot, "Sandbox", "src", "main.sg");
            return new CompilerConfig(inputPath, true, true);
#else
            if (args.Length == 0) throw new ArgumentException("Usage: Sage <main.sg>");
            bool verbose = args.Contains("--verbose") || args.Contains("-v");
            string file = args.First(a => !a.StartsWith("-"));
            return new CompilerConfig(Path.GetFullPath(file), verbose, true);
#endif
        }

        private static void SaveDebugOutput(string path, string content) => File.WriteAllText(path, content);
    }
}