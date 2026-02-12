using Sage.Core.AST;
using Sage.Enums;

namespace Sage.Core
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        /// <summary>
        /// Gets the current token without consuming it.
        /// </summary>
        private Token Current => _position < _tokens.Count ? _tokens[_position] : _tokens[_tokens.Count - 1];

        /// <summary>
        /// Checks if the current token matches the expected type.
        /// If yes, consumes it and returns true.
        /// </summary>
        private bool Match(TokenType type)
        {
            if (Current.Type == type)
            {
                _position++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Consumes the current token if it matches the type.
        /// Throws an exception if it doesn't match.
        /// </summary>
        private Token Expect(TokenType type)
        {
            if (Current.Type == type)
            {
                var token = Current;
                _position++;
                return token;
            }

            // Simple error handling for now. In the future, we should collect errors instead of crashing.
            throw new Exception($"[PARSER ERROR] Expected {type} but found {Current.Type} ('{Current.Value}') at line {Current.Line}");
        }

        // =========================================================================================
        // Parsing Methods
        // =========================================================================================

        public ProgramNode Parse()
        {
            var program = new ProgramNode();

            // Loop until we reach the end of the file
            while (Current.Type != TokenType.EndOfFile)
            {
                program.Statements.Add(ParseStatement());
            }

            return program;
        }

        private StatementNode ParseStatement()
        {
            // Case: use Console;
            if (Match(TokenType.Keyword_Use))
            {
                string moduleName = Expect(TokenType.Identifier).Value;
                Expect(TokenType.Semicolon);
                return new UseNode(moduleName);
            }

            // Case: func Main() -> i32 { ... }
            if (Match(TokenType.Keyword_Func))
            {
                return ParseFunctionDeclaration();
            }

            // Case: return 0;
            if (Match(TokenType.Keyword_Return))
            {
                ExpressionNode expr = ParseExpression();
                Expect(TokenType.Semicolon);
                return new ReturnNode(expr);
            }

            // Case: Variable Declaration (e.g., i32 number = 5;)
            // We check if the current token is a type keyword
            if (IsType(Current.Type))
            {
                return ParseVariableDeclaration();
            }

            // Case: Expression Statement (e.g., Function calls like Console::Print(...);)
            // If it's none of the above, it must be an expression acting as a statement
            ExpressionNode expression = ParseExpression();
            Expect(TokenType.Semicolon); // Statements must end with semicolon
            return new ExpressionStatementNode(expression); // In a robust AST, we would wrap this in an ExpressionStatementNode
        }

        private FunctionDeclarationNode ParseFunctionDeclaration()
        {
            // func Name
            string name = Expect(TokenType.Identifier).Value;

            // ( ) -> Parameters (TODO: Implement parameters parsing)
            Expect(TokenType.OpenParen);
            Expect(TokenType.CloseParen);

            // -> ReturnType
            Expect(TokenType.Arrow);
            string returnType = Expect(TokenType.Type_I32).Value; // Currently supporting i32 only for simplicity

            // { Body }
            BlockNode body = ParseBlock();

            return new FunctionDeclarationNode(name, returnType, body);
        }

        private VariableDeclarationNode ParseVariableDeclaration()
        {
            // i32 name = value;
            string type = Current.Value;
            _position++; // Consume type

            string name = Expect(TokenType.Identifier).Value;
            Expect(TokenType.Equals);

            ExpressionNode initializer = ParseExpression();
            Expect(TokenType.Semicolon);

            return new VariableDeclarationNode(type, name, initializer);
        }

        private BlockNode ParseBlock()
        {
            Expect(TokenType.OpenBrace);
            var block = new BlockNode();

            while (Current.Type != TokenType.CloseBrace && Current.Type != TokenType.EndOfFile)
            {
                block.Statements.Add(ParseStatement());
            }

            Expect(TokenType.CloseBrace);
            return block;
        }

        // =========================================================================================
        // Expression Parsing (Recursive Descent for Operator Precedence)
        // =========================================================================================

        private ExpressionNode ParseExpression()
        {
            return ParseAdditive();
        }

        // Handles + and -
        private ExpressionNode ParseAdditive()
        {
            ExpressionNode left = ParseMultiplicative();

            while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
            {
                TokenType op = Current.Type;
                _position++; // Consume operator
                ExpressionNode right = ParseMultiplicative();
                left = new BinaryExpressionNode(left, op, right);
            }

            return left;
        }

        // Handles * and /
        private ExpressionNode ParseMultiplicative()
        {
            ExpressionNode left = ParsePrimary();

            while (Current.Type == TokenType.Asterisk || Current.Type == TokenType.Slash)
            {
                TokenType op = Current.Type;
                _position++; // Consume operator
                ExpressionNode right = ParsePrimary();
                left = new BinaryExpressionNode(left, op, right);
            }

            return left;
        }

        // Handles literals, identifiers, parentheses, and function calls
        private ExpressionNode ParsePrimary()
        {
            // 1. Case: (Expression) - Handled at the top for general grouping
            if (Match(TokenType.OpenParen))
            {
                ExpressionNode expr = ParseExpression();
                Expect(TokenType.CloseParen);
                // We wrap it in a ParenthesizedExpressionNode so the CodeGenerator knows 
                // it must print the actual parentheses in C++.
                return new ParenthesizedExpressionNode(expr);
            }

            // 2. Case: Numbers
            if (Match(TokenType.Number))
            {
                return new LiteralNode(_tokens[_position - 1].Value, "i32");
            }

            // 3. Case: Strings & Interpolation
            if (Match(TokenType.String))
            {
                string fullValue = _tokens[_position - 1].Value;

                if (!fullValue.Contains("{"))
                {
                    return new LiteralNode(fullValue, "string");
                }

                var interpolated = new InterpolatedStringNode();
                int cursor = 0;

                while (cursor < fullValue.Length)
                {
                    int openBrace = fullValue.IndexOf('{', cursor);
                    if (openBrace == -1)
                    {
                        interpolated.Parts.Add(new LiteralNode(fullValue.Substring(cursor), "string"));
                        break;
                    }

                    if (openBrace > cursor)
                    {
                        interpolated.Parts.Add(new LiteralNode(fullValue.Substring(cursor, openBrace - cursor), "string"));
                    }

                    int closeBrace = fullValue.IndexOf('}', openBrace);
                    if (closeBrace == -1) throw new Exception("Mismatched '{' in interpolated string.");

                    string expressionText = fullValue.Substring(openBrace + 1, closeBrace - (openBrace + 1));

                    Lexer tempLexer = new Lexer(expressionText);
                    List<Token> tempTokens = tempLexer.Tokenize();
                    tempTokens.RemoveAll(t => t.Type == TokenType.EndOfFile);
                    tempTokens.Add(new Token(TokenType.EndOfFile, "\0", 0, 0));

                    Parser tempParser = new Parser(tempTokens);
                    interpolated.Parts.Add(tempParser.ParseExpression());

                    cursor = closeBrace + 1;
                }

                return interpolated;
            }

            // 4. Case: Identifier (Variable or Function Call)
            if (Current.Type == TokenType.Identifier)
            {
                string name = Current.Value;
                _position++;

                // Handle Namespace (Console::Print)
                if (Match(TokenType.DoubleColon))
                {
                    string memberName = Expect(TokenType.Identifier).Value;
                    name += "::" + memberName;
                }

                // Handle Function Call: Name(...)
                if (Match(TokenType.OpenParen))
                {
                    var args = new List<ExpressionNode>();

                    if (Current.Type != TokenType.CloseParen)
                    {
                        do
                        {
                            args.Add(ParseExpression());
                        } while (Match(TokenType.Comma));
                    }

                    Expect(TokenType.CloseParen);
                    return new FunctionCallNode(name, args);
                }

                // Just a variable usage
                return new IdentifierNode(name);
            }

            throw new Exception($"[PARSER ERROR] Unexpected token for expression: {Current.Type} ('{Current.Value}') at line {Current.Line}");
        }

        private bool IsType(TokenType type)
        {
            return type >= TokenType.Type_U8 && type <= TokenType.Type_Void;
        }
    }
}