using Sage.Ast;
using Sage.Enums;
using Sage.Interfaces;
using Sage.Utilities;

namespace Sage.Core
{
    /// <summary>
    /// Performs static semantic analysis on the Abstract Syntax Tree (AST).
    /// Responsible for type checking, scope management, and identifier resolution.
    /// </summary>
    public class SemanticAnalyzer : IAstVisitor<string>
    {
        private readonly SymbolTable _symbolTable = new();

        /// <summary>
        /// Orchestrates the semantic analysis of the entire program.
        /// </summary>
        /// <param name="program">The root node of the AST to analyze.</param>
        public void Analyze(ProgramNode program) => program.Accept(this);

        /// <summary>
        /// Checks if a Sage type is considered numeric for arithmetic operations.
        /// </summary>
        /// <param name="type">The type name to check.</param>
        /// <returns>True if the type is a signed/unsigned integer or floating point.</returns>
        private static bool IsNumeric(string type) =>
            type is "i8" or "u8" or "i16" or "u16" or "i32" or "u32" or "i64" or "u64" or "f32" or "f64";

        public string Visit(ProgramNode node)
        {
            foreach (var statement in node.Statements) statement.Accept(this);
            return "none";
        }

        public string Visit(ModuleNode node)
        {
            foreach (var function in node.Functions) function.Accept(this);
            return "none";
        }

        /// <summary>
        /// Handles function-level scoping. Parameters are registered in a new local scope.
        /// </summary>
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
            foreach (var statement in node.Statements) statement.Accept(this);
            return "none";
        }

        /// <summary>
        /// Validates variable/constant declarations. Enforces explicit initialization and type compatibility.
        /// </summary>
        public string Visit(VariableDeclarationNode node)
        {
            string initType = node.Initializer.Accept(this);

            if (node.Type != initType)
            {
                // Implicit promotion rule: i32 can be assigned to f64
                if (node.Type == "f64" && initType == "i32") { /* Allowed */ }
                else
                {
                    throw new CompilerException(null, "S101",
                        $"Type Mismatch: Cannot assign {initType} to {node.Name} of type {node.Type}.");
                }
            }

            _symbolTable.Define(node.Name, node.Type);
            return node.Type;
        }

        /// <summary>
        /// Validates variable assignments. Ensures target exists and types are compatible.
        /// </summary>
        public string Visit(AssignmentNode node)
        {
            string? varType = _symbolTable.Resolve(node.Name);
            if (varType == null)
                throw new CompilerException(null, "S105", $"Usage Error: Variable '{node.Name}' not declared.");

            string exprType = node.Expression.Accept(this);

            if (varType != exprType)
            {
                if (varType == "f64" && exprType == "i32") return "f64";
                throw new CompilerException(null, "S108", $"Type Mismatch: Cannot assign {exprType} to {varType}.");
            }
            return varType;
        }

        public string Visit(IfNode node)
        {
            if (node.Condition.Accept(this) != "b8")
                throw new CompilerException(null, "S102", "Control Flow Error: 'if' condition must be b8 (boolean).");

            node.ThenBranch.Accept(this);
            node.ElseBranch?.Accept(this);
            return "none";
        }

        public string Visit(WhileNode node)
        {
            if (node.Condition.Accept(this) != "b8")
                throw new CompilerException(null, "S103", "Control Flow Error: 'while' condition must be b8 (boolean).");

            node.Body.Accept(this);
            return "none";
        }

        /// <summary>
        /// Handles 'for' loops with an isolated scope for the initializer variable.
        /// </summary>
        public string Visit(ForNode node)
        {
            _symbolTable.EnterScope();

            node.Initializer?.Accept(this);
            if (node.Condition != null && node.Condition.Accept(this) != "b8")
                throw new CompilerException(null, "S104", "Control Flow Error: 'for' condition must be b8 (boolean).");

            node.Increment?.Accept(this);
            node.Body.Accept(this);

            _symbolTable.ExitScope();
            return "none";
        }

        public string Visit(IdentifierNode node)
        {
            // Simple handling for external namespaces for now
            if (node.Name.Contains("::")) return "f64";

            string? type = _symbolTable.Resolve(node.Name);
            if (type == null)
                throw new CompilerException(null, "S105", $"Usage Error: Identifier '{node.Name}' not found in current scope.");

            return type;
        }

        /// <summary>
        /// Validates binary operations, including arithmetic promotion and boolean logic results.
        /// </summary>
        public string Visit(BinaryExpressionNode node)
        {
            string lhs = node.Left.Accept(this);
            string rhs = node.Right.Accept(this);

            // Comparison Operators (Result is always boolean b8)
            if (node.Operator is TokenType.EqualEqual or TokenType.NotEqual or TokenType.Less or
                TokenType.LessEqual or TokenType.Greater or TokenType.GreaterEqual)
            {
                if (lhs != rhs && !(IsNumeric(lhs) && IsNumeric(rhs)))
                    throw new CompilerException(null, "S106", $"Type Mismatch: Cannot compare {lhs} with {rhs}.");
                return "b8";
            }

            // Logical Operators
            if (node.Operator is TokenType.AmpersandAmpersand or TokenType.PipePipe)
            {
                if (lhs != "b8" || rhs != "b8")
                    throw new CompilerException(null, "S107", "Type Mismatch: Logical operators require b8 operands.");
                return "b8";
            }

            // Arithmetic Operators
            if (lhs != rhs)
            {
                if ((lhs == "f64" && rhs == "i32") || (lhs == "i32" && rhs == "f64")) return "f64";
                throw new CompilerException(null, "S108", $"Type Mismatch: Arithmetic operation between {lhs} and {rhs} is invalid.");
            }

            return lhs;
        }

        public string Visit(UnaryExpressionNode node)
        {
            string type = node.Operand.Accept(this);

            if (node.Operator == TokenType.Bang && type != "b8")
                throw new CompilerException(null, "S109", "Type Mismatch: '!' operator requires b8 operand.");

            if (node.Operator == TokenType.PlusPlus && !IsNumeric(type))
                throw new CompilerException(null, "S110", "Type Mismatch: Increment '++' requires numeric operand.");

            return type;
        }

        public string Visit(FunctionCallNode node)
        {
            foreach (var arg in node.Arguments) arg.Accept(this);
            return "f64"; // Default return type for external/unresolved calls
        }

        public string Visit(LiteralNode node) => node.TypeName;
        public string Visit(ReturnNode node) => node.Expression.Accept(this);
        public string Visit(ExpressionStatementNode node) => node.Expression.Accept(this);
        public string Visit(UseNode node) => "none";
        public string Visit(InterpolatedStringNode node) => "str";
        public string Visit(CastExpressionNode node) => node.TargetType;
    }
}