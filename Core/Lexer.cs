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
            { "i32", TokenType.Type_I32 },
            // TODO: Add more types here (u8, i64, etc)
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

            while (char.IsDigit(Current))
            {
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