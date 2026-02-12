using Sage.Core.AST;
using Sage.Enums;
using System;
using System.Text;

namespace Sage.Core
{
    public class CodeGenerator
    {
        private StringBuilder _code;
        private int _indentationLevel;

        public CodeGenerator()
        {
            _code = new StringBuilder();
            _indentationLevel = 0;
        }

        private void WriteLine(string text = "")
        {
            _code.AppendLine(new string('\t', _indentationLevel) + text);
        }

        public string Generate(ProgramNode program)
        {
            // Standard C++ includes
            WriteLine("#include <iostream>");
            WriteLine("#include <string>");
            WriteLine("#include <cstdint>"); // REQUIRED for fixed width integers
            WriteLine();

            // Inside Generate() in CodeGenerator.cs
            WriteLine("// --- Sage Helper Functions ---");
            WriteLine("template<typename T> std::string sage_to_string(T val) { return std::to_string(val); }");

            // This prevents the compiler from trying to call to_string on something that is already a string
            WriteLine("inline std::string sage_to_string(std::string val) { return val; }");
            WriteLine("inline std::string sage_to_string(const char* val) { return std::string(val); }");
            WriteLine();

            // Sage Type System Definitions (Mapping to C++)
            WriteLine("// --- Sage Type Definitions ---");
            WriteLine("using u8 = std::uint8_t;");
            WriteLine("using u16 = std::uint16_t;");
            WriteLine("using u32 = std::uint32_t;");
            WriteLine("using u64 = std::uint64_t;");
            WriteLine("using i8 = std::int8_t;");
            WriteLine("using i16 = std::int16_t;");
            WriteLine("using i32 = std::int32_t;");
            WriteLine("using i64 = std::int64_t;");
            WriteLine("using f32 = float;");
            WriteLine("using f64 = double;");
            WriteLine("using b8 = bool;");
            WriteLine("using c8 = char;");
            WriteLine("using none = void;");
            WriteLine("// -----------------------------");
            WriteLine();

            // Basic Sage Runtime Emulation in C++
            WriteLine("namespace Console {");
            _indentationLevel++;
            WriteLine("void Print(std::string text) { std::cout << text << std::endl; }");
            WriteLine("void Print(int value) { std::cout << value << std::endl; }");
            WriteLine("void Print(float value) { std::cout << value << std::endl; }"); // Adicionado suporte básico a float
            WriteLine("void Print(double value) { std::cout << value << std::endl; }"); // Adicionado suporte básico a double
            _indentationLevel--;
            WriteLine("}");
            WriteLine();

            foreach (var stmt in program.Statements)
            {
                Visit(stmt);
            }

            return _code.ToString();
        }

        // Helper method to convert Sage types to C++ compatible strings
        private string MapType(string sageType)
        {
            // Special case for string convenience
            if (sageType == "str") return "std::string";

            // Special case for 'null' type
            if (sageType == "null") return "nullptr";

            // Primitives like i32, u8, f32 already have 'using' aliases in the C++ header
            return sageType;
        }

        private void Visit(AstNode node)
        {
            switch (node)
            {
                case UseNode use:
                    WriteLine($"// Used module: {use.ModuleName}");
                    break;

                case FunctionDeclarationNode func:
                    // FIX: Use MapType instead of manual check to support all new types (u8, f32, etc)
                    string returnType = MapType(func.ReturnType);
                    string cppFuncName = func.Name == "Main" ? "main" : func.Name;

                    WriteLine($"{returnType} {cppFuncName}()");
                    Visit(func.Body);
                    break;

                case BlockNode block:
                    WriteLine("{");
                    _indentationLevel++;
                    foreach (var stmt in block.Statements)
                        Visit(stmt);
                    _indentationLevel--;
                    WriteLine("}");
                    break;

                case VariableDeclarationNode varDecl:
                    // FIX: Removed duplicate variable definition and invalid 'func' usage
                    string cppType = MapType(varDecl.Type);

                    _code.Append(new string('\t', _indentationLevel));
                    _code.Append($"{cppType} {varDecl.Name} = ");
                    VisitExpression(varDecl.Initializer);
                    _code.AppendLine(";");
                    break;

                case ReturnNode ret:
                    _code.Append(new string('\t', _indentationLevel));
                    _code.Append("return ");
                    VisitExpression(ret.Expression);
                    _code.AppendLine(";");
                    break;

                case ExpressionStatementNode exprStmt:
                    _code.Append(new string('\t', _indentationLevel));
                    VisitExpression(exprStmt.Expression);
                    _code.AppendLine(";");
                    break;
            }
        }

        private void VisitExpression(ExpressionNode node)
        {
            switch (node)
            {
                case BinaryExpressionNode bin:
                    VisitExpression(bin.Left);
                    _code.Append($" {GetOperator(bin.Operator)} ");
                    VisitExpression(bin.Right);
                    break;

                case LiteralNode lit:
                    if (lit.TypeName == "string")
                        _code.Append($"\"{lit.Value}\"");
                    else if (lit.TypeName == "float" || lit.TypeName == "f32") // Garante sufixo f para floats se necessário
                        _code.Append($"{lit.Value}f");
                    else
                        _code.Append(lit.Value);
                    break;

                case IdentifierNode id:
                    _code.Append(id.Name.Replace("::", "::"));
                    break;

                case FunctionCallNode call:
                    _code.Append(call.FunctionName.Replace("::", "::") + "(");
                    for (int i = 0; i < call.Arguments.Count; i++)
                    {
                        VisitExpression(call.Arguments[i]);
                        if (i < call.Arguments.Count - 1) _code.Append(", ");
                    }
                    _code.Append(")");
                    break;

                case InterpolatedStringNode interpolated:
                    _code.Append("(");
                    for (int i = 0; i < interpolated.Parts.Count; i++)
                    {
                        var part = interpolated.Parts[i];
                        if (part is LiteralNode lit && lit.TypeName == "string")
                        {
                            _code.Append($"std::string(\"{lit.Value}\")");
                        }
                        else
                        {
                            _code.Append("sage_to_string(");
                            VisitExpression(part);
                            _code.Append(")");
                        }

                        if (i < interpolated.Parts.Count - 1)
                            _code.Append(" + ");
                    }
                    _code.Append(")");
                    break;

                case ParenthesizedExpressionNode paren:
                    _code.Append("(");
                    VisitExpression(paren.Expression);
                    _code.Append(")");
                    break;
            }
        }

        private string GetOperator(TokenType type) => type switch
        {
            TokenType.Plus => "+",
            TokenType.Minus => "-",
            TokenType.Asterisk => "*",
            TokenType.Slash => "/",
            TokenType.Equals => "=",
            _ => throw new Exception("Unknown operator")
        };
    }
}