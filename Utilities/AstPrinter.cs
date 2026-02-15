using System.Text.Json;
using System.Text.Json.Serialization;
using Sage.Ast;
using Sage.Interfaces;

namespace Sage.Utilities
{
    /// <summary>
    /// Serializes the Abstract Syntax Tree (AST) into a human-readable JSON format.
    /// Useful for debugging the Parser output and verifying tree structure.
    /// </summary>
    public class AstPrinter : IAstVisitor<object>
    {
        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // Ensures TokenType enums are serialized as strings (e.g., "Plus") instead of integers
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Converts the AST starting from the root ProgramNode into a formatted JSON string.
        /// </summary>
        /// <param name="node">The root node of the AST.</param>
        /// <returns>A JSON representation of the tree.</returns>
        public string Print(ProgramNode node)
        {
            return JsonSerializer.Serialize(node.Accept(this), _options);
        }

        public object Visit(ProgramNode node) => new
        {
            Type = "Program",
            Statements = node.Statements.Select(s => s.Accept(this))
        };

        public object Visit(ModuleNode node) => new
        {
            Type = "Module",
            Name = node.Name,
            Functions = node.Functions.Select(f => f.Accept(this))
        };

        public object Visit(FunctionDeclarationNode node) => new
        {
            Type = "FunctionDeclaration",
            Name = node.Name,
            ReturnType = node.ReturnType,
            Parameters = node.Parameters.Select(p => new { p.Name, p.Type }),
            Body = node.Body.Accept(this)
        };

        public object Visit(BlockNode node) => new
        {
            Type = "Block",
            Statements = node.Statements.Select(s => s.Accept(this))
        };

        public object Visit(VariableDeclarationNode node) => new
        {
            Type = "VariableDeclaration",
            IsConstant = node.IsConstant,
            Name = node.Name,
            DataType = node.Type,
            Initializer = node.Initializer.Accept(this)
        };

        public object Visit(AssignmentNode node) => new
        {
            Type = "Assignment",
            Variable = node.Name,
            Value = node.Expression.Accept(this)
        };

        public object Visit(IfNode node) => new
        {
            Type = "If",
            Condition = node.Condition.Accept(this),
            Then = node.ThenBranch.Accept(this),
            Else = node.ElseBranch?.Accept(this)
        };

        public object Visit(WhileNode node) => new
        {
            Type = "While",
            Condition = node.Condition.Accept(this),
            Body = node.Body.Accept(this)
        };

        public object Visit(ForNode node) => new
        {
            Type = "For",
            Initializer = node.Initializer?.Accept(this),
            Condition = node.Condition?.Accept(this),
            Increment = node.Increment?.Accept(this),
            Body = node.Body.Accept(this)
        };

        public object Visit(ReturnNode node) => new
        {
            Type = "Return",
            Value = node.Expression.Accept(this)
        };

        public object Visit(ExpressionStatementNode node) => node.Expression.Accept(this);

        public object Visit(BinaryExpressionNode node) => new
        {
            Type = "BinaryExpression",
            Left = node.Left.Accept(this),
            Operator = node.Operator,
            Right = node.Right.Accept(this)
        };

        public object Visit(UnaryExpressionNode node) => new
        {
            Type = "UnaryExpression",
            Operator = node.Operator,
            Operand = node.Operand.Accept(this),
            IsPostfix = node.IsPostfix
        };

        public object Visit(CastExpressionNode node) => new
        {
            Type = "Cast",
            TargetType = node.TargetType,
            Expression = node.Expression.Accept(this)
        };

        public object Visit(FunctionCallNode node) => new
        {
            Type = "FunctionCall",
            FunctionName = node.Name,
            Arguments = node.Arguments.Select(a => a.Accept(this))
        };

        public object Visit(LiteralNode node) => new
        {
            Type = "Literal",
            Value = node.Value,
            DataType = node.TypeName
        };

        public object Visit(IdentifierNode node) => new
        {
            Type = "Identifier",
            Name = node.Name
        };

        public object Visit(UseNode node) => new { Type = "Use", Module = node.Module };

        public object Visit(InterpolatedStringNode node) => new { Type = "InterpolatedString" };
    }
}