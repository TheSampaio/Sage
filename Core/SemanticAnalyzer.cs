using Sage.Ast;
using Sage.Interfaces;
using System;

namespace Sage.Core
{
    /// <summary>
    /// Performs semantic validation on the Abstract Syntax Tree (AST).
    /// Ensures variables are declared before use, validates types, and checks string interpolation contents.
    /// </summary>
    public class SemanticAnalyzer : IAstVisitor<string>
    {
        private readonly SymbolTable _symbolTable = new();

        /// <summary>
        /// Orchestrates the semantic analysis of the entire program.
        /// </summary>
        /// <param name="program">The root node of the program to be analyzed.</param>
        public void Analyze(ProgramNode program)
        {
            program.Accept(this);
        }

        /// <summary>Visits the program root and analyzes all top-level statements.</summary>
        /// <param name="node">The program node.</param>
        /// <returns>The string "none".</returns>
        public string Visit(ProgramNode node)
        {
            foreach (var stmt in node.Statements) stmt.Accept(this);
            return "none";
        }

        /// <summary>Visits a module and analyzes its internal function declarations.</summary>
        /// <param name="node">The module node.</param>
        /// <returns>The string "none".</returns>
        public string Visit(ModuleNode node)
        {
            foreach (var func in node.Functions) func.Accept(this);
            return "none";
        }

        /// <summary>
        /// Manages function-level scoping and parameter registration with their respective types.
        /// </summary>
        /// <param name="node">The function declaration node.</param>
        /// <returns>The declared return type of the function.</returns>
        public string Visit(FunctionDeclarationNode node)
        {
            _symbolTable.EnterScope();

            foreach (var param in node.Parameters)
            {
                _symbolTable.Define(param.Name, param.Type);
            }

            node.Body.Accept(this);

            _symbolTable.ExitScope();
            return node.ReturnType;
        }

        /// <summary>Visits a block and analyzes all statements within it.</summary>
        /// <param name="node">The block node.</param>
        /// <returns>The string "none".</returns>
        public string Visit(BlockNode node)
        {
            foreach (var stmt in node.Statements) stmt.Accept(this);
            return "none";
        }

        /// <summary>
        /// Validates variable initialization, checks type compatibility, and registers the identifier.
        /// </summary>
        /// <param name="node">The variable declaration node.</param>
        /// <returns>The type of the declared variable.</returns>
        /// <exception cref="Exception">Thrown when types between declaration and initializer mismatch.</exception>
        public string Visit(VariableDeclarationNode node)
        {
            string initializerType = node.Initializer.Accept(this);

            if (node.Type != initializerType)
            {
                // Basic promotion: allow i32 to be assigned to f64
                if (node.Type == "f64" && initializerType == "i32") { /* OK */ }
                else
                {
                    throw new Exception($"[TYPE ERROR] Cannot assign {initializerType} to variable '{node.Name}' of type {node.Type}.");
                }
            }

            _symbolTable.Define(node.Name, node.Type);
            return node.Type;
        }

        /// <summary>Resolves the type of an identifier from the symbol table.</summary>
        /// <param name="node">The identifier node.</param>
        /// <returns>The resolved Sage type of the identifier.</returns>
        /// <exception cref="Exception">Thrown if the variable has not been declared.</exception>
        public string Visit(IdentifierNode node)
        {
            if (node.Name.Contains("::")) return "f64"; // Default for external modules for now

            string type = _symbolTable.Resolve(node.Name);
            if (type == null)
            {
                throw new Exception($"[SEMANTIC ERROR] Variable '{node.Name}' used but not declared.");
            }
            return type;
        }

        /// <summary>
        /// Validates function calls and performs deep inspection of string templates for console output.
        /// </summary>
        /// <param name="node">The function call node.</param>
        /// <returns>The return type of the function (currently defaults to "f64").</returns>
        public string Visit(FunctionCallNode node)
        {
            foreach (var arg in node.Arguments)
            {
                arg.Accept(this);
            }

            if (node.Name == "console::print_line" && node.Arguments.Count > 0 && node.Arguments[0] is LiteralNode lit)
            {
                string template = lit.Value.ToString();
                ValidateStringInterpolation(template);
            }

            return "f64";
        }

        /// <summary>Analyzes a binary expression and ensures both sides have compatible types.</summary>
        /// <param name="node">The binary expression node.</param>
        /// <returns>The resulting type of the binary operation.</returns>
        public string Visit(BinaryExpressionNode node)
        {
            string leftType = node.Left.Accept(this);
            string rightType = node.Right.Accept(this);

            if (leftType != rightType)
            {
                if ((leftType == "f64" && rightType == "i32") || (leftType == "i32" && rightType == "f64"))
                    return "f64";

                throw new Exception($"[TYPE ERROR] Mismatched types in binary expression: {leftType} and {rightType}.");
            }

            return leftType;
        }

        /// <summary>Returns the type name of the literal value.</summary>
        /// <param name="node">The literal node.</param>
        /// <returns>The literal's type name.</returns>
        public string Visit(LiteralNode node) => node.TypeName;

        /// <summary>Analyzes a return statement.</summary>
        /// <param name="node">The return node.</param>
        /// <returns>The type of the returned expression.</returns>
        public string Visit(ReturnNode node) => node.Expression.Accept(this);

        /// <summary>Analyzes a standalone expression statement.</summary>
        /// <param name="node">The expression statement node.</param>
        /// <returns>The type of the underlying expression.</returns>
        public string Visit(ExpressionStatementNode node) => node.Expression.Accept(this);

        /// <summary>Analyzes a 'use' directive.</summary>
        /// <param name="node">The use node.</param>
        /// <returns>The string "none".</returns>
        public string Visit(UseNode node) => "none";

        /// <summary>Analyzes an interpolated string node.</summary>
        /// <param name="node">The interpolated string node.</param>
        /// <returns>The string type "str".</returns>
        public string Visit(InterpolatedStringNode node) => "str";

        #region Helper Methods

        /// <summary>
        /// Scans a string literal for interpolation markers {} and validates the expressions inside them.
        /// </summary>
        /// <param name="template">The raw string template content.</param>
        private void ValidateStringInterpolation(string template)
        {
            int i = 0;
            while (i < template.Length)
            {
                if (template[i] == '{')
                {
                    i++;
                    int start = i;
                    while (i < template.Length && template[i] != '}') i++;

                    if (i < template.Length)
                    {
                        string content = template[start..i].Trim();
                        ValidateInterpolatedExpression(content);
                    }
                }
                i++;
            }
        }

        /// <summary>
        /// Validates identifiers used within interpolation braces.
        /// </summary>
        /// <param name="expression">The expression content inside the braces.</param>
        private void ValidateInterpolatedExpression(string expression)
        {
            if (expression.Contains('('))
            {
                int open = expression.IndexOf('(');
                int close = expression.LastIndexOf(')');
                if (open != -1 && close > open)
                {
                    string argsStr = expression.Substring(open + 1, close - open - 1);
                    var args = argsStr.Split(',');
                    foreach (var arg in args)
                    {
                        string varName = arg.Trim();
                        if (!string.IsNullOrEmpty(varName) && !char.IsDigit(varName[0]))
                        {
                            CheckVariable(varName);
                        }
                    }
                }
            }
            else
            {
                CheckVariable(expression);
            }
        }

        /// <summary>
        /// Performs a symbol table lookup for a specific variable name.
        /// </summary>
        /// <param name="name">The variable name to check.</param>
        private void CheckVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Contains("::")) return;

            if (_symbolTable.Resolve(name) == null)
            {
                throw new Exception($"[SEMANTIC ERROR] Variable '{name}' used inside string interpolation but not declared.");
            }
        }

        #endregion
    }
}