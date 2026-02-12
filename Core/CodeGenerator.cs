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
            // 1. Headers Standard do C
            WriteLine("#include <stdio.h>");
            WriteLine("#include <stdint.h>");
            WriteLine("#include <stdbool.h>");
            WriteLine("#include <string.h>");
            WriteLine();

            // 2. Definições de Tipos (Usando Typedef do C)
            WriteLine("// --- Sage Type Definitions ---");
            WriteLine("typedef uint8_t   u8;");
            WriteLine("typedef uint16_t  u16;");
            WriteLine("typedef uint32_t  u32;");
            WriteLine("typedef uint64_t  u64;");
            WriteLine("typedef int8_t    i8;");
            WriteLine("typedef int16_t   i16;");
            WriteLine("typedef int32_t   i32;");
            WriteLine("typedef int64_t   i64;");
            WriteLine("typedef float     f32;");
            WriteLine("typedef double    f64;");
            WriteLine("typedef bool      b8;");
            WriteLine("typedef char      c8;");
            WriteLine("typedef const char* str;");
            WriteLine("typedef void      none;");
            WriteLine("// -----------------------------");
            WriteLine();

            // 3. Emulação de Namespace e Overloading (C11 _Generic)
            WriteLine("// --- Console Module (C Emulation) ---");
            WriteLine("void _Console_PrintLine_Str(str text) { printf(\"%s\\n\", text); }");
            WriteLine("void _Console_PrintLine_I32(i32 val)  { printf(\"%d\\n\", val); }");
            WriteLine("void _Console_PrintLine_F32(f32 val)  { printf(\"%f\\n\", val); }");
            WriteLine();

            // O Macro _Generic permite que Console_PrintLine funcione com vários tipos no C puro
            WriteLine("#define Console_PrintLine(x) _Generic((x), \\");
            WriteLine("    char*: _Console_PrintLine_Str, \\");
            WriteLine("    const char*: _Console_PrintLine_Str, \\");
            WriteLine("    i32: _Console_PrintLine_I32, \\");
            WriteLine("    f32: _Console_PrintLine_F32 \\");
            WriteLine(")(x)");
            WriteLine();

            foreach (var stmt in program.Statements)
            {
                Visit(stmt);
            }

            return _code.ToString();
        }

        private string MapType(string sageType)
        {
            // Como usamos typedefs no topo do arquivo C, 
            // a maioria dos nomes (u8, i32, none) já é válida no C gerado.
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
                    string returnType = MapType(func.ReturnType);
                    // No C puro, Main vira main
                    string cFuncName = func.Name == "Main" ? "main" : func.Name;

                    WriteLine($"{returnType} {cFuncName}()");
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
                    _code.Append(new string('\t', _indentationLevel));
                    _code.Append($"{MapType(varDecl.Type)} {varDecl.Name} = ");
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
                    // CORREÇÃO: Verifica tanto "string" (do Parser) quanto "str" (do Sage)
                    if (lit.TypeName == "string" || lit.TypeName == "str")
                    {
                        _code.Append($"\"{lit.Value}\"");
                    }
                    // Adiciona sufixo 'f' para floats para evitar warnings de double no C
                    else if (lit.TypeName == "float" || lit.TypeName == "f32")
                    {
                        _code.Append($"{lit.Value}f");
                    }
                    else
                    {
                        _code.Append(lit.Value);
                    }
                    break;

                case IdentifierNode id:
                    // Transforma Console::PrintLine em Console_PrintLine
                    _code.Append(id.Name.Replace("::", "_"));
                    break;

                case FunctionCallNode call:
                    // Transforma chamada de função com namespace para C style
                    _code.Append(call.FunctionName.Replace("::", "_") + "(");
                    for (int i = 0; i < call.Arguments.Count; i++)
                    {
                        VisitExpression(call.Arguments[i]);
                        if (i < call.Arguments.Count - 1) _code.Append(", ");
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