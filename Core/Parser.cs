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
        /// Checks if the current token represents a known data type.
        /// This is used to detect the start of a Variable Declaration.
        /// </summary>
        /// <summary>
        /// Checks if the current token represents a known data type.
        /// Based strictly on Sage.Enums.TokenType.
        /// </summary>
        private bool IsType(Token token)
        {
            return token.Type is
                // Signed Integers
                TokenType.Type_I8 or TokenType.Type_I16 or TokenType.Type_I32 or TokenType.Type_I64 or
                // Unsigned Integers
                TokenType.Type_U8 or TokenType.Type_U16 or TokenType.Type_U32 or TokenType.Type_U64 or
                // Floating Point
                TokenType.Type_F32 or TokenType.Type_F64 or
                // Boolean & Text & Void
                TokenType.Type_B8 or
                TokenType.Type_C8 or TokenType.Type_C16 or TokenType.Type_C32 or
                TokenType.Type_Str or
                TokenType.Type_Void;
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
                if (Current.Type == TokenType.Keyword_Func)
                {
                    // CORREÇÃO: Adicione à lista unificada 'Members'
                    module.Members.Add(ParseFunction(module.Name));
                }
                else if (Current.Type == TokenType.Keyword_Extern)
                {
                    // Já estava correto, adicionando a Members
                    module.Members.Add(ParseExternBlock());
                }
                else
                {
                    throw new CompilerException(Current, "S002", "Only functions and extern blocks are supported in modules.");
                }
            }

            Consume(TokenType.CloseBrace);
            return module;
        }

        /// <summary>
        /// Parses an FFI block: extern alias("header") { ... }
        /// </summary>
        private ExternBlockNode ParseExternBlock()
        {
            var startToken = Current;
            Consume(TokenType.Keyword_Extern);

            // 1. Alias (namespace local)
            string alias = Consume(TokenType.Identifier).Value;

            // 2. Header File ("stdio.h")
            Consume(TokenType.OpenParen);
            string header = Consume(TokenType.String).Value;
            Consume(TokenType.CloseParen);

            Consume(TokenType.OpenBrace);

            var declarations = new List<AstNode>();

            while (Current.Type != TokenType.CloseBrace && Current.Type != TokenType.EndOfFile)
            {
                if (Match(TokenType.Keyword_Func))
                {
                    // Parse de função externa dentro do bloco
                    // Reutilizamos a lógica de função, mas forçamos isExtern = true
                    // E o nome será registrado no SemanticAnalyzer como alias::funcName
                    string funcName = Consume(TokenType.Identifier).Value;
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
                    Consume(TokenType.Semicolon); // Externs não tem corpo

                    // Criamos a declaração. Note que ModuleOwner aqui será o ALIAS do extern para resolução interna
                    var funcNode = new FunctionDeclarationNode(funcName, retType, parameters, null!, alias)
                    {
                        IsExtern = true
                    };
                    declarations.Add(CreateNode(funcNode, startToken));
                }
                // TODO: Adicionar suporte a 'struct' aqui no futuro
                else
                {
                    throw new CompilerException(Current, "S006", "Only function declarations are allowed inside extern blocks for now.");
                }
            }

            Consume(TokenType.CloseBrace);
            return CreateNode(new ExternBlockNode(alias, header, declarations), startToken);
        }

        /// <summary>Parses a single statement or declaration.</summary>
        /// <summary>Parses a single statement or declaration.</summary>
        private AstNode ParseStatement()
        {
            var startToken = Current;

            // 1. Import directive
            if (Match(TokenType.Keyword_Use))
            {
                string name = Consume(TokenType.Identifier).Value;
                Consume(TokenType.Semicolon);
                return CreateNode(new UseNode(name), startToken);
            }

            // 2. Function definition
            if (Current.Type == TokenType.Keyword_Func) return ParseFunction("");

            // 3. Control Flow
            if (Current.Type == TokenType.Keyword_If) return ParseIf();
            if (Current.Type == TokenType.Keyword_While) return ParseWhile();
            if (Current.Type == TokenType.Keyword_For) return ParseFor();

            if (Match(TokenType.Keyword_Return))
            {
                var expr = ParseExpression();
                Consume(TokenType.Semicolon);
                return CreateNode(new ReturnNode(expr), startToken);
            }

            // 4. Variable Declaration
            // Verifica se começa com 'var' ou 'const'
            if (Current.Type == TokenType.Keyword_Var || Current.Type == TokenType.Keyword_Const)
            {
                return ParseVariableDeclaration();
            }

            // 5. Expression Statement (Assignments, Calls)
            // Ex: x = 1; (Assignment usa Equals, mas é parseado dentro de ParseExpression -> ParseAssignment)
            var expression = ParseExpression();
            Consume(TokenType.Semicolon);
            return CreateNode(new ExpressionStatementNode(expression), startToken);
        }

        /// <summary>Parses an 'if' statement, including optional 'else' blocks.</summary>
        private IfNode ParseIf()
        {
            var startToken = Current;
            Consume(TokenType.Keyword_If);
            Consume(TokenType.OpenParen);
            var condition = ParseExpression();
            Consume(TokenType.CloseParen);

            var thenBranch = ParseBlock();
            AstNode? elseBranch = null;

            if (Match(TokenType.Keyword_Else))
            {
                // Suporte para 'else if' recursivo
                if (Current.Type == TokenType.Keyword_If)
                {
                    elseBranch = ParseIf();
                }
                else
                {
                    elseBranch = ParseBlock();
                }
            }

            return CreateNode(new IfNode(condition, thenBranch, elseBranch), startToken);
        }

        /// <summary>Parses a 'while' loop.</summary>
        private WhileNode ParseWhile()
        {
            var startToken = Current;
            Consume(TokenType.Keyword_While);
            Consume(TokenType.OpenParen);
            var condition = ParseExpression();
            Consume(TokenType.CloseParen);

            var body = ParseBlock();
            return CreateNode(new WhileNode(condition, body), startToken);
        }

        /// <summary>Parses a 'for' loop: for(init; condition; increment) { ... }</summary>
        private ForNode ParseFor()
        {
            var startToken = Current;
            Consume(TokenType.Keyword_For);
            Consume(TokenType.OpenParen);

            // 1. Initializer (pode ser vazio, declaração ou expressão)
            AstNode? initializer = null;
            if (Current.Type != TokenType.Semicolon)
            {
                if (IsType(Current))
                    initializer = ParseVariableDeclaration(); // int i = 0;
                else
                    initializer = ParseExpression(); // i = 0

                // ParseVariableDeclaration já consome o ponto e vírgula interno se for declaração?
                // Geralmente VariableDeclaration consome o ';'. Se for expressão pura, precisamos consumir.
                // Ajuste conforme sua implementação de ParseVariableDeclaration. 
                // Assumindo que ParseVariableDeclaration consome ';', mas ParseExpression não.

                // NOTA: Para simplificar, vamos assumir que ParseVariableDeclaration consome ';'.
                // Se foi expressão, precisamos consumir.
                if (initializer is not VariableDeclarationNode)
                {
                    Consume(TokenType.Semicolon);
                }
            }
            else
            {
                Consume(TokenType.Semicolon); // Vazio
            }

            // 2. Condition
            AstNode? condition = null;
            if (Current.Type != TokenType.Semicolon)
            {
                condition = ParseExpression();
            }
            Consume(TokenType.Semicolon);

            // 3. Increment
            AstNode? increment = null;
            if (Current.Type != TokenType.CloseParen)
            {
                increment = ParseExpression();
            }
            Consume(TokenType.CloseParen);

            var body = ParseBlock();

            return CreateNode(new ForNode(initializer, condition, increment, body), startToken);
        }

        /// <summary>Parses variable (var) or constant (const) declarations.</summary>
        /// <summary>
        /// Parses a variable declaration: var name: type = value;
        /// </summary>
        private VariableDeclarationNode ParseVariableDeclaration()
        {
            var startToken = Current;
            bool isConstant = false;

            // 1. Consume Keyword (var / const)
            if (Match(TokenType.Keyword_Const))
            {
                isConstant = true;
            }
            else
            {
                Consume(TokenType.Keyword_Var, "Expected 'var' or 'const'.");
            }

            // 2. Identifier Name
            string name = Consume(TokenType.Identifier).Value;

            // 3. Type Annotation
            Consume(TokenType.Colon, "Expected ':' after variable name.");
            string type = ConsumeType(); // Retorna "str", "i32", "none*", etc.

            // 4. Initializer
            Consume(TokenType.Equals, "Expected '=' for initialization.");
            var initializer = ParseExpression();

            Consume(TokenType.Semicolon);

            return CreateNode(new VariableDeclarationNode(name, type, initializer, isConstant), startToken);
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

            var statements = new List<AstNode>();
            while (Current.Type != TokenType.CloseBrace && Current.Type != TokenType.EndOfFile)
            {
                statements.Add(ParseStatement());
            }

            Consume(TokenType.CloseBrace);
            // Cria o nó já com a lista completa, garantindo imutabilidade se necessário
            return CreateNode(new BlockNode(statements), startToken);
        }

        /// <summary>Helper to consume and return a valid Sage data type.</summary>
        /// <summary>
        /// Consumes a type token and handles pointer syntax (e.g., "i32", "none*", "u8**").
        /// </summary>
        /// <summary>
        /// Consumes a type token and handles pointer syntax (e.g., "i32", "none*", "u8**").
        /// </summary>
        private string ConsumeType()
        {
            // 1. Verifica se começa com um tipo válido (int, float, void, struct name...)
            if (!IsType(Current))
            {
                // Se não for tipo primitivo, pode ser um Identifier (nome de struct/enum)
                // Mas por enquanto vamos nos ater aos primitivos definidos no seu Enum + Void
                if (Current.Type != TokenType.Identifier)
                {
                    throw new CompilerException(Current, "S003", $"Expected a type but found {Current.Type} ('{Current.Value}')");
                }
            }

            // 2. Consome o tipo base (ex: "none", "i32", "str")
            string type = Current.Value;
            _pos++; // Avança manualmente (Consume sem validação de tipo fixo)

            // 3. IMPLEMENTAÇÃO DE PONTEIROS
            // Enquanto o próximo token for um Asterisco (*), anexa ao tipo.
            while (Current.Type == TokenType.Asterisk)
            {
                type += "*";
                _pos++; // Consome o '*'
            }

            return type;
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