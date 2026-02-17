using System.Text;
using Sage.Ast;
using Sage.Interfaces;

namespace Sage.Core
{
    /// <summary>
    /// Traverses the Abstract Syntax Tree (AST) to generate C header files (.h).
    /// This generator handles the creation of function prototypes, name mangling for modules, 
    /// and type definitions to ensure compatibility between Sage and the target C code.
    /// </summary>
    public class HeaderGenerator : IAstVisitor<string>
    {
        /// <summary>
        /// Generates a complete C header file string from the provided AST root.
        /// </summary>
        /// <param name="ast">The root <see cref="ProgramNode"/> of the program.</param>
        /// <returns>A string containing the formatted C header code, including guards and prototypes.</returns>
        public string Generate(ProgramNode ast)
        {
            var sb = new StringBuilder();

            // 1. Header Guard and Standard Includes
            sb.AppendLine("/* --- Sage Header File --- */");
            sb.AppendLine("#pragma once");
            sb.AppendLine("#include <stdint.h>");
            sb.AppendLine("#include <stdbool.h>");
            sb.AppendLine("");

            // 2. Standard Type Definitions
            // These bridge Sage types (like i32, b8) to C-compatible types
            sb.AppendLine("/* --- Type Definitions --- */");
            sb.AppendLine("typedef float    f32; typedef double   f64;");
            sb.AppendLine("typedef int32_t  i32; typedef bool     b8;");
            sb.AppendLine("typedef char* str;");
            sb.AppendLine("typedef void     none;");
            sb.AppendLine("");

            // 3. Module Prototypes
            sb.AppendLine("/* --- Function Prototypes --- */");
            sb.Append(ast.Accept(this));

            return sb.ToString();
        }

        /// <summary>
        /// Converts Sage-specific type names to their target C type names.
        /// </summary>
        /// <param name="sageType">The name of the type in Sage.</param>
        /// <returns>The corresponding type name string for C.</returns>
        private static string ConvertType(string sageType)
        {
            return sageType switch
            {
                "none" => "void",
                "str" => "char*",
                _ => sageType
            };
        }

        /// <summary>Visits the program root to process top-level statements.</summary>
        public string Visit(ProgramNode node)
        {
            var sb = new StringBuilder();
            foreach (var stmt in node.Statements)
            {
                sb.Append(stmt.Accept(this));
            }
            return sb.ToString();
        }

        /// <summary>Visits a module and prepares its functions for prototype generation.</summary>
        public string Visit(ModuleNode node)
        {
            var sb = new StringBuilder();
            foreach (var func in node.Functions)
            {
                // Inject the module name to ensure unique function naming in C (mangling)
                func.ModuleOwner = node.Name;
                sb.Append(func.Accept(this));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Visits a function declaration to produce a C function prototype.
        /// Handles name mangling for modules and preserves original names for 'extern' calls.
        /// </summary>
        public string Visit(FunctionDeclarationNode node)
        {
            // Name Resolution: 'extern' functions keep their name; 
            // internal functions are mangled as module_function.
            string cName = node.IsExtern
                ? node.Name
                : (node.Name.Equals("main", StringComparison.OrdinalIgnoreCase)
                    ? "main"
                    : (string.IsNullOrEmpty(node.ModuleOwner) ? node.Name : $"{node.ModuleOwner}_{node.Name}"));

            bool isMain = node.Name.Equals("main", StringComparison.OrdinalIgnoreCase);
            string cReturnType = isMain ? "int" : ConvertType(node.ReturnType);

            // Parameter conversion
            string paramsStr = (isMain || node.Parameters.Count == 0)
                ? "void"
                : string.Join(", ", node.Parameters.Select(p => $"{ConvertType(p.Type)} {p.Name}"));

            string prefix = node.IsExtern ? "extern " : "";

            return $"{prefix}{cReturnType} {cName}({paramsStr});\n";
        }

        // --- Structural and logic nodes that do not produce content in header files ---

        /// <summary>Ignored in header generation.</summary>
        public string Visit(VariableDeclarationNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(BlockNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(ReturnNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(LiteralNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(IdentifierNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(BinaryExpressionNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(AssignmentNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(ExpressionStatementNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(IfNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(WhileNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(ForNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(FunctionCallNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(UnaryExpressionNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(UseNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(InterpolatedStringNode node) => "";
        /// <summary>Ignored in header generation.</summary>
        public string Visit(CastExpressionNode node) => "";
    }
}