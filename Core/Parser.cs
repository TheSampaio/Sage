using Sage.Ast;
using Sage.Enums;
using Sage.Interfaces;

namespace Sage.Core
{
    /// <summary>
    /// Implements a recursive descent parser to convert a stream of tokens into an Abstract Syntax Tree (AST).
    /// </summary>
    public class Parser(List<Token> tokens) : IParser
    {
        private readonly List<Token> _tokens = tokens;
        private int _pos = 0;

        private Token Current => _tokens[_pos];

        /// <summary>
        /// Provides a lookahead to the next token without consuming the current one.
        /// </summary>
        private Token Peek => (_pos + 1 < _tokens.Count) ? _tokens[_pos + 1] : _tokens[^1];

        /// <summary>
        /// Consumes a token of a specific type or throws a syntax error.
        /// </summary>
        private Token Consume(TokenType type)
        {
            if (Current.Type == type)
            {
                var t = Current;
                _pos++;
                return t;
            }
            throw new Exception($"[PARSER ERROR] Expected {type} but got {Current.Type} ('{Current.Value}') at line {Current.Line}");
        }

        /// <summary>
        /// Checks if the current token matches a type and advances if true.
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
        /// Main entry point for parsing the token stream into a ProgramNode.
        /// </summary>
        public ProgramNode Parse()
        {
            var program = new ProgramNode();
            while (Current.Type != TokenType.EndOfFile)
            {
                if (Current.Type == TokenType.Keyword_Module)
                    program.Statements.Add(ParseModule());
                else
                    program.Statements.Add(ParseStatement());
            }
            return program;
        }

        private ModuleNode ParseModule()
        {
            Consume(TokenType.Keyword_Module);
            string name = Consume(TokenType.Identifier).Value;
            Consume(TokenType.OpenBrace);

            var module = new ModuleNode(name);
            while (Current.Type != TokenType.CloseBrace && Current.Type != TokenType.EndOfFile)
            {
                if (Current.Type == TokenType.Keyword_Func)
                    module.Functions.Add(ParseFunction(module.Name));
                else
                    throw new Exception("Only functions are currently supported within modules.");
            }

            Consume(TokenType.CloseBrace);
            return module;
        }

        /// <summary>
        /// Parses a single statement, branching between keywords, declarations, and expressions.
        /// </summary>
        private AstNode ParseStatement()
        {
            if (Match(TokenType.Keyword_Use))
            {
                string name = Consume(TokenType.Identifier).Value;
                Consume(TokenType.Semicolon);
                return new UseNode(name);
            }

            if (Current.Type == TokenType.Keyword_Func)
                return ParseFunction("");

            if (Match(TokenType.Keyword_Return))
            {
                var expr = ParseExpression();
                Consume(TokenType.Semicolon);
                return new ReturnNode(expr);
            }

            // Variable Declaration check: var/const name: type = value;
            if (Current.Type == TokenType.Keyword_Var || Current.Type == TokenType.Keyword_Const)
            {
                return ParseVariableDeclaration();
            }

            // Fallback to general expression (e.g., function calls or assignments)
            var exprStmt = ParseExpression();
            Consume(TokenType.Semicolon);
            return new ExpressionStatementNode(exprStmt);
        }

        private VariableDeclarationNode ParseVariableDeclaration()
        {
            // Consume 'var' or 'const'. 
            // TODO: In the future, pass this to the Node to enforce immutability semantics.
            bool isConst = Match(TokenType.Keyword_Const);
            if (!isConst) Consume(TokenType.Keyword_Var);

            string name = Consume(TokenType.Identifier).Value;
            Consume(TokenType.Colon);
            string type = ConsumeType();
            Consume(TokenType.Equals);

            var init = ParseExpression();
            Consume(TokenType.Semicolon);

            return new VariableDeclarationNode(type, name, init);
        }

        private FunctionDeclarationNode ParseFunction(string moduleOwner)
        {
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

            var body = ParseBlock();
            return new FunctionDeclarationNode(name, retType, parameters, body, moduleOwner);
        }

        private BlockNode ParseBlock()
        {
            Consume(TokenType.OpenBrace);
            var block = new BlockNode();
            while (Current.Type != TokenType.CloseBrace)
                block.Statements.Add(ParseStatement());
            Consume(TokenType.CloseBrace);
            return block;
        }

        private bool IsType(TokenType t) => t >= TokenType.Type_I8 && t <= TokenType.Type_Void;

        /// <summary>
        /// Consumes a token that represents a valid Sage data type.
        /// </summary>
        private string ConsumeType()
        {
            if (IsType(Current.Type))
            {
                string v = Current.Value;
                _pos++;
                return v;
            }
            throw new Exception($"Expected a valid Type but found {Current.Type} ('{Current.Value}')");
        }

        private AstNode ParseExpression() => ParseAdditive();

        private AstNode ParseAdditive()
        {
            var left = ParseMultiplicative();
            while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
            {
                var op = Current.Type;
                _pos++;
                left = new BinaryExpressionNode(left, op, ParseMultiplicative());
            }
            return left;
        }

        private AstNode ParseMultiplicative()
        {
            var left = ParsePrimary();
            while (Current.Type == TokenType.Asterisk || Current.Type == TokenType.Slash)
            {
                var op = Current.Type;
                _pos++;
                left = new BinaryExpressionNode(left, op, ParsePrimary());
            }
            return left;
        }

        /// <summary>
        /// Parses the most basic components of an expression, such as literals, identifiers, and grouped expressions.
        /// </summary>
        private AstNode ParsePrimary()
        {
            if (Current.Type == TokenType.Integer)
                return new LiteralNode(Consume(TokenType.Integer).Value, "i32");

            if (Current.Type == TokenType.Float)
                return new LiteralNode(Consume(TokenType.Float).Value, "f64");

            if (Current.Type == TokenType.String)
                return new LiteralNode(Consume(TokenType.String).Value, "string");

            if (Current.Type == TokenType.Identifier)
            {
                string name = Consume(TokenType.Identifier).Value;

                // Handle static namespace calls (e.g., math::sum)
                if (Match(TokenType.DoubleColon))
                    name += "::" + Consume(TokenType.Identifier).Value;

                // Handle function calls
                if (Match(TokenType.OpenParen))
                {
                    var args = new List<AstNode>();
                    if (Current.Type != TokenType.CloseParen)
                    {
                        do { args.Add(ParseExpression()); } while (Match(TokenType.Comma));
                    }
                    Consume(TokenType.CloseParen);
                    return new FunctionCallNode(name, args);
                }
                return new IdentifierNode(name);
            }

            if (Match(TokenType.OpenParen))
            {
                var expr = ParseExpression();
                Consume(TokenType.CloseParen);
                return expr;
            }

            throw new Exception($"Unexpected token in expression: {Current.Type} ('{Current.Value}')");
        }
    }
}