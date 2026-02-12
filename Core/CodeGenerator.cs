using Sage.Core.AST;
using Sage.Enums;
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
            // Initial C++ boilerplate
            WriteLine("#include <iostream>");
            WriteLine("#include <string>");
            WriteLine();

            // Basic Sage Runtime Emulation in C++
            WriteLine("namespace Console {");
            _indentationLevel++;
            WriteLine("void Print(std::string text) { std::cout << text << std::endl; }");
            WriteLine("void Print(int value) { std::cout << value << std::endl; }");
            _indentationLevel--;
            WriteLine("}");
            WriteLine();

            foreach (var stmt in program.Statements)
            {
                Visit(stmt);
            }

            return _code.ToString();
        }

        private void Visit(AstNode node)
        {
            switch (node)
            {
                case UseNode use:
                    // In C++, 'use' could be translated to includes or namespace usages
                    WriteLine($"// Used module: {use.ModuleName}");
                    break;

                case FunctionDeclarationNode func:
                    // Map Sage types to C++ types (i32 -> int)
                    string returnType = func.ReturnType == "i32" ? "int" : "void";
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
                    string cppType = varDecl.Type == "i32" ? "int" : varDecl.Type;
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
                    else
                        _code.Append(lit.Value);
                    break;

                case IdentifierNode id:
                    // Convert Sage double colon (::) to C++ (::)
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
                    // We will use a sequence of << in C++ or a temp stringstream
                    // For simplicity with Console::Print, we'll wrap it in a parenthesized block
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
                            // Convert numbers/others to string using std::to_string
                            _code.Append("std::to_string(");
                            VisitExpression(part);
                            _code.Append(")");
                        }

                        if (i < interpolated.Parts.Count - 1)
                            _code.Append(" + ");
                    }
                    _code.Append(")");
                    break;

                // Inside VisitExpression() switch
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