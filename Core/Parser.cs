using Sage.Ast;
using Sage.Enums;
using Sage.Interfaces;
using Sage.Utilities;

namespace Sage.Core
{
    /// <summary>
    /// Performs syntax analysis (parsing) on a stream of Sage tokens.
    /// Implements a Recursive Descent Parser to construct an Abstract Syntax Tree (AST).
    /// </summary>
    /// <param name="tokens">The list of tokens produced by the Lexer.</param>
    /// <param name="fileName">The name of the source file being parsed (for error reporting).</param>
    public class Parser(List<Token> tokens, string fileName) : IParser
    {
        private readonly List<Token> _tokens = tokens;
        private readonly string _fileName = fileName;
        private int _pos = 0;

        /// <summary>Gets the current token in the stream without consuming it.</summary>
        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : _tokens[^1];

        /// <summary>
        /// Helper method to initialize an AST node with its source code location.
        /// </summary>
        private static T CreateNode<T>(T node, Token token) where T : AstNode
        {
            node.Line = token.Line;
            node.Column = token.Column;
            return node;
        }

        /// <summary>
        /// Ensures the current token matches the expected type and consumes it.
        /// </summary>
        /// <exception cref="CompilerException">Thrown if the current token does not match the expected type.</exception>
        private Token Consume(TokenType type, string errorMessage = "")
        {
            if (Current.Type == type)
            {
                var t = Current;
                _pos++;
                return t;
            }

            string msg = string.IsNullOrEmpty(errorMessage)
                ? $"Expected {type} but found {Current.Type} ('{Current.Value}')"
                : errorMessage;

            throw new CompilerException(Current, "S001", msg);
        }

        /// <summary>
        /// Checks if the current token matches the given type. If so, consumes it and returns true.
        /// </summary>
        private bool Match(TokenType type)
        {
            if (Current.Type == type)
            {
                _pos++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Entry point of the parser. Processes the token stream into a <see cref="ProgramNode"/>.
        /// </summary>
        public ProgramNode Parse()
        {
            var program = new ProgramNode { Line = 1, Column = 1 };
            while (Current.Type != TokenType.EndOfFile)
            {
                if (Current.Type == TokenType.Keyword_Module)
                    program.Statements.Add(ParseModule());
                else
                    program.Statements.Add(ParseStatement());
            }
            return program;
        }

        /// <summary>Parses a module definition and its internal functions.</summary>
        private ModuleNode ParseModule()
        {
            var startToken = Current;
            Consume(TokenType.Keyword_Module);
            string name = Consume(TokenType.Identifier).Value;
            Consume(TokenType.OpenBrace);

            var module = CreateNode(new ModuleNode(name), startToken);
            while (Current.Type != TokenType.CloseBrace && Current.Type != TokenType.EndOfFile)
            {
                if (Current.Type == TokenType.Keyword_Func || Current.Type == TokenType.Keyword_Extern)
                    module.Functions.Add(ParseFunction(module.Name));
                else
                    throw new CompilerException(Current, "S002", "Only functions are supported in modules.");
            }

            Consume(TokenType.CloseBrace);
            return module;
        }

        /// <summary>Parses a single statement or declaration.</summary>
        private AstNode ParseStatement()
        {
            var startToken = Current;

            // Import directive: use module;
            if (Match(TokenType.Keyword_Use))
            {
                string name = Consume(TokenType.Identifier).Value;
                Consume(TokenType.Semicolon);
                return CreateNode(new UseNode(name), startToken);
            }

            if (Current.Type == TokenType.Keyword_Func) return ParseFunction("");

            if (Match(TokenType.Keyword_Return))
            {
                var expr = ParseExpression();
                Consume(TokenType.Semicolon);
                return CreateNode(new ReturnNode(expr), startToken);
            }

            if (Current.Type == TokenType.Keyword_Var || Current.Type == TokenType.Keyword_Const)
                return ParseVariableDeclaration();

            if (Current.Type == TokenType.Keyword_If) return ParseIfStatement();
            if (Current.Type == TokenType.Keyword_While) return ParseWhileStatement();
            if (Current.Type == TokenType.Keyword_For) return ParseForStatement();

            // Fallback: Expression Statement
            var exprStmt = ParseExpression();
            Consume(TokenType.Semicolon);
            return CreateNode(new ExpressionStatementNode(exprStmt), startToken);
        }

        /// <summary>Parses variable (var) or constant (const) declarations.</summary>
        private VariableDeclarationNode ParseVariableDeclaration()
        {
            var startToken = Current;
            bool isConst = Match(TokenType.Keyword_Const);
            if (!isConst) Consume(TokenType.Keyword_Var);

            string name = Consume(TokenType.Identifier).Value;
            Consume(TokenType.Colon);
            string type = ConsumeType();

            Consume(TokenType.Equals, "Variables and constants must be initialized explicitly in Sage.");
            var init = ParseExpression();
            Consume(TokenType.Semicolon);

            return CreateNode(new VariableDeclarationNode(type, name, init, isConst), startToken);
        }

        /// <summary>Parses if-else conditional branches.</summary>
        private IfNode ParseIfStatement()
        {
            var startToken = Current;
            Consume(TokenType.Keyword_If);
            Consume(TokenType.OpenParen);
            var condition = ParseExpression();
            Consume(TokenType.CloseParen);
            var thenBranch = ParseBlock();

            BlockNode? elseBranch = null;
            if (Match(TokenType.Keyword_Else))
                elseBranch = ParseBlock();

            return CreateNode(new IfNode(condition, thenBranch, elseBranch), startToken);
        }

        /// <summary>Parses while loop structures.</summary>
        private WhileNode ParseWhileStatement()
        {
            var startToken = Current;
            Consume(TokenType.Keyword_While);
            Consume(TokenType.OpenParen);
            var condition = ParseExpression();
            Consume(TokenType.CloseParen);
            var body = ParseBlock();
            return CreateNode(new WhileNode(condition, body), startToken);
        }

        /// <summary>Parses C-style for loops.</summary>
        private ForNode ParseForStatement()
        {
            var startToken = Current;
            Consume(TokenType.Keyword_For);
            Consume(TokenType.OpenParen);

            AstNode? initializer = null;
            if (Current.Type != TokenType.Semicolon)
            {
                if (Current.Type == TokenType.Keyword_Var)
                    initializer = ParseVariableDeclaration();
                else
                {
                    initializer = ParseExpression();
                    Consume(TokenType.Semicolon);
                }
            }
            else Consume(TokenType.Semicolon);

            AstNode? condition = null;
            if (Current.Type != TokenType.Semicolon)
                condition = ParseExpression();
            Consume(TokenType.Semicolon);

            AstNode? increment = null;
            if (Current.Type != TokenType.CloseParen)
                increment = ParseExpression();
            Consume(TokenType.CloseParen);

            return CreateNode(new ForNode(initializer, condition, increment, ParseBlock()), startToken);
        }

        /// <summary>Parses function declarations, including external C interop functions.</summary>
        private FunctionDeclarationNode ParseFunction(string moduleOwner)
        {
            var startToken = Current;
            bool isExtern = Match(TokenType.Keyword_Extern);
            Consume(TokenType.Keyword_Func);

            string name = Consume(TokenType.Identifier).Value;
            Consume(TokenType.OpenParen);

            var parameters = new List<ParameterNode>();
            if (Current.Type != TokenType.CloseParen)
            {
                do
                {
                    string pName = Consume(TokenType.Identifier).Value;
                    Consume(TokenType.Colon);
                    string pType = ConsumeType();
                    parameters.Add(new ParameterNode(pName, pType));
                } while (Match(TokenType.Comma));
            }
            Consume(TokenType.CloseParen);
            Consume(TokenType.Colon);
            string retType = ConsumeType();

            if (isExtern)
            {
                Consume(TokenType.Semicolon);
                return CreateNode(new FunctionDeclarationNode(name, retType, parameters, null!, moduleOwner) { IsExtern = true }, startToken);
            }

            return CreateNode(new FunctionDeclarationNode(name, retType, parameters, ParseBlock(), moduleOwner), startToken);
        }

        /// <summary>Parses a scoped block of code enclosed in braces.</summary>
        private BlockNode ParseBlock()
        {
            var startToken = Current;
            Consume(TokenType.OpenBrace);
            var block = CreateNode(new BlockNode(), startToken);
            while (Current.Type != TokenType.CloseBrace && Current.Type != TokenType.EndOfFile)
                block.Statements.Add(ParseStatement());
            Consume(TokenType.CloseBrace);
            return block;
        }

        /// <summary>Helper to consume and return a valid Sage data type.</summary>
        private string ConsumeType()
        {
            if (Current.Type >= TokenType.Type_I8 && Current.Type <= TokenType.Type_Void)
            {
                string v = Current.Value;
                _pos++;
                return v;
            }
            throw new CompilerException(Current, "S003", $"Expected Type, found {Current.Type}");
        }

        // --- Expression Hierarchy (Operator Precedence) ---

        private AstNode ParseExpression() => ParseAssignment();

        private AstNode ParseAssignment()
        {
            var expr = ParseLogicalOr();
            if (Match(TokenType.Equals))
            {
                var opToken = Current;
                var value = ParseAssignment();
                if (expr is IdentifierNode id)
                    return CreateNode(new AssignmentNode(id.Name, value), opToken);

                throw new CompilerException(Current, "S005", "Invalid assignment target.");
            }
            return expr;
        }

        private AstNode ParseLogicalOr()
        {
            var left = ParseLogicalAnd();
            while (Current.Type == TokenType.PipePipe)
            {
                var opToken = Current; _pos++;
                left = CreateNode(new BinaryExpressionNode(left, TokenType.PipePipe, ParseLogicalAnd()), opToken);
            }
            return left;
        }

        private AstNode ParseLogicalAnd()
        {
            var left = ParseEquality();
            while (Current.Type == TokenType.AmpersandAmpersand)
            {
                var opToken = Current; _pos++;
                left = CreateNode(new BinaryExpressionNode(left, TokenType.AmpersandAmpersand, ParseEquality()), opToken);
            }
            return left;
        }

        private AstNode ParseEquality()
        {
            var left = ParseRelational();
            while (Current.Type == TokenType.EqualEqual || Current.Type == TokenType.NotEqual)
            {
                var opToken = Current;
                var op = Current.Type; _pos++;
                left = CreateNode(new BinaryExpressionNode(left, op, ParseRelational()), opToken);
            }
            return left;
        }

        private AstNode ParseRelational()
        {
            var left = ParseAdditive();
            while (Current.Type == TokenType.Less || Current.Type == TokenType.LessEqual ||
                   Current.Type == TokenType.Greater || Current.Type == TokenType.GreaterEqual)
            {
                var opToken = Current;
                var op = Current.Type; _pos++;
                left = CreateNode(new BinaryExpressionNode(left, op, ParseAdditive()), opToken);
            }
            return left;
        }

        private AstNode ParseAdditive()
        {
            var left = ParseMultiplicative();
            while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
            {
                var opToken = Current;
                var op = Current.Type; _pos++;
                left = CreateNode(new BinaryExpressionNode(left, op, ParseMultiplicative()), opToken);
            }
            return left;
        }

        private AstNode ParseMultiplicative()
        {
            var left = ParseUnary();
            while (Current.Type == TokenType.Asterisk || Current.Type == TokenType.Slash || Current.Type == TokenType.Percent)
            {
                var opToken = Current;
                var op = Current.Type; _pos++;
                left = CreateNode(new BinaryExpressionNode(left, op, ParseUnary()), opToken);
            }
            return left;
        }

        private AstNode ParseUnary()
        {
            if (Current.Type == TokenType.Bang || Current.Type == TokenType.Minus)
            {
                var opToken = Current;
                var op = Current.Type; _pos++;
                return CreateNode(new UnaryExpressionNode(op, ParseUnary()), opToken);
            }
            return ParsePostfix();
        }

        private AstNode ParsePostfix()
        {
            var left = ParseCastExpression();
            if (Current.Type == TokenType.PlusPlus)
            {
                var opToken = Current; _pos++;
                return CreateNode(new UnaryExpressionNode(TokenType.PlusPlus, left, isPostfix: true), opToken);
            }
            return left;
        }

        private AstNode ParseCastExpression()
        {
            var expr = ParsePrimary();
            while (Current.Type == TokenType.Keyword_As)
            {
                var opToken = Current; _pos++;
                expr = CreateNode(new CastExpressionNode(expr, ConsumeType()), opToken);
            }
            return expr;
        }

        /// <summary>Parses primary expressions: literals, identifiers, and parenthesized expressions.</summary>
        private AstNode ParsePrimary()
        {
            var startToken = Current;

            if (Match(TokenType.Keyword_True)) return CreateNode(new LiteralNode(true, "b8"), startToken);
            if (Match(TokenType.Keyword_False)) return CreateNode(new LiteralNode(false, "b8"), startToken);
            if (Current.Type == TokenType.Integer) return CreateNode(new LiteralNode(Consume(TokenType.Integer).Value, "i32"), startToken);
            if (Current.Type == TokenType.Float) return CreateNode(new LiteralNode(Consume(TokenType.Float).Value, "f64"), startToken);
            if (Current.Type == TokenType.String) return CreateNode(new LiteralNode(Consume(TokenType.String).Value, "string"), startToken);

            if (Current.Type == TokenType.Identifier)
            {
                string name = Consume(TokenType.Identifier).Value;
                while (Match(TokenType.DoubleColon)) name += "::" + Consume(TokenType.Identifier).Value;

                if (Match(TokenType.OpenParen))
                {
                    var args = new List<AstNode>();
                    if (Current.Type != TokenType.CloseParen)
                    {
                        do { args.Add(ParseExpression()); } while (Match(TokenType.Comma));
                    }
                    Consume(TokenType.CloseParen);
                    return CreateNode(new FunctionCallNode(name, args), startToken);
                }
                return CreateNode(new IdentifierNode(name), startToken);
            }

            if (Match(TokenType.OpenParen))
            {
                var expr = ParseExpression();
                Consume(TokenType.CloseParen);
                return expr;
            }

            throw new CompilerException(Current, "S004", $"Unexpected token: {Current.Type}");
        }
    }
}