using Sage.Ast;
using Sage.Core;
using Sage.Interfaces;
using Sage.Utilities; // Namespace onde colocamos o AstPrinter
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sage
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // 1. Configuração do Contexto (Input/Output)
                CompilerConfig config = ResolveConfiguration(args);

                if (!File.Exists(config.InputPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] File not found: {config.InputPath}");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine($"[INFO] Compiling: {Path.GetFileName(config.InputPath)}");

                // 2. Leitura do Código Fonte
                string sourceCode = File.ReadAllText(config.InputPath);

                // 3. Etapa: Lexical Analysis
                Console.WriteLine("1. Tokenizing...");
                ILexer lexer = new Lexer(sourceCode);
                var tokens = lexer.Tokenize();

                // 3.1 Debug: Dump Tokens (JSON)
                if (config.IsDebugMode)
                {
                    string tokenPath = config.OutputBaseName + ".tok";
                    string json = JsonSerializer.Serialize(tokens, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Converters = { new JsonStringEnumConverter() } // Para mostrar TokenType como texto
                    });

                    File.WriteAllText(tokenPath, json);
                    Console.WriteLine($"   [DEBUG] Tokens saved to: {tokenPath}");
                }

                // 4. Etapa: Parsing
                Console.WriteLine("2. Parsing...");
                IParser parser = new Parser(tokens);
                ProgramNode ast = parser.Parse();

                // 4.1 Debug: Dump AST (Tree Visualization)
                if (config.IsDebugMode)
                {
                    string astPath = config.OutputBaseName + ".ast";
                    var printer = new AstPrinter();
                    string astDump = printer.Print(ast);

                    File.WriteAllText(astPath, astDump);
                    Console.WriteLine($"   [DEBUG] AST saved to: {astPath}");
                }

                // 5. Etapa: Code Generation
                Console.WriteLine("3. Generating C Code...");
                ICodeGenerator generator = new CodeGenerator(); // Use a classe CCodeGenerator criada anteriormente
                string cCode = generator.Generate(ast);

                // 6. Output Final
                string cPath = config.OutputBaseName + ".c";
                File.WriteAllText(cPath, cCode);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[SUCCESS] Compilation complete. Output: {cPath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[FATAL ERROR] {ex.Message}");
                if (ex.StackTrace != null) Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
        }

        // --- Helper de Configuração ---

        private static CompilerConfig ResolveConfiguration(string[] args)
        {
            string inputPath;
            bool isDebug = false;

#if DEBUG
            // Lógica Exclusiva de Debug (Visual Studio / Development)
            isDebug = true;

            // Pega o diretório do projeto subindo 3 níveis a partir do bin/Debug/net8.0
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            inputPath = Path.Combine(projectRoot, "Assets", "main.sg");
#else
            // Lógica de Release (Produção / CLI)
            if (args.Length == 0)
            {
                throw new Exception("No input file provided. Usage: Sage.exe <filename.sg>");
            }
            inputPath = Path.GetFullPath(args[0]);
#endif

            return new CompilerConfig(inputPath, isDebug);
        }
    }

    // DTO para carregar as configurações
    public class CompilerConfig
    {
        public string InputPath { get; }
        public string OutputBaseName { get; }
        public bool IsDebugMode { get; }

        public CompilerConfig(string inputPath, bool isDebugMode)
        {
            InputPath = inputPath;
            IsDebugMode = isDebugMode;

            // Se input é "Assets/main.sg", output base é "Assets/main.sg" (para gerar main.sg.tok, main.sg.c)
            // Ou podemos remover a extensão original. Vamos manter para clareza (main.sg.c).
            OutputBaseName = inputPath;
        }
    }
}