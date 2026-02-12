using Sage.Enums;
using System.Text;

namespace Sage.Core
{
    public class Lexer
    {
        private readonly string _text;
        private int _position;
        private int _line = 1;
        private int _column = 1;

        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            { "use", TokenType.Keyword_Use },
            { "function", TokenType.Keyword_Func },
            { "return", TokenType.Keyword_Return },

            // Primitive Types
            { "u8", TokenType.Type_U8 },
            { "u16", TokenType.Type_U16 },
            { "u32", TokenType.Type_U32 },
            { "u64", TokenType.Type_U64 },

            { "i8", TokenType.Type_I8 },
            { "i16", TokenType.Type_I16 },
            { "i32", TokenType.Type_I32 },
            { "i64", TokenType.Type_I64 },

            { "f32", TokenType.Type_F32 },
            { "f64", TokenType.Type_F64 },

            { "b8", TokenType.Type_B8 },
            { "c8", TokenType.Type_Char },
            { "str", TokenType.Type_Str }, // Vamos manter 'string' como alias de std::string por enquanto
            { "none", TokenType.Type_Void },

            { "null", TokenType.Value_Null }
        };

        public Lexer(string text)
        {
            _text = text;
        }

        private char Current => Peek(0);
        private char Lookahead => Peek(1);

        private char Peek(int offset)
        {
            int index = _position + offset;
            if (index >= _text.Length)
                return '\0';
            return _text[index];
        }

        private void Next()
        {
            _position++;
            _column++;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _text.Length)
            {
                char current = Current;

                // 1. Ignore Whitespace
                if (char.IsWhiteSpace(current))
                {
                    if (current == '\n')
                    {
                        _line++;
                        _column = 0;
                    }
                    Next();
                }
                // 2. Commentaries (//)
                else if (current == '/' && Lookahead == '/')
                {
                    while (Current != '\n' && Current != '\0')
                        Next();
                }
                // 3. Identifiers e Keywords
                else if (char.IsLetter(current) || current == '_')
                {
                    tokens.Add(ReadIdentifierOrKeyword());
                }
                // 4. Numbers
                else if (char.IsDigit(current))
                {
                    tokens.Add(ReadNumber());
                }
                // 5. Strings
                else if (current == '"')
                {
                    tokens.Add(ReadString());
                }
                // 6. Operators and Punctuation
                else
                {
                    int startCol = _column;

                    // Treatment of compound operators (->, ::)
                    if (current == '-' && Lookahead == '>')
                    {
                        tokens.Add(new Token(TokenType.Arrow, "->", _line, startCol));
                        Next(); Next();
                    }
                    else if (current == ':' && Lookahead == ':')
                    {
                        tokens.Add(new Token(TokenType.DoubleColon, "::", _line, startCol));
                        Next(); Next();
                    }
                    else
                    {
                        // Simple operators and punctuation
                        TokenType type = current switch
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
                            _ => TokenType.Unknown
                        };

                        if (type == TokenType.Unknown)
                        {
                            Console.WriteLine($"[LEXER ERROR] Unexpected character '{current}' at {_line}:{_column}");
                        }

                        tokens.Add(new Token(type, current.ToString(), _line, startCol));
                        Next();
                    }
                }
            }

            tokens.Add(new Token(TokenType.EndOfFile, "\0", _line, _column));
            return tokens;
        }

        private Token ReadIdentifierOrKeyword()
        {
            int startCol = _column;
            var sb = new StringBuilder();

            while (char.IsLetterOrDigit(Current) || Current == '_')
            {
                sb.Append(Current);
                Next();
            }

            string text = sb.ToString();
            if (!Keywords.TryGetValue(text, out TokenType type))
            {
                type = TokenType.Identifier;
            }

            return new Token(type, text, _line, startCol);
        }

        private Token ReadNumber()
        {
            int startCol = _column;
            var sb = new StringBuilder();
            bool hasDecimalSeparator = false;

            while (char.IsDigit(Current) || (Current == '.' && !hasDecimalSeparator))
            {
                if (Current == '.')
                {
                    // If the next character isn't a digit, this dot might not be part of a number
                    // (Useful for future member access like object.method)
                    if (!char.IsDigit(Lookahead)) break;

                    hasDecimalSeparator = true;
                }

                sb.Append(Current);
                Next();
            }

            return new Token(TokenType.Number, sb.ToString(), _line, startCol);
        }

        private Token ReadString()
        {
            int startCol = _column;
            Next(); // Jump the opening quote
            var sb = new StringBuilder();

            while (Current != '"' && Current != '\0')
            {
                sb.Append(Current);
                Next();
            }

            Next(); // Jump the closing quote
            return new Token(TokenType.String, sb.ToString(), _line, startCol);
        }
    }
}