using Sage.Enums;
using Sage.Interfaces;
using Sage.Utilities;

namespace Sage.Core
{
    /// <summary>
    /// Performs lexical analysis on Sage source code. 
    /// Converts raw text into a sequence of tokens using a single-pass scanner.
    /// </summary>
    /// <param name="text">The raw source code to be tokenized.</param>
    public class Lexer(string text) : ILexer
    {
        private readonly string _text = text;
        private int _pos = 0;
        private int _line = 1;
        private int _col = 1;

        /// <summary>
        /// Gets the character at the current scanner position.
        /// </summary>
        private char Current => _pos < _text.Length ? _text[_pos] : '\0';

        /// <summary>
        /// Peeks at the character immediately following the current position.
        /// </summary>
        private char Lookahead => _pos + 1 < _text.Length ? _text[_pos + 1] : '\0';

        /// <summary>
        /// Internal map of keywords and built-in types to their respective token types.
        /// </summary>
        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
            { "use", TokenType.Keyword_Use }, { "func", TokenType.Keyword_Func },
            { "return", TokenType.Keyword_Return }, { "module", TokenType.Keyword_Module },
            { "var", TokenType.Keyword_Var }, { "const", TokenType.Keyword_Const },
            { "as", TokenType.Keyword_As }, { "true", TokenType.Keyword_True },
            { "false", TokenType.Keyword_False }, { "if", TokenType.Keyword_If },
            { "else", TokenType.Keyword_Else }, { "while", TokenType.Keyword_While },
            { "for", TokenType.Keyword_For },

            // Fixed-width types
            { "i8", TokenType.Type_I8 }, { "u8", TokenType.Type_U8 },
            { "i16", TokenType.Type_I16 }, { "u16", TokenType.Type_U16 },
            { "i32", TokenType.Type_I32 }, { "u32", TokenType.Type_U32 },
            { "i64", TokenType.Type_I64 }, { "u64", TokenType.Type_U64 },
            { "f32", TokenType.Type_F32 }, { "f64", TokenType.Type_F64 },
            { "b8", TokenType.Type_B8 }, { "str", TokenType.Type_Str },
            { "none", TokenType.Type_Void }
        };

        /// <summary>
        /// Scans the entire source text and returns a list of valid tokens.
        /// </summary>
        /// <returns>A list of Token objects, ending with EndOfFile.</returns>
        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_pos < _text.Length)
            {
                char c = Current;

                if (char.IsWhiteSpace(c))
                {
                    Advance();
                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    tokens.Add(LexIdentifier());
                    continue;
                }

                if (char.IsDigit(c))
                {
                    tokens.Add(LexNumber());
                    continue;
                }

                if (c == '"')
                {
                    tokens.Add(LexString());
                    continue;
                }

                Token? symbol = LexSymbol();
                if (symbol != null) tokens.Add(symbol);
            }

            tokens.Add(new Token(TokenType.EndOfFile, "\0", _line, _col));
            return tokens;
        }

        /// <summary>
        /// Advances the scanner position by one and updates line/column trackers.
        /// </summary>
        private void Advance()
        {
            if (_pos < _text.Length)
            {
                if (_text[_pos] == '\n')
                {
                    _line++;
                    _col = 1;
                }
                else
                {
                    _col++;
                }
                _pos++;
            }
        }

        /// <summary>
        /// Reads an alphanumeric identifier and checks if it matches a reserved keyword.
        /// </summary>
        /// <returns>A token of type Identifier or Keyword.</returns>
        private Token LexIdentifier()
        {
            int startPos = _pos;
            int startCol = _col;

            while (_pos < _text.Length && (char.IsLetterOrDigit(Current) || Current == '_'))
            {
                Advance();
            }

            string value = _text[startPos.._pos];
            var type = Keywords.GetValueOrDefault(value, TokenType.Identifier);

            return new Token(type, value, _line, startCol);
        }

        /// <summary>
        /// Scans a numeric literal, supporting both integers and floating-point numbers.
        /// </summary>
        /// <returns>A token of type Integer or Float.</returns>
        private Token LexNumber()
        {
            int startPos = _pos;
            int startCol = _col;

            while (_pos < _text.Length && char.IsDigit(Current)) Advance();

            // Floating point check
            if (Current == '.' && char.IsDigit(Lookahead))
            {
                Advance(); // Consume '.'
                while (_pos < _text.Length && char.IsDigit(Current)) Advance();
                return new Token(TokenType.Float, _text[startPos.._pos], _line, startCol);
            }

            return new Token(TokenType.Integer, _text[startPos.._pos], _line, startCol);
        }

        /// <summary>
        /// Scans a string literal enclosed in double quotes.
        /// </summary>
        /// <returns>A token containing the string content without quotes.</returns>
        private Token LexString()
        {
            int startPos = _pos;
            int startCol = _col;

            Advance(); // Skip opening "
            while (_pos < _text.Length && Current != '"') Advance();
            Advance(); // Skip closing "

            string value = _text[(startPos + 1)..(_pos - 1)];
            return new Token(TokenType.String, value, _line, startCol);
        }

        /// <summary>
        /// Handles symbols, multi-character operators, and comments.
        /// </summary>
        /// <returns>A Token object, or null if a comment was skipped.</returns>
        private Token? LexSymbol()
        {
            char c = Current;
            char next = Lookahead;
            int startCol = _col;

            // Handle Comments (//)
            if (c == '/' && next == '/')
            {
                while (_pos < _text.Length && Current != '\n') Advance();
                return null;
            }

            // Multi-character Operators
            if (c == ':' && next == ':') { Advance(); Advance(); return new Token(TokenType.DoubleColon, "::", _line, startCol); }
            if (c == '=' && next == '=') { Advance(); Advance(); return new Token(TokenType.EqualEqual, "==", _line, startCol); }
            if (c == '!' && next == '=') { Advance(); Advance(); return new Token(TokenType.NotEqual, "!=", _line, startCol); }
            if (c == '<' && next == '=') { Advance(); Advance(); return new Token(TokenType.LessEqual, "<=", _line, startCol); }
            if (c == '>' && next == '=') { Advance(); Advance(); return new Token(TokenType.GreaterEqual, ">=", _line, startCol); }
            if (c == '&' && next == '&') { Advance(); Advance(); return new Token(TokenType.AmpersandAmpersand, "&&", _line, startCol); }
            if (c == '|' && next == '|') { Advance(); Advance(); return new Token(TokenType.PipePipe, "||", _line, startCol); }
            if (c == '+' && next == '+') { Advance(); Advance(); return new Token(TokenType.PlusPlus, "++", _line, startCol); }

            // Single-character Operators
            Advance();
            TokenType type = c switch
            {
                '+' => TokenType.Plus,
                '-' => TokenType.Minus,
                '*' => TokenType.Asterisk,
                '/' => TokenType.Slash,
                '%' => TokenType.Percent,
                '=' => TokenType.Equals,
                '(' => TokenType.OpenParen,
                ')' => TokenType.CloseParen,
                '{' => TokenType.OpenBrace,
                '}' => TokenType.CloseBrace,
                ';' => TokenType.Semicolon,
                ',' => TokenType.Comma,
                ':' => TokenType.Colon,
                '<' => TokenType.Less,
                '>' => TokenType.Greater,
                '!' => TokenType.Bang,
                _ => TokenType.Unknown
            };

            if (type == TokenType.Unknown)
            {
                CompilerLogger.LogError(new Token(type, c.ToString(), _line, startCol), "S000", $"Unexpected character '{c}'.");
            }

            return new Token(type, c.ToString(), _line, startCol);
        }
    }
}