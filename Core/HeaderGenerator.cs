using System.Text;
using Sage.Ast;
using Sage.Interfaces;

namespace Sage.Core
{
    /// <summary>
    /// Generates C header files (.h) for Sage modules to facilitate modular compilation.
    /// </summary>
    public class HeaderGenerator : IAstVisitor<string>
    {
        public string Generate(ProgramNode ast)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/* --- Sage Header File --- */");
            sb.AppendLine("#pragma once");
            sb.AppendLine("#include <stdint.h>");
            sb.AppendLine("#include <stdbool.h>");

            sb.AppendLine("\n/* --- Type Definitions --- */");
            sb.AppendLine("typedef float    f32; typedef double   f64;");
            sb.AppendLine("typedef int32_t  i32; typedef bool     b8;");
            sb.AppendLine("typedef char* str; typedef void     none;");

            sb.AppendLine("\n/* --- Function Prototypes --- */");
            sb.Append(ast.Accept(this));

            return sb.ToString();
        }

        public string Visit(FunctionDeclarationNode node)
        {
            string cName = node.IsExtern
                ? node.Name
                : (node.Name.Equals("main", StringComparison.OrdinalIgnoreCase)
                    ? "main"
                    : (string.IsNullOrEmpty(node.ModuleOwner) ? node.Name : $"{node.ModuleOwner}_{node.Name}"));

            bool isMain = node.Name.Equals("main", StringComparison.OrdinalIgnoreCase);
            string cReturnType = isMain ? "int" : TypeSystem.ToCType(node.ReturnType);

            string paramsStr = (isMain || node.Parameters.Count == 0)
                ? "void"
                : string.Join(", ", node.Parameters.Select(p => $"{TypeSystem.ToCType(p.Type)} {p.Name}"));

            string prefix = node.IsExtern ? "extern " : "";
            return $"{prefix}{cReturnType} {cName}({paramsStr});\n";
        }

        public string Visit(ModuleNode node)
        {
            var sb = new StringBuilder();
            foreach (var func in node.Functions)
            {
                func.ModuleOwner = node.Name;
                sb.Append(func.Accept(this));
            }
            return sb.ToString();
        }

        public string Visit(ProgramNode node)
        {
            var sb = new StringBuilder();
            foreach (var stmt in node.Statements) sb.Append(stmt.Accept(this));
            return sb.ToString();
        }

        // Ignored nodes for Headers
        public string Visit(VariableDeclarationNode node) => "";
        public string Visit(BlockNode node) => "";
        public string Visit(ReturnNode node) => "";
        public string Visit(LiteralNode node) => "";
        public string Visit(IdentifierNode node) => "";
        public string Visit(BinaryExpressionNode node) => "";
        public string Visit(AssignmentNode node) => "";
        public string Visit(ExpressionStatementNode node) => "";
        public string Visit(ExternBlockNode node) => "";
        public string Visit(IfNode node) => "";
        public string Visit(WhileNode node) => "";
        public string Visit(ForNode node) => "";
        public string Visit(FunctionCallNode node) => "";
        public string Visit(UnaryExpressionNode node) => "";
        public string Visit(UseNode node) => "";
        public string Visit(InterpolatedStringNode node) => "";
        public string Visit(CastExpressionNode node) => "";
        public string Visit(StructDeclarationNode node) => "";
        public string Visit(StructInitializationNode node) => "";

        public string Visit(MemberAccessNode node) => "";
    }
}