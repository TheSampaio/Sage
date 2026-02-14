using Sage.Core;
using System.Text;

namespace Sage.Utilities
{
    /// <summary>
    /// Utility for converting a list of tokens into a human-readable string format.
    /// </summary>
    public static class TokenPrinter
    {
        /// <summary>
        /// Formats a list of tokens into a readable string.
        /// </summary>
        public static string Print(IEnumerable<Token> tokens)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{"Type",-20} | {"Lexeme",-15} | {"Value",-15}");
            sb.AppendLine(new string('-', 55));

            foreach (var token in tokens)
            {
                // Formats each token as a row in a table
                sb.AppendLine($"{token.Type,-20} | {token.Value,-15} | {token.Value ?? "null",-15}");
            }

            return sb.ToString();
        }
    }
}