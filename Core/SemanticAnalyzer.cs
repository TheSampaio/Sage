using Sage.Ast;
using Sage.Enums;
using Sage.Interfaces;
using Sage.Utilities;

namespace Sage.Core
{
    /// <summary>
    /// Traverses the Abstract Syntax Tree (AST) to verify the semantic correctness of the program.
    /// It performs type checking, scope management, and enforces Sage's security and language rules.
    /// </summary>
    /// <param name="sharedTable">An optional pre-populated symbol table, useful for multi-module compilation.</param>
    public class SemanticAnalyzer(SymbolTable? sharedTable = null) : IAstVisitor<string>
    {
        private readonly SymbolTable _symbolTable = sharedTable ?? new SymbolTable();

        /// <summary>
        /// Starts the semantic analysis process on the provided program AST.
        /// </summary>
        /// <param name="ast">The root node of the program to analyze.</param>
        public void Analyze(ProgramNode ast)
        {
            ast.Accept(this);
        }

        /// <summary>
        /// Pre-registers all functions and modules in the symbol table. 
        /// This allows for forward references and cross-module calls.
        /// </summary>
        /// <param name="moduleAst">The AST of the module to register.</param>
        public void RegisterModuleSymbols(ProgramNode moduleAst)
        {
            foreach (var node in moduleAst.Statements.OfType<ModuleNode>())
            {
                foreach (var func in node.Functions)
                {
                    // Register both the direct name and the namespaced name
                    _symbolTable.Define(func.Name, func.ReturnType, isFunction: true, isExtern: func.IsExtern);
                    _symbolTable.Define($"{node.Name}::{func.Name}", func.ReturnType, isFunction: true, isExtern: func.IsExtern);
                }
            }
        }

        /// <summary>Determines if a Sage type name represents a numeric value.</summary>
        private static bool IsNumeric(string type) =>
            type is "i8" or "u8" or "i16" or "u16" or "i32" or "u32" or "i64" or "u64" or "f32" or "f64";

        /// <summary>Determines if a Sage type name represents a floating-point value.</summary>
        private static bool IsFloatingPoint(string type) => type is "f32" or "f64";

        /// <summary>
        /// Determines the "dominant" type between two numeric types to handle implicit promotion.
        /// For example, adding i32 and f32 results in f32.
        /// </summary>
        private static string? GetDominantType(string typeA, string typeB)
        {
            var hierarchy = new List<string> { "i8", "u8", "i16", "u16", "i32", "u32", "i64", "u64", "f32", "f64" };
            int indexA = hierarchy.IndexOf(typeA);
            int indexB = hierarchy.IndexOf(typeB);
            if (indexA == -1 || indexB == -1) return null;
            return hierarchy[Math.Max(indexA, indexB)];
        }

        /// <summary>
        /// Verifies if a source node's value can be assigned to a target data type.
        /// Supports implicit numeric promotion for literals.
        /// </summary>
        private bool CanAssign(string targetType, AstNode sourceNode)
        {
            string sourceType = sourceNode.Accept(this);
            if (targetType == sourceType) return true;

            if (sourceNode is LiteralNode)
            {
                // Allow integer literals to be assigned to floating point variables
                if (IsFloatingPoint(targetType) && IsNumeric(sourceType)) return true;
            }

            if (targetType == "f64" && sourceType == "f32") return true;
            return false;
        }

        /// <summary>Analyzes the program entry point.</summary>
        public string Visit(ProgramNode node)
        {
            foreach (var stmt in node.Statements) stmt.Accept(this);
            return "none";
        }

        /// <summary>Analyzes a module and its contents.</summary>
        public string Visit(ModuleNode node)
        {
            foreach (var func in node.Functions)
            {
                _symbolTable.Define(func.Name, func.ReturnType, isFunction: true, isExtern: func.IsExtern);
                _symbolTable.Define($"{node.Name}::{func.Name}", func.ReturnType, isFunction: true, isExtern: func.IsExtern);
            }

            foreach (var func in node.Functions) func.Accept(this);
            return "none";
        }

        /// <summary>Verifies function signatures, parameters, and body logic.</summary>
        public string Visit(FunctionDeclarationNode node)
        {
            if (!_symbolTable.IsDefinedInCurrentScope(node.Name))
                _symbolTable.Define(node.Name, node.ReturnType, isFunction: true, isExtern: node.IsExtern);

            _symbolTable.EnterScope();
            foreach (var param in node.Parameters)
            {
                _symbolTable.Define(param.Name, param.Type);
            }

            if (!node.IsExtern && node.Body != null)
            {
                node.Body.Accept(this);
            }

            _symbolTable.ExitScope();
            return node.ReturnType;
        }

        /// <summary>Checks function existence and enforces security rules for external calls.</summary>
        public string Visit(FunctionCallNode node)
        {
            var symbol = _symbolTable.Resolve(node.Name) ?? throw new CompilerException(node, "S105", $"Function '{node.Name}' not found.");

            // Security: Prevent accessing raw C functions if they are abstracted in a module
            if (symbol.IsExtern && node.Name.Contains("::"))
            {
                throw new CompilerException(
                    node,
                    "S005",
                    $"Security Violation: Direct access to extern function '{node.Name}' is prohibited."
                );
            }

            foreach (var arg in node.Arguments) arg.Accept(this);
            return symbol.Type;
        }

        /// <summary>Resolves identifier types and ensures they are declared.</summary>
        public string Visit(IdentifierNode node)
        {
            var symbol = _symbolTable.Resolve(node.Name) ?? throw new CompilerException(node, "S105", $"Identifier '{node.Name}' not declared.");
            node.VariableType = symbol.Type;
            return symbol.Type;
        }

        /// <summary>Manages scope for code blocks.</summary>
        public string Visit(BlockNode node)
        {
            _symbolTable.EnterScope();
            foreach (var stmt in node.Statements) stmt.Accept(this);
            _symbolTable.ExitScope();
            return "none";
        }

        /// <summary>Validates variable declarations and mandatory initialization.</summary>
        public string Visit(VariableDeclarationNode node)
        {
            if (_symbolTable.IsDefinedInCurrentScope(node.Name))
                throw new CompilerException(node, "S102", $"Variable '{node.Name}' is already defined.");

            if (!CanAssign(node.Type, node.Initializer))
            {
                string sourceType = node.Initializer.Accept(this);
                throw new CompilerException(node, "S101", $"Type Mismatch: Cannot assign {sourceType} to {node.Type}.");
            }

            if (node.Initializer is LiteralNode literal) literal.TypeName = node.Type;

            _symbolTable.Define(node.Name, node.Type);
            return node.Type;
        }

        /// <summary>Validates type safety during assignment.</summary>
        public string Visit(AssignmentNode node)
        {
            var symbol = _symbolTable.Resolve(node.Name) ?? throw new CompilerException(node, "S105", $"Variable '{node.Name}' not declared.");
            node.VariableType = symbol.Type;

            if (!CanAssign(symbol.Type, node.Expression))
            {
                string exprType = node.Expression.Accept(this);
                throw new CompilerException(node, "S108", $"Type Mismatch: Cannot assign {exprType} to {symbol.Type}.");
            }

            if (node.Expression is LiteralNode literal) literal.TypeName = symbol.Type;
            return symbol.Type;
        }

        /// <summary>Performs type checking for binary operations and determines resulting types.</summary>
        public string Visit(BinaryExpressionNode node)
        {
            string lhs = node.Left.Accept(this);
            string rhs = node.Right.Accept(this);

            // Comparison operators always return boolean (b8)
            if (node.Operator is TokenType.EqualEqual or TokenType.NotEqual or TokenType.Less or
                TokenType.LessEqual or TokenType.Greater or TokenType.GreaterEqual) return "b8";

            // Logical operators require boolean operands
            if (node.Operator is TokenType.AmpersandAmpersand or TokenType.PipePipe)
            {
                if (lhs != "b8" || rhs != "b8")
                    throw new CompilerException(node, "S107", "Logical operators require b8.");
                return "b8";
            }

            // Arithmetic promotion
            string? dominantType = GetDominantType(lhs, rhs) ?? throw new CompilerException(node, "S108", $"Invalid arithmetic: {lhs} and {rhs}");
            return dominantType;
        }

        /// <summary>Validates if-statement condition types.</summary>
        public string Visit(IfNode node)
        {
            if (node.Condition.Accept(this) != "b8")
                throw new CompilerException(node, "S103", "Condition must be b8.");
            node.ThenBranch.Accept(this);
            node.ElseBranch?.Accept(this);
            return "none";
        }

        /// <summary>Validates while-loop condition types.</summary>
        public string Visit(WhileNode node)
        {
            if (node.Condition.Accept(this) != "b8")
                throw new CompilerException(node, "S104", "Condition must be b8.");
            node.Body.Accept(this);
            return "none";
        }

        /// <summary>Validates for-loop structure and types.</summary>
        public string Visit(ForNode node)
        {
            _symbolTable.EnterScope();
            node.Initializer?.Accept(this);
            if (node.Condition != null && node.Condition.Accept(this) != "b8")
                throw new CompilerException(node, "S104", "Condition must be b8.");
            node.Increment?.Accept(this);
            node.Body.Accept(this);
            _symbolTable.ExitScope();
            return "none";
        }

        // --- Literal and Leaf Nodes ---
        public string Visit(LiteralNode node) => node.TypeName;
        public string Visit(ReturnNode node) => node.Expression.Accept(this);
        public string Visit(ExpressionStatementNode node) => node.Expression.Accept(this);
        public string Visit(UnaryExpressionNode node) => node.Operand.Accept(this);
        public string Visit(CastExpressionNode node) => node.TargetType;
        public string Visit(UseNode node) => "none";
        public string Visit(InterpolatedStringNode node) => "string";
    }
}