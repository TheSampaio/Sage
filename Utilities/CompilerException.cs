using Sage.Ast;
using Sage.Core;

namespace Sage.Utilities
{
    public class CompilerException : Exception
    {
        public Token? OffendingToken { get; private set; }
        public string ErrorCode { get; }
        public int Line { get; }
        public int Column { get; }

        // Construtor para Lexer/Parser (Usa Token)
        public CompilerException(Token? token, string code, string message) : base(message)
        {
            ErrorCode = code;
            OffendingToken = token;
            if (token != null)
            {
                Line = token.Line;
                Column = token.Column;
            }
        }

        // Construtor para Semantic (Usa AstNode)
        public CompilerException(AstNode node, string code, string message) : base(message)
        {
            ErrorCode = code;
            Line = node.Line;
            Column = node.Column;
            OffendingToken = null;
        }

        public override string ToString()
        {
            // Mantendo seu padrão original de saída
            if (OffendingToken != null)
            {
                return $"error {ErrorCode}: {Message} at line {OffendingToken.Line}, column {OffendingToken.Column} ('{OffendingToken.Value}')";
            }

            if (Line > 0)
            {
                return $"error {ErrorCode}: {Message} at line {Line}, column {Column}";
            }

            return $"error {ErrorCode}: {Message}";
        }
    }
}