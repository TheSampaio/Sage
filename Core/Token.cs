using Sage.Enums;

namespace Sage.Core
{
    /// <summary>
    /// Represents a single unit of the source code, identified during lexical analysis.
    /// Stores the type, literal value, and exact source location for error reporting.
    /// </summary>
    /// <param name="type">The categorized type of the token.</param>
    /// <param name="value">The raw string value as it appears in the source code.</param>
    /// <param name="line">The 1-based line number where the token was found.</param>
    /// <param name="column">The 1-based column position where the token starts.</param>
    public class Token(TokenType type, string value, int line, int column)
    {
        /// <summary>
        /// Gets the specific <see cref="TokenType"/> of this token.
        /// </summary>
        public TokenType Type { get; } = type;

        /// <summary>
        /// Gets the literal string representation of the token.
        /// </summary>
        public string Value { get; } = value;

        /// <summary>
        /// Gets the line number for diagnostics and debugging.
        /// </summary>
        public int Line { get; } = line;

        /// <summary>
        /// Gets the starting column position for diagnostics and debugging.
        /// </summary>
        public int Column { get; } = column;

        /// <summary>
        /// Returns a string representation of the token for debugging purposes.
        /// </summary>
        public override string ToString() => $"[{Line}:{Column}] {Type}: {Value}";
    }
}