using Sage.Ast;
using Sage.Interfaces;

namespace Sage.Core
{
    /// <summary>
    /// Performs semantic validation on the Abstract Syntax Tree (AST).
    /// Ensures variables are declared before use and validates string interpolation contents.
    /// </summary>
    public class SemanticAnalyzer : IAstVisitor<bool>
    {
        private readonly SymbolTable _symbolTable = new();

        /// <summary>
        /// Orchestrates the semantic analysis of the entire program.
        /// </summary>
        /// <param name="program">The root node of the program.</param>
        public void Analyze(ProgramNode program)
        {
            program.Accept(this);
        }

        public bool Visit(ProgramNode node)
        {
            foreach (var stmt in node.Statements) stmt.Accept(this);
            return true;
        }

        public bool Visit(ModuleNode node)
        {
            foreach (var func in node.Functions) func.Accept(this);
            return true;
        }

        /// <summary>
        /// Manages function-level scoping and parameter registration.
        /// </summary>
        public bool Visit(FunctionDeclarationNode node)
        {
            _symbolTable.EnterScope();

            foreach (var param in node.Parameters)
            {
                _symbolTable.Define(param.Name);
            }

            node.Body.Accept(this);

            _symbolTable.ExitScope();
            return true;
        }

        public bool Visit(BlockNode node)
        {
            foreach (var stmt in node.Statements) stmt.Accept(this);
            return true;
        }

        /// <summary>
        /// Validates variable initialization and registers the new identifier in the current scope.
        /// </summary>
        public bool Visit(VariableDeclarationNode node)
        {
            // Validate the initializer first to prevent using the variable within its own definition
            node.Initializer.Accept(this);

            _symbolTable.Define(node.Name);
            return true;
        }

        public bool Visit(IdentifierNode node)
        {
            // Namespaced identifiers (e.g., math::sum) are currently treated as external/global
            if (node.Name.Contains("::")) return true;

            if (!_symbolTable.Resolve(node.Name))
            {
                throw new Exception($"[SEMANTIC ERROR] Variable '{node.Name}' used but not declared.");
            }
            return true;
        }

        /// <summary>
        /// Validates standard function calls and performs deep inspection of string templates for console output.
        /// </summary>
        public bool Visit(FunctionCallNode node)
        {
            foreach (var arg in node.Arguments)
            {
                arg.Accept(this);
            }

            // Deep validation for interpolation in console::print_line calls
            if (node.Name == "console::print_line" && node.Arguments.Count > 0 && node.Arguments[0] is LiteralNode lit)
            {
                string template = lit.Value.ToString();
                ValidateStringInterpolation(template);
            }

            return true;
        }

        /// <summary>
        /// Scans a string literal for interpolation markers {} and validates the expressions inside them.
        /// </summary>
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
        /// Validates identifiers used within interpolation braces, including basic function call arguments.
        /// </summary>
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
                        // Filter out numeric literals and validate actual variable names
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
        private void CheckVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (name.Contains("::")) return;

            if (!_symbolTable.Resolve(name))
            {
                throw new Exception($"[SEMANTIC ERROR] Variable '{name}' used inside string interpolation but not declared.");
            }
        }

        public bool Visit(ReturnNode node) { node.Expression.Accept(this); return true; }
        public bool Visit(ExpressionStatementNode node) { node.Expression.Accept(this); return true; }
        public bool Visit(BinaryExpressionNode node) { node.Left.Accept(this); node.Right.Accept(this); return true; }

        public bool Visit(LiteralNode node) => true;
        public bool Visit(InterpolatedStringNode node) => true;
        public bool Visit(UseNode node) => true;
    }
}