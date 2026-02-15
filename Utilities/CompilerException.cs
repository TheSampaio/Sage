using Sage.Core;

namespace Sage.Utilities
{
    /// <summary>
    /// Represents a specialized exception thrown during the compilation process (Lexing, Parsing, or Semantic Analysis).
    /// Provides metadata about the specific token and error code to facilitate debugging.
    /// </summary>
    /// <param name="token">The token where the error was detected. Can be null if the error is global.</param>
    /// <param name="code">A standardized Sage error code (e.g., "S001", "S105").</param>
    /// <param name="message">A human-readable description of the error.</param>
    public class CompilerException(Token? token, string code, string message) : Exception(message)
    {
        /// <summary>
        /// Gets the token that caused the compilation error.
        /// Includes line and column information for error reporting.
        /// </summary>
        public Token? OffendingToken { get; } = token;

        /// <summary>
        /// Gets the standardized Sage error code.
        /// </summary>
        public string ErrorCode { get; } = code;

        /// <summary>
        /// Formats the exception into a professional compiler error string.
        /// </summary>
        /// <returns>A formatted string: Error [Code] at line [Line]: [Message]</returns>
        public override string ToString()
        {
            if (OffendingToken != null)
            {
                return $"error {ErrorCode}: {Message} at line {OffendingToken.Line}, column {OffendingToken.Column} ('{OffendingToken.Value}')";
            }
            return $"error {ErrorCode}: {Message}";
        }
    }
}