using Sage.Ast;
using Sage.Enums;
using Sage.Interfaces;
using System.Text;

namespace Sage.Core
{
    public class CodeGenerator : ICodeGenerator, IAstVisitor<string>
    {
        private int _indent = 0;
        private string Indent => new string(' ', _indent * 4);

        public string Generate(ProgramNode ast)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#include <stdio.h>");
            sb.AppendLine("#include <stdint.h>");
            sb.AppendLine("// Sage Standard Types");
            sb.AppendLine("typedef int32_t i32;");
            sb.AppendLine("typedef void none;");
            sb.AppendLine("typedef char* string;");
            sb.AppendLine("");
            sb.AppendLine("// --- Generated Code ---");

            sb.Append(ast.Accept(this));
            return sb.ToString();
        }

        public string Visit(ProgramNode node)
        {
            var sb = new StringBuilder();
            foreach (var stmt in node.Statements) sb.Append(stmt.Accept(this));
            return sb.ToString();
        }

        public string Visit(ModuleNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"// Module: {node.Name}");
            foreach (var func in node.Functions)
            {
                func.ModuleOwner = node.Name;
                sb.Append(func.Accept(this));
            }
            return sb.ToString();
        }

        public string Visit(FunctionDeclarationNode node)
        {
            var sb = new StringBuilder();
            string cName = node.Name;
            string cRet = node.ReturnType == "none" ? "void" : node.ReturnType;

            if (!string.IsNullOrEmpty(node.ModuleOwner)) cName = $"{node.ModuleOwner}_{node.Name}";
            if (node.Name.ToLower() == "main") cName = "main";

            sb.Append($"{Indent}{cRet} {cName}(");

            for (int i = 0; i < node.Parameters.Count; i++)
            {
                sb.Append($"{node.Parameters[i].Type} {node.Parameters[i].Name}");
                if (i < node.Parameters.Count - 1) sb.Append(", ");
            }

            sb.AppendLine(")");
            sb.Append(node.Body.Accept(this));
            return sb.ToString();
        }

        public string Visit(BlockNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Indent}{{");
            _indent++;
            foreach (var stmt in node.Statements) sb.Append(stmt.Accept(this));
            _indent--;
            sb.AppendLine($"{Indent}}}");
            return sb.ToString();
        }

        public string Visit(VariableDeclarationNode node) =>
            $"{Indent}{node.Type} {node.Name} = {node.Initializer.Accept(this)};\n";

        public string Visit(ReturnNode node) =>
            $"{Indent}return {node.Expression.Accept(this)};\n";

        public string Visit(ExpressionStatementNode node) =>
            $"{Indent}{node.Expression.Accept(this)};\n";

        public string Visit(UseNode node) => $"// use {node.Module};\n";

        public string Visit(BinaryExpressionNode node)
        {
            string op = node.Operator switch
            {
                TokenType.Plus => "+",
                TokenType.Minus => "-",
                TokenType.Asterisk => "*",
                TokenType.Slash => "/",
                _ => "?"
            };
            return $"({node.Left.Accept(this)} {op} {node.Right.Accept(this)})";
        }

        public string Visit(LiteralNode node)
        {
            if (node.TypeName == "string") return $"\"{node.Value}\"";
            return node.Value.ToString();
        }

        public string Visit(IdentifierNode node) => node.Name.Replace("::", "_");

        public string Visit(FunctionCallNode node)
        {
            // Tratamento especial para console::print_line -> printf
            if (node.Name == "console::print_line")
            {
                var sb = new StringBuilder();
                sb.Append("printf(");

                if (node.Args.Count > 0 && node.Args[0] is LiteralNode lit)
                {
                    string template = lit.Value.ToString();

                    var vars = new List<string>();
                    var cleanTemplate = new StringBuilder();
                    bool inside = false;
                    var currentVar = new StringBuilder();

                    foreach (char c in template)
                    {
                        if (c == '{') { inside = true; cleanTemplate.Append("%d"); continue; }
                        if (c == '}')
                        {
                            inside = false;
                            // CORREÇÃO AQUI: Sanitizar a variável extraída (:: -> _)
                            string rawVar = currentVar.ToString();
                            vars.Add(rawVar.Replace("::", "_"));
                            currentVar.Clear();
                            continue;
                        }
                        if (inside) currentVar.Append(c);
                        else cleanTemplate.Append(c);
                    }

                    cleanTemplate.Append("\\n");
                    sb.Append($"\"{cleanTemplate}\"");

                    if (vars.Count > 0) sb.Append(", ");
                    sb.Append(string.Join(", ", vars));
                }
                sb.Append(")");
                return sb.ToString();
            }

            // Chamadas normais: math::sum -> math_sum
            string cName = node.Name.Replace("::", "_");

            var args = node.Args.Select(a => a.Accept(this));
            return $"{cName}({string.Join(", ", args)})";
        }

        public string Visit(InterpolatedStringNode node) => "\"Not Implemented\"";
    }
}
