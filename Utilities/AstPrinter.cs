using System.Text.Json;
using System.Text.Json.Serialization;
using Sage.Ast;
using Sage.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Sage.Utilities
{
    /// <summary>
    /// Utility class that traverses the Abstract Syntax Tree (AST) to generate a 
    /// standardized JSON representation. Highly efficient for external tool integration.
    /// </summary>
    public class AstPrinter : IAstVisitor<object>
    {
        /// <summary>
        /// Entry point to serialize the entire program into an indented JSON string.
        /// </summary>
        /// <param name="node">The root ProgramNode of the AST.</param>
        /// <returns>A formatted JSON string representing the full AST hierarchy.</returns>
        public string Print(ProgramNode node)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                // Ensures TokenType enums are readable strings
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Serialize(node.Accept(this), options);
        }

        public object Visit(ProgramNode node) => new
        {
            Type = "Program",
            Body = node.Statements.Select(s => s.Accept(this))
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
            Name = node.Name,
            DataType = node.Type,
            Initializer = node.Initializer.Accept(this)
        };

        public object Visit(ReturnNode node) => new
        {
            Type = "Return",
            Expression = node.Expression.Accept(this)
        };

        public object Visit(ExpressionStatementNode node) => new
        {
            Type = "ExpressionStatement",
            Expression = node.Expression.Accept(this)
        };

        public object Visit(UseNode node) => new
        {
            Type = "Use",
            Module = node.Module
        };

        public object Visit(BinaryExpressionNode node) => new
        {
            Type = "BinaryExpression",
            Operator = node.Operator,
            Left = node.Left.Accept(this),
            Right = node.Right.Accept(this)
        };

        public object Visit(CastExpressionNode node) => new
        {
            Type = "Cast",
            TargetType = node.TargetType,
            Value = node.Expression.Accept(this)
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

        public object Visit(FunctionCallNode node) => new
        {
            Type = "FunctionCall",
            Name = node.Name,
            Arguments = node.Arguments.Select(a => a.Accept(this))
        };

        public object Visit(InterpolatedStringNode node) => new
        {
            Type = "InterpolatedString",
            Parts = node.Parts.Select(p => p.Accept(this))
        };
    }
}