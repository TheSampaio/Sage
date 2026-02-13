using Sage.Enums;
using Sage.Interfaces;

namespace Sage.Core
{
    public class Lexer : ILexer
    {
        private readonly string _text;
        private int _pos, _line = 1, _col = 1;

        // Dicionário estático para performance
        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
            { "use", TokenType.Keyword_Use }, { "func", TokenType.Keyword_Func },
            { "return", TokenType.Keyword_Return }, { "module", TokenType.Keyword_Module },
            { "i32", TokenType.Type_I32 }, { "none", TokenType.Type_Void }, // (Outros tipos omitidos por brevidade)
        };

        public Lexer(string text) => _text = text;

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            while (_pos < _text.Length)
            {
                char current = _text[_pos];
                if (char.IsWhiteSpace(current)) { Advance(); continue; }

                if (char.IsLetter(current) || current == '_') tokens.Add(LexIdentifier());
                else if (char.IsDigit(current)) tokens.Add(LexNumber());
                else if (current == '"') tokens.Add(LexString());
                else tokens.Add(LexSymbol());
            }
            tokens.Add(new Token(TokenType.EndOfFile, "\0", _line, _col));
            return tokens;
        }

        private void Advance()
        {
            if (_text[_pos] == '\n') { _line++; _col = 0; }
            _pos++; _col++;
        }

        private Token LexIdentifier()
        {
            int start = _pos;
            while (_pos < _text.Length && (char.IsLetterOrDigit(_text[_pos]) || _text[_pos] == '_')) Advance();
            string txt = _text[start.._pos];
            return new Token(Keywords.GetValueOrDefault(txt, TokenType.Identifier), txt, _line, _col);
        }

        private Token LexNumber() { /* Lógica de números igual ao anterior */ int start = _pos; while (_pos < _text.Length && char.IsDigit(_text[_pos])) Advance(); return new Token(TokenType.Number, _text[start.._pos], _line, _col); }

        private Token LexString()
        {
            int start = _pos; Advance();
            while (_pos < _text.Length && _text[_pos] != '"') Advance();
            Advance(); // Fecha aspas
                       // Remove aspas para o valor
            string val = _text.Substring(start + 1, (_pos - start) - 2);
            return new Token(TokenType.String, val, _line, _col);
        }

        private Token LexSymbol()
        {
            int startCol = _col;
            char c = _text[_pos];
            Advance();

            // Símbolos compostos
            if (c == ':' && _pos < _text.Length && _text[_pos] == ':') { Advance(); return new Token(TokenType.DoubleColon, "::", _line, startCol); }

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
            return new Token(type, c.ToString(), _line, startCol);
        }
    }
}
