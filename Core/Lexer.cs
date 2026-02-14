using Sage.Enums;
using Sage.Interfaces;

namespace Sage.Core
{
    /// <summary>
    /// Performs lexical analysis on Sage source code, converting raw text into a sequence of tokens.
    /// </summary>
    public class Lexer(string text) : ILexer
    {
        private readonly string _text = text;
        private int _pos;
        private int _line = 1;
        private int _col = 1;

        /// <summary>
        /// Gets the character at the current position.
        /// </summary>
        private char Current => _pos < _text.Length ? _text[_pos] : '\0';

        /// <summary>
        /// Gets the character immediately following the current position.
        /// </summary>
        private char Lookahead => _pos + 1 < _text.Length ? _text[_pos + 1] : '\0';

        /// <summary>
        /// Mapping of Sage keywords and primitive types to their respective token types.
        /// </summary>
        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
            { "use", TokenType.Keyword_Use },
            { "func", TokenType.Keyword_Func },
            { "return", TokenType.Keyword_Return },
            { "module", TokenType.Keyword_Module },

            { "i8", TokenType.Type_I8 },   { "u8", TokenType.Type_U8 },
            { "i16", TokenType.Type_I16 }, { "u16", TokenType.Type_U16 },
            { "i32", TokenType.Type_I32 }, { "u32", TokenType.Type_U32 },
            { "i64", TokenType.Type_I64 }, { "u64", TokenType.Type_U64 },
            { "f32", TokenType.Type_F32 }, { "f64", TokenType.Type_F64 },
            { "b8", TokenType.Type_B8 },   { "c8", TokenType.Type_C8 },
            { "c16", TokenType.Type_C16 }, { "c32", TokenType.Type_C32 },
            { "str", TokenType.Type_Str }, { "none", TokenType.Type_Void },
        };

        /// <summary>
        /// Iterates through the source text to generate a list of valid tokens.
        /// </summary>
        /// <returns>A list of identified tokens ending with an EndOfFile token.</returns>
        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_pos < _text.Length)
            {
                char current = Current;

                if (char.IsWhiteSpace(current))
                {
                    Advance();
                    continue;
                }

                if (char.IsLetter(current) || current == '_')
                {
                    tokens.Add(LexIdentifier());
                }
                else if (char.IsDigit(current))
                {
                    tokens.Add(LexNumber());
                }
                else if (current == '"')
                {
                    tokens.Add(LexString());
                }
                else
                {
                    Token? symbol = LexSymbol();
                    if (symbol != null)
                    {
                        tokens.Add(symbol);
                    }
                }
            }

            tokens.Add(new Token(TokenType.EndOfFile, "\0", _line, _col));
            return tokens;
        }

        /// <summary>
        /// Moves the pointer forward and updates line/column trackers.
        /// </summary>
        private void Advance()
        {
            if (_pos < _text.Length)
            {
                if (_text[_pos] == '\n')
                {
                    _line++;
                    _col = 0;
                }
                _pos++;
                _col++;
            }
        }

        /// <summary>
        /// Reads an identifier or maps it to a keyword if applicable.
        /// </summary>
        private Token LexIdentifier()
        {
            int start = _pos;
            int startCol = _col;

            while (_pos < _text.Length && (char.IsLetterOrDigit(Current) || Current == '_'))
            {
                Advance();
            }

            string txt = _text[start.._pos];
            var type = Keywords.GetValueOrDefault(txt, TokenType.Identifier);

            return new Token(type, txt, _line, startCol);
        }

        /// <summary>
        /// Reads a numeric literal, supporting optional floating-point notation.
        /// </summary>
        private Token LexNumber()
        {
            int start = _pos;
            int startCol = _col;

            while (_pos < _text.Length && char.IsDigit(Current))
            {
                Advance();
            }

            if (Current == '.' && char.IsDigit(Lookahead))
            {
                Advance();
                while (_pos < _text.Length && char.IsDigit(Current))
                {
                    Advance();
                }
            }

            string value = _text[start.._pos];
            return new Token(TokenType.Number, value, _line, startCol);
        }

        /// <summary>
        /// Reads a string literal enclosed in double quotes.
        /// </summary>
        private Token LexString()
        {
            int start = _pos;
            int startCol = _col;

            Advance(); // Skip opening quote

            while (_pos < _text.Length && Current != '"')
            {
                Advance();
            }

            Advance(); // Skip closing quote

            string val = _text.Substring(start + 1, (_pos - start) - 2);
            return new Token(TokenType.String, val, _line, startCol);
        }

        /// <summary>
        /// Handles symbols, multi-character operators, and skips comments.
        /// </summary>
        /// <returns>A token if the sequence represents a symbol; null if it is a comment.</returns>
        private Token? LexSymbol()
        {
            char c = Current;
            char next = Lookahead;

            // Handle line comments
            if (c == '/' && next == '/')
            {
                while (_pos < _text.Length && Current != '\n')
                {
                    Advance();
                }
                return null;
            }

            // Handle multi-character operators
            if (c == ':' && next == ':')
            {
                int startCol = _col;
                Advance();
                Advance();
                return new Token(TokenType.DoubleColon, "::", _line, startCol);
            }

            int tokenCol = _col;
            Advance();

            TokenType type = c switch
            {
                '+' => TokenType.Plus,
                '-' => TokenType.Minus,
                '*' => TokenType.Asterisk,
                '/' => TokenType.Slash,
                '=' => TokenType.Equals,
                '(' => TokenType.OpenParen,
                ')' => TokenType.CloseParen,
                '{' => TokenType.OpenBrace,
                '}' => TokenType.CloseBrace,
                ';' => TokenType.Semicolon,
                ',' => TokenType.Comma,
                ':' => TokenType.Colon,
                _ => TokenType.Unknown
            };

            if (type == TokenType.Unknown)
            {
                Console.WriteLine($"[LEXER WARNING] Unexpected character '{c}' at {_line}:{tokenCol}");
            }

            return new Token(type, c.ToString(), _line, tokenCol);
        }
    }
}