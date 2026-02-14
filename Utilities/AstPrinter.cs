using Sage.Ast;
using Sage.Interfaces;
using System.Text;

namespace Sage.Utilities
{
    /// <summary>
    /// Utility class that traverses the Abstract Syntax Tree (AST) to generate a 
    /// human-readable text representation. Useful for debugging and compiler diagnostics.
    /// </summary>
    public class AstPrinter : IAstVisitor<string>
    {
        private int _indent = 0;

        /// <summary>
        /// Generates a string representing the indentation based on the current depth.
        /// </summary>
        private string IndentStr => new(' ', _indent * 2);

        /// <summary>
        /// Main entry point to print the entire program structure.
        /// </summary>
        /// <param name="node">The root ProgramNode.</param>
        /// <returns>A formatted string representing the AST.</returns>
        public string Print(ProgramNode node) => node.Accept(this);

        public string Visit(ProgramNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Program");
            foreach (var stmt in node.Statements)
            {
                sb.Append(stmt.Accept(this));
            }
            return sb.ToString();
        }

        public string Visit(ModuleNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{IndentStr}Module '{node.Name}' {{");

            _indent++;
            foreach (var func in node.Functions)
            {
                sb.Append(func.Accept(this));
            }
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
            foreach (var stmt in node.Statements)
            {
                sb.Append(stmt.Accept(this));
            }
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

        // --- NEW IMPLEMENTATION for Cast Support ---
        public string Visit(CastExpressionNode node) =>
            $"(Cast {node.TargetType} {node.Expression.Accept(this)})";
        // ------------------------------------------

        public string Visit(LiteralNode node) => node.Value?.ToString() ?? "null";

        public string Visit(IdentifierNode node) => node.Name;

        public string Visit(FunctionCallNode node)
        {
            var args = string.Join(", ", node.Arguments.Select(a => a.Accept(this)));
            return $"Call {node.Name}({args})";
        }

        public string Visit(InterpolatedStringNode node)
        {
            var parts = string.Join(" + ", node.Parts.Select(p => p.Accept(this)));
            return $"$\"{parts}\"";
        }
    }
}