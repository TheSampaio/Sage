using Sage.Ast;
using Sage.Enums;
using Sage.Interfaces;
using Sage.Utilities;

namespace Sage.Core
{
    /// <summary>
    /// Performs semantic validation on the AST, including type checking, 
    /// scope management, and AST decoration.
    /// </summary>
    public class SemanticAnalyzer : IAstVisitor<string>
    {
        // Manages nested scopes for variable resolution
        private readonly SymbolTable _symbolTable = new();

        /// <summary>
        /// Public API to start the semantic analysis on the entire AST.
        /// This is the method Program.cs is trying to call.
        /// </summary>
        /// <param name="ast">The root node of the Abstract Syntax Tree.</param>
        public void Analyze(ProgramNode ast)
        {
            // We start the traversal at the root.
            // The visitor pattern handles the rest recursively.
            ast.Accept(this);
        }

        /// <summary>
        /// Checks if a Sage type is considered numeric.
        /// </summary>
        private static bool IsNumeric(string type) =>
            type is "i8" or "u8" or "i16" or "u16" or "i32" or "u32" or "i64" or "u64" or "f32" or "f64";

        /// <summary>
        /// Checks if a type is a floating-point number.
        /// </summary>
        private static bool IsFloatingPoint(string type) => type is "f32" or "f64";

        /// <summary>
        /// Determines the resulting type of a binary operation between two types.
        /// Follows a promotion hierarchy (e.g., f64 > f32 > i32).
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
        /// Validates if a source expression can be assigned to a target type, 
        /// allowing for implicit numeric promotions.
        /// </summary>
        private bool CanAssign(string targetType, AstNode sourceNode)
        {
            string sourceType = sourceNode.Accept(this);

            // Exact match is always allowed
            if (targetType == sourceType) return true;

            // Literals (Contextual Typing)
            // We allow "var x: f32 = 50.5;" because the compiler sees the raw number 
            // and can fit it safely.
            if (sourceNode is LiteralNode)
            {
                if (IsFloatingPoint(targetType) && IsNumeric(sourceType)) return true;
            }

            // Allow f32 -> f64 (Safe)
            if (targetType == "f64" && sourceType == "f32") return true;

            // Allow Integers -> Floats (Safe-ish)
            if (IsFloatingPoint(targetType) && IsNumeric(sourceType) && !IsFloatingPoint(sourceType)) return true;
            
            return false;
        }

        public string Visit(ProgramNode node)
        {
            foreach (var stmt in node.Statements) stmt.Accept(this);
            return "none";
        }

        public string Visit(ModuleNode node)
        {
            foreach (var func in node.Functions) func.Accept(this);
            return "none";
        }

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

        public string Visit(BlockNode node)
        {
            _symbolTable.EnterScope();
            foreach (var stmt in node.Statements) stmt.Accept(this);
            _symbolTable.ExitScope();
            return "none";
        }

        public string Visit(VariableDeclarationNode node)
        {
            // Use IsDefinedInCurrentScope to prevent duplicate declarations in the same block
            if (_symbolTable.IsDefinedInCurrentScope(node.Name))
                throw new CompilerException(null, "S102", $"Variable '{node.Name}' is already defined in this scope.");

            // Validate assignment compatibility
            if (!CanAssign(node.Type, node.Initializer))
            {
                string sourceType = node.Initializer.Accept(this);
                throw new CompilerException(null, "S101",
                    $"Type Mismatch: Cannot assign {sourceType} to variable '{node.Name}' of type {node.Type}.");
            }

            // Contextual Typing: Rewrite literal type to match variable type if applicable
            if (node.Initializer is LiteralNode literal)
            {
                literal.TypeName = node.Type;
            }

            _symbolTable.Define(node.Name, node.Type);
            return node.Type;
        }

        public string Visit(AssignmentNode node)
        {
            string? varType = _symbolTable.Resolve(node.Name);
            if (varType == null)
                throw new CompilerException(null, "S105", $"Variable '{node.Name}' not declared.");

            // Decorate the node so CodeGenerator knows the type later
            node.VariableType = varType;

            if (!CanAssign(varType, node.Expression))
            {
                string exprType = node.Expression.Accept(this);
                throw new CompilerException(null, "S108", $"Type Mismatch: Cannot assign {exprType} to {varType}.");
            }

            // Contextual Typing for assignment
            if (node.Expression is LiteralNode literal)
            {
                literal.TypeName = varType;
            }

            return varType;
        }

        public string Visit(BinaryExpressionNode node)
        {
            string lhs = node.Left.Accept(this);
            string rhs = node.Right.Accept(this);

            // Comparison Operators (Always return b8)
            if (node.Operator is TokenType.EqualEqual or TokenType.NotEqual or TokenType.Less or
                TokenType.LessEqual or TokenType.Greater or TokenType.GreaterEqual)
            {
                return "b8";
            }

            // Logical Operators (Require b8)
            if (node.Operator is TokenType.AmpersandAmpersand or TokenType.PipePipe)
            {
                if (lhs != "b8" || rhs != "b8")
                    throw new CompilerException(null, "S107", "Logical operators require boolean operands.");
                return "b8";
            }

            // Arithmetic Operators (Type Promotion)
            string? dominantType = GetDominantType(lhs, rhs);
            if (dominantType == null)
            {
                throw new CompilerException(null, "S108", $"Arithmetic operation between {lhs} and {rhs} is invalid.");
            }

            return dominantType;
        }

        public string Visit(IdentifierNode node)
        {
            // Simple handling for external namespaces (e.g., math::sum)
            if (node.Name.Contains("::")) return "f64";

            string? type = _symbolTable.Resolve(node.Name);
            if (type == null)
                throw new CompilerException(null, "S105", $"Variable '{node.Name}' not declared.");

            node.VariableType = type;
            return type;
        }

        public string Visit(IfNode node)
        {
            if (node.Condition.Accept(this) != "b8")
                throw new CompilerException(null, "S103", "If condition must evaluate to a boolean (b8).");

            node.ThenBranch.Accept(this);
            node.ElseBranch?.Accept(this);
            return "none";
        }

        public string Visit(WhileNode node)
        {
            if (node.Condition.Accept(this) != "b8")
                throw new CompilerException(null, "S104", "While condition must evaluate to a boolean (b8).");
            node.Body.Accept(this);
            return "none";
        }

        public string Visit(ForNode node)
        {
            _symbolTable.EnterScope(); // Create a new scope for the loop variable
            node.Initializer?.Accept(this);

            if (node.Condition != null && node.Condition.Accept(this) != "b8")
                throw new CompilerException(null, "S104", "For condition must evaluate to a boolean (b8).");

            node.Increment?.Accept(this);
            node.Body.Accept(this);
            _symbolTable.ExitScope();
            return "none";
        }

        public string Visit(LiteralNode node) => node.TypeName;
        public string Visit(ReturnNode node) => node.Expression.Accept(this);
        public string Visit(ExpressionStatementNode node) => node.Expression.Accept(this);
        public string Visit(UnaryExpressionNode node) => node.Operand.Accept(this);
        public string Visit(CastExpressionNode node) => node.TargetType;
        public string Visit(UseNode node) => "none";
        public string Visit(InterpolatedStringNode node) => "string";
        public string Visit(FunctionCallNode node)
        {
            foreach (var arg in node.Arguments) arg.Accept(this);
            return "f64"; // Default return type for external calls in MVP
        }
    }
}