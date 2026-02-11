using Sage.Core;
using Sage.Core.AST;
using Sage.Enums;

namespace Sage
{
    class Program
    {
        static void Main()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            string filename = "Main.sg";

            string inputPath = Path.Combine(projectRoot, "Assets", filename);
            string outputPath = $"{inputPath}.tok";

            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"[ERROR] File not found: {inputPath}");
                return;
            }

            try
            {
                Console.WriteLine("1. Lexing...");
                string sourceCode = File.ReadAllText(inputPath);
                Lexer lexer = new Lexer(sourceCode);
                List<Token> tokens = lexer.Tokenize();

                // Save tokens for debug
#if DEBUG
                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    foreach (Token token in tokens)
                        if (token.Type != TokenType.EndOfFile) writer.WriteLine(token.ToString());
                }
#endif
                Console.WriteLine("2. Parsing...");
                Parser parser = new Parser(tokens);
                ProgramNode ast = parser.Parse();

                Console.WriteLine("Build successful!");
                Console.WriteLine($"AST created with {ast.Statements.Count} top-level statements.");

                Console.WriteLine("\n--- AST Structure ---");
                Printer.Print(ast);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] {ex.Message}");
            }
        }
    }
}