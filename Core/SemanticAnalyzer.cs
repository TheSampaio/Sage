using Sage.Ast;
using Sage.Enums;
using Sage.Interfaces;
using Sage.Utilities;

namespace Sage.Core
{
    public class SemanticAnalyzer(SymbolTable? sharedTable = null) : IAstVisitor<string>
    {
        private readonly SymbolTable _symbolTable = sharedTable ?? new SymbolTable();

        public void Analyze(ProgramNode ast) => ast.Accept(this);

        /// <summary>
        /// Pre-registers module-level symbols to allow forward references and out-of-order calls.
        /// </summary>
        public void RegisterModuleSymbols(ProgramNode moduleAst)
        {
            foreach (var node in moduleAst.Statements.OfType<ModuleNode>())
            {
                foreach (var member in node.Members.OfType<FunctionDeclarationNode>())
                {
                    // Ensure the AST node knows its owner for later phases
                    member.ModuleOwner = node.Name;
                    RegisterFunction(member, node.Name);
                }
            }
        }

        private void RegisterFunction(FunctionDeclarationNode func, string moduleName = "")
        {
            // 1. Define the bare name (e.g., "print_line") for use inside the module itself
            _symbolTable.Define(func.Name, func.ReturnType, isFunction: true, isExtern: func.IsExtern);

            // 2. Define the fully qualified name (e.g., "console::print_line") for external use
            if (!string.IsNullOrEmpty(moduleName))
            {
                _symbolTable.Define($"{moduleName}::{func.Name}", func.ReturnType, isFunction: true, isExtern: func.IsExtern);
            }
        }

        public string Visit(ProgramNode node)
        {
            foreach (var stmt in node.Statements) stmt.Accept(this);
            return "none";
        }

        public string Visit(ModuleNode node)
        {
            // Internal registration to support recursion within the module
            foreach (var member in node.Members.OfType<FunctionDeclarationNode>())
            {
                member.ModuleOwner = node.Name; // IMPORTANT: Tag the node with its module
                RegisterFunction(member, node.Name);
            }

            foreach (var member in node.Members) member.Accept(this);
            return "none";
        }

        public string Visit(FunctionDeclarationNode node)
        {
            // If the function wasn't caught by module pre-registration, register it now
            if (!_symbolTable.IsDefinedInCurrentScope(node.Name))
            {
                RegisterFunction(node, node.ModuleOwner);
            }

            _symbolTable.EnterScope();

            // Register parameters
            foreach (var param in node.Parameters)
            {
                _symbolTable.Define(param.Name, param.Type);
            }

            // Analyze body
            if (!node.IsExtern && node.Body != null)
            {
                node.Body.Accept(this);
            }

            _symbolTable.ExitScope();
            return node.ReturnType;
        }

        public string Visit(VariableDeclarationNode node)
        {
            if (_symbolTable.IsDefinedInCurrentScope(node.Name))
                throw new CompilerException(node, "S102", $"Variable '{node.Name}' is already defined.");

            if (node.Initializer != null)
            {
                string initType = node.Initializer.Accept(this);

                // Allow struct initializations to pass type checking for now
                if (initType != "struct_initializer")
                {
                    if (!TypeSystem.AreTypesCompatible(node.Type, initType) &&
                        !IsAutoPromotableLiteral(node.Type, node.Initializer))
                    {
                        throw new CompilerException(node, "S101", $"Type Mismatch: Cannot assign {initType} to {node.Name} (expected {node.Type}).");
                    }
                }

                if (node.Initializer is LiteralNode literal) literal.TypeName = node.Type;
            }

            _symbolTable.Define(node.Name, node.Type);
            return node.Type;
        }

        public string Visit(BinaryExpressionNode node)
        {
            string lhs = node.Left.Accept(this);
            string rhs = node.Right.Accept(this);

            if (IsComparisonOp(node.Operator)) return "b8";

            if (IsLogicalOp(node.Operator))
            {
                if (lhs != "b8" || rhs != "b8")
                    throw new CompilerException(node, "S107", "Logical operators require b8.");
                return "b8";
            }

            string? dominantType = TypeSystem.GetDominantType(lhs, rhs);
            return dominantType ?? throw new CompilerException(node, "S108", $"Invalid arithmetic: {lhs} and {rhs}");
        }

        public string Visit(FunctionCallNode node)
        {
            var symbol = _symbolTable.Resolve(node.Name)
                ?? throw new CompilerException(node, "S105", $"Function '{node.Name}' not found.");

            node.IsExternCall = symbol.IsExtern;

            foreach (var arg in node.Arguments) arg.Accept(this);

            return symbol.Type;
        }

        // --- Helpers ---

        private static bool IsComparisonOp(TokenType op) =>
            op is TokenType.EqualEqual or TokenType.NotEqual or TokenType.Less or TokenType.LessEqual or TokenType.Greater or TokenType.GreaterEqual;

        private static bool IsLogicalOp(TokenType op) =>
            op is TokenType.AmpersandAmpersand or TokenType.PipePipe;

        // Syntactic Sugar: Allows assigning "10" (int) to an f32 variable without explicit casting
        private bool IsAutoPromotableLiteral(string targetType, AstNode initializer)
        {
            if (initializer is not LiteralNode) return false;
            if (TypeSystem.IsFloatingPoint(targetType) && TypeSystem.IsNumeric(initializer.Accept(this))) return true;
            return false;
        }

        // --- Standard Visitor Implementations ---

        public string Visit(IdentifierNode node)
        {
            var symbol = _symbolTable.Resolve(node.Name)
                ?? throw new CompilerException(node, "S105", $"Identifier '{node.Name}' not declared.");
            node.VariableType = symbol.Type;
            return symbol.Type;
        }

        public string Visit(BlockNode node)
        {
            _symbolTable.EnterScope();
            foreach (var stmt in node.Statements) stmt.Accept(this);
            _symbolTable.ExitScope();
            return "none";
        }

        public string Visit(AssignmentNode node)
        {
            var symbol = _symbolTable.Resolve(node.Name)
                ?? throw new CompilerException(node, "S105", $"Variable '{node.Name}' not declared.");
            node.VariableType = symbol.Type;

            if (!TypeSystem.AreTypesCompatible(symbol.Type, node.Expression.Accept(this)))
                throw new CompilerException(node, "S108", $"Cannot assign {node.Expression.Accept(this)} to {symbol.Type}.");

            return symbol.Type;
        }

        public string Visit(ExternBlockNode node)
        {
            // Scoped extern definitions (private to the module/block where they are defined)
            foreach (var decl in node.Declarations.OfType<FunctionDeclarationNode>())
            {
                // Extern functions in modules are usually static/namespaced
                _symbolTable.Define($"{node.Alias}::{decl.Name}", decl.ReturnType, isFunction: true, isExtern: true);
            }
            return "none";
        }

        public string Visit(IfNode node) { CheckCondition(node.Condition); node.ThenBranch.Accept(this); node.ElseBranch?.Accept(this); return "none"; }
        public string Visit(WhileNode node) { CheckCondition(node.Condition); node.Body.Accept(this); return "none"; }

        public string Visit(ForNode node)
        {
            _symbolTable.EnterScope();
            node.Initializer?.Accept(this);
            if (node.Condition != null) CheckCondition(node.Condition);
            node.Increment?.Accept(this);
            node.Body.Accept(this);
            _symbolTable.ExitScope();
            return "none";
        }

        private void CheckCondition(AstNode condition)
        {
            if (condition.Accept(this) != "b8")
                throw new CompilerException(condition, "S104", "Condition must be b8.");
        }

        public string Visit(LiteralNode node) => node.TypeName;
        public string Visit(ReturnNode node) => node.Expression.Accept(this);
        public string Visit(ExpressionStatementNode node) => node.Expression.Accept(this);
        public string Visit(UnaryExpressionNode node) => node.Operand.Accept(this);
        public string Visit(CastExpressionNode node) => node.TargetType;
        public string Visit(UseNode node) => "none";

        public string Visit(InterpolatedStringNode node)
        {
            foreach (var part in node.Parts)
            {
                part.VariableType = part.Accept(this);
            }
            return "string";
        }

        public string Visit(StructDeclarationNode node)
        {
            _symbolTable.Define(node.Name, "struct");
            foreach (var field in node.Fields) field.Accept(this);
            return "none";
        }

        public string Visit(StructInitializationNode node)
        {
            // Visit all fields to ensure inner expressions are validated
            foreach (var value in node.Fields.Values) value.Accept(this);

            // Return a special internal type marker
            return "struct_initializer";
        }
    }
}