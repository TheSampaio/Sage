using Sage.Ast;
using Sage.Core;
using Sage.Interfaces;
using Sage.Utilities;
using System;
using System.IO;

namespace Sage
{
    /// <summary>
    /// The entry point for the Sage Compiler. 
    /// Coordinates the Lexer, Parser, Semantic Analyzer, and Code Generator.
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

                CompilerLogger.LogInfo($"Compiling: {Path.GetFileName(config.InputPath)}");

                // 2. Read Source Content
                string sourceCode = File.ReadAllText(config.InputPath);

                // 3. Frontend: Lexical Analysis
                CompilerLogger.LogStep("1. Tokenizing...");
                ILexer lexer = new Lexer(sourceCode);
                var tokens = lexer.Tokenize();

                if (config.IsDebugMode)
                {
                    // Using the new TokenPrinter for readable debug files
                    SaveDebugOutput(config.OutputBaseName + ".tok", TokenPrinter.Print(tokens));
                }

                // 4. Middle-end: Parsing & AST Construction
                CompilerLogger.LogStep("2. Parsing...");
                IParser parser = new Parser(tokens);
                ProgramNode ast = parser.Parse();

                // 5. Middle-end: Semantic Validation
                CompilerLogger.LogStep("3. Semantic Analysis...");
                new SemanticAnalyzer().Analyze(ast);
                CompilerLogger.LogDebug("Semantic checks passed.");

                if (config.IsDebugMode)
                {
                    SaveDebugOutput(config.OutputBaseName + ".ast", new AstPrinter().Print(ast));
                }

                // 6. Backend: Code Generation
                CompilerLogger.LogStep("4. Generating C Code...");
                string cCode = new CodeGenerator().Generate(ast);

                // 7. Final Output Delivery
                string cPath = config.OutputBaseName + ".c";
                File.WriteAllText(cPath, cCode);

                CompilerLogger.LogSuccess($"Compilation complete. Output: {cPath}");
            }
            catch (Exception ex)
            {
                CompilerLogger.LogFatal(ex);
            }
        }

        #region Helpers

        private static CompilerConfig ResolveConfiguration(string[] args)
        {
            string inputPath;
            bool isDebug = false;

#if DEBUG
            isDebug = true;
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            inputPath = Path.Combine(projectRoot, "Assets", "main.sg");
#else
            if (args.Length == 0) throw new ArgumentException("Usage: Sage <filename.sg>");
            inputPath = Path.GetFullPath(args[0]);
#endif
            return new CompilerConfig(inputPath, isDebug);
        }

        private static void SaveDebugOutput(string path, string content)
        {
            File.WriteAllText(path, content);
            CompilerLogger.LogDebug($"Debug info saved to: {path}");
        }

        #endregion
    }
}