using Sage.Ast;
using Sage.Interfaces;
using System.Text;

namespace Sage.Utilities
{
    public class AstPrinter : IAstVisitor<string>
    {
        private int _indent = 0;
        private string IndentStr => new string(' ', _indent * 2);

        public string Print(ProgramNode node) => node.Accept(this);

        public string Visit(ProgramNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Program");
            foreach (var stmt in node.Statements) sb.Append(stmt.Accept(this));
            return sb.ToString();
        }

        public string Visit(ModuleNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{IndentStr}Module '{node.Name}' {{");
            _indent++;
            foreach (var func in node.Functions) sb.Append(func.Accept(this));
            _indent--;
            sb.AppendLine($"{IndentStr}}}");
            return sb.ToString();
        }

        public string Visit(FunctionDeclarationNode node)
        {
            var sb = new StringBuilder();
            string paramsStr = string.Join(", ", node.Parameters.Select(p => $"{p.Name}: {p.Type}"));
            sb.AppendLine($"{IndentStr}Func {node.Name}({paramsStr}) -> {node.ReturnType}");
            sb.Append(node.Body.Accept(this));
            return sb.ToString();
        }

        public string Visit(BlockNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{IndentStr}{{");
            _indent++;
            foreach (var stmt in node.Statements) sb.Append(stmt.Accept(this));
            _indent--;
            sb.AppendLine($"{IndentStr}}}");
            return sb.ToString();
        }

        public string Visit(VariableDeclarationNode node) =>
            $"{IndentStr}Var {node.Name}: {node.Type} = {node.Initializer.Accept(this)}\n";

        public string Visit(ReturnNode node) =>
            $"{IndentStr}Return {node.Expression.Accept(this)}\n";

        public string Visit(ExpressionStatementNode node) =>
            $"{IndentStr}Expr: {node.Expression.Accept(this)}\n";

        public string Visit(UseNode node) =>
            $"{IndentStr}Use {node.Module}\n";

        public string Visit(BinaryExpressionNode node) =>
            $"({node.Left.Accept(this)} {node.Operator} {node.Right.Accept(this)})";

        public string Visit(LiteralNode node) =>
            $"{node.Value}{(node.TypeName == "string" ? "" : "")}"; // Simplificado

        public string Visit(IdentifierNode node) => node.Name;

        public string Visit(FunctionCallNode node) =>
            $"Call {node.Name}({string.Join(", ", node.Args.Select(a => a.Accept(this)))})";

        public string Visit(InterpolatedStringNode node) => "\"interpolated_string\"";
    }
}
