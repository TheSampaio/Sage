using System.Text;
using Sage.Ast;
using Sage.Interfaces;

namespace Sage.Core
{
    /// <summary>
    /// Traverses the AST and generates C header files (.h).
    /// Responsible for emitting function prototypes and type definitions
    /// to support the Sage module system.
    /// </summary>
    public class HeaderGenerator : IAstVisitor<string>
    {
        public string Generate(ProgramNode ast)
        {
            var sb = new StringBuilder();

            // 1. Header Guard and Standard Includes
            sb.AppendLine("/* --- Sage Header File --- */");
            sb.AppendLine("#pragma once");
            sb.AppendLine("#include <stdint.h>");
            sb.AppendLine("#include <stdbool.h>");
            sb.AppendLine("");

            // 2. Standard Type Definitions (Shared with CodeGenerator)
            sb.AppendLine("/* --- Type Definitions --- */");
            sb.AppendLine("typedef float    f32; typedef double   f64;");
            sb.AppendLine("typedef int32_t  i32; typedef bool     b8;");
            sb.AppendLine("typedef void     none;");
            sb.AppendLine("");

            // 3. Module Prototypes
            sb.AppendLine("/* --- Function Prototypes --- */");
            sb.Append(ast.Accept(this));

            return sb.ToString();
        }

        public string Visit(ProgramNode node)
        {
            var sb = new StringBuilder();
            foreach (var stmt in node.Statements)
            {
                sb.Append(stmt.Accept(this));
            }
            return sb.ToString();
        }

        public string Visit(ModuleNode node)
        {
            var sb = new StringBuilder();
            foreach (var func in node.Functions)
            {
                // Inject the module name to ensure C function uniqueness (e.g., math_sum)
                func.ModuleOwner = node.Name;
                sb.Append(func.Accept(this));
            }
            return sb.ToString();
        }

        public string Visit(FunctionDeclarationNode node)
        {
            // Convert Sage types to C types
            string cReturnType = node.ReturnType == "none" ? "void" : node.ReturnType;

            // Resolve C function name (Namespace_Function)
            string cName = string.IsNullOrEmpty(node.ModuleOwner)
                ? node.Name
                : $"{node.ModuleOwner}_{node.Name}";

            // Format parameters using LINQ for cleaner code
            var paramList = node.Parameters.Select(p => $"{p.Type} {p.Name}");
            string paramsStr = string.Join(", ", paramList);

            // Output: "f32 math_sum(f32 x, f32 y);\n"
            return $"{cReturnType} {cName}({paramsStr});\n";
        }

        // --- Nodes ignored in Header Generation ---

        public string Visit(VariableDeclarationNode node) => "";
        public string Visit(BlockNode node) => "";
        public string Visit(ReturnNode node) => "";
        public string Visit(LiteralNode node) => "";
        public string Visit(IdentifierNode node) => "";
        public string Visit(BinaryExpressionNode node) => "";
        public string Visit(AssignmentNode node) => "";
        public string Visit(ExpressionStatementNode node) => "";
        public string Visit(IfNode node) => "";
        public string Visit(WhileNode node) => "";
        public string Visit(ForNode node) => "";
        public string Visit(FunctionCallNode node) => "";
        public string Visit(UnaryExpressionNode node) => "";
        public string Visit(UseNode node) => "";
        public string Visit(InterpolatedStringNode node) => "";
        public string Visit(CastExpressionNode node) => "";
    }
}