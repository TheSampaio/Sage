using Sage.Ast;
using Sage.Enums;
using Sage.Interfaces;

namespace Sage.Core
{
    public class Parser : IParser
    {
        private readonly List<Token> _tokens;
        private int _pos = 0;

        public Parser(List<Token> tokens) => _tokens = tokens;
        private Token Current => _tokens[_pos];
        private Token Consume(TokenType type)
        {
            if (Current.Type == type) { var t = Current; _pos++; return t; }
            throw new Exception($"Expected {type} but got {Current.Type} at Line {Current.Line}");
        }
        private bool Match(TokenType type) { if (Current.Type == type) { _pos++; return true; } return false; }

        public ProgramNode Parse()
        {
            var program = new ProgramNode();
            while (Current.Type != TokenType.EndOfFile)
            {
                if (Current.Type == TokenType.Keyword_Module) program.Statements.Add(ParseModule());
                else program.Statements.Add(ParseStatement());
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
                {
                    var func = ParseFunction(module.Name);
                    module.Functions.Add(func);
                }
                else { throw new Exception("Only functions allowed in modules for now."); }
            }
            Consume(TokenType.CloseBrace);
            return module;
        }

        private AstNode ParseStatement()
        {
            if (Match(TokenType.Keyword_Use))
            {
                var name = Consume(TokenType.Identifier).Value; Consume(TokenType.Semicolon);
                return new UseNode(name);
            }
            if (Current.Type == TokenType.Keyword_Func) return ParseFunction("");
            if (Match(TokenType.Keyword_Return))
            {
                var expr = ParseExpression(); Consume(TokenType.Semicolon);
                return new ReturnNode(expr);
            }
            // Variable Declaration: Type Identifier = ...
            if (IsType(Current.Type))
            {
                string type = Current.Value; _pos++;
                string name = Consume(TokenType.Identifier).Value;
                Consume(TokenType.Equals);
                var init = ParseExpression(); Consume(TokenType.Semicolon);
                return new VariableDeclarationNode(type, name, init);
            }
            var exprStmt = ParseExpression(); Consume(TokenType.Semicolon);
            return new ExpressionStatementNode(exprStmt);
        }

        private FunctionDeclarationNode ParseFunction(string moduleOwner)
        {
            Consume(TokenType.Keyword_Func);
            string name = Consume(TokenType.Identifier).Value;

            // Parâmetros
            Consume(TokenType.OpenParen);
            var parameters = new List<ParameterNode>();
            if (Current.Type != TokenType.CloseParen)
            {
                do
                {
                    string pType = ConsumeType();
                    string pName = Consume(TokenType.Identifier).Value;
                    parameters.Add(new ParameterNode(pName, pType));
                } while (Match(TokenType.Comma));
            }
            Consume(TokenType.CloseParen);

            // Retorno (syntaxe: func name(): type)
            Consume(TokenType.Colon);
            string retType = ConsumeType();

            var body = ParseBlock();
            return new FunctionDeclarationNode(name, retType, parameters, body, moduleOwner);
        }

        private BlockNode ParseBlock()
        {
            Consume(TokenType.OpenBrace);
            var block = new BlockNode();
            while (Current.Type != TokenType.CloseBrace) block.Statements.Add(ParseStatement());
            Consume(TokenType.CloseBrace);
            return block;
        }

        // Simples Recursive Descent Expression Parser (focado em + - * /)
        private AstNode ParseExpression() => ParseAdditive();

        private AstNode ParseAdditive()
        {
            var left = ParseMultiplicative();
            while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
            {
                var op = Current.Type; _pos++;
                left = new BinaryExpressionNode(left, op, ParseMultiplicative());
            }
            return left;
        }

        private AstNode ParseMultiplicative()
        {
            var left = ParsePrimary();
            while (Current.Type == TokenType.Asterisk || Current.Type == TokenType.Slash)
            {
                var op = Current.Type; _pos++;
                left = new BinaryExpressionNode(left, op, ParsePrimary());
            }
            return left;
        }

        private AstNode ParsePrimary()
        {
            if (Current.Type == TokenType.Number) return new LiteralNode(Consume(TokenType.Number).Value, "i32");
            if (Current.Type == TokenType.String) return ParseStringInterpolation(Consume(TokenType.String).Value);

            if (Current.Type == TokenType.Identifier)
            {
                string name = Consume(TokenType.Identifier).Value;
                // Namespace resolution
                if (Match(TokenType.DoubleColon)) name += "::" + Consume(TokenType.Identifier).Value;

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
            throw new Exception("Unexpected token in expression");
        }

        private AstNode ParseStringInterpolation(string text)
        {
            if (!text.Contains("{")) return new LiteralNode(text, "string");
            // Implementação simplificada de interpolação:
            // Quebra "ola {nome}" em [Literal("ola "), Identifier(nome)]
            var node = new InterpolatedStringNode();
            // ... (Logica de parsing de string interpolada seria inserida aqui, mantendo simples para o exemplo)
            // Assumindo que o lexer anterior fazia isso via sub-lexer, aqui vamos simplificar:
            // Em Clean Code, extrairíamos essa lógica para um "StringInterpolatorParser".
            // Para compilar o exemplo, trataremos como string normal por enquanto no C
            // mas com suporte a placeholders do printf.
            return new LiteralNode(text, "string");
        }

        private bool IsType(TokenType t) => t >= TokenType.Type_U8 && t <= TokenType.Type_Void;
        private string ConsumeType()
        {
            if (IsType(Current.Type)) { string v = Current.Value; _pos++; return v; }
            throw new Exception("Expected Type");
        }
    }
}
