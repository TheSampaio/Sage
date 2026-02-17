using System.Text.Json;
using System.Text.Json.Serialization;
using Sage.Core;

namespace Sage.Utilities
{
    /// <summary>
    /// Utility for converting a list of tokens into a standardized JSON format.
    /// This is useful for external tooling, debugging, and IDE integration.
    /// </summary>
    public static class TokenPrinter
    {
        // FIX: Cache the options instance to avoid performance hit (CA1869)
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Serializes a collection of tokens into an indented JSON string.
        /// </summary>
        /// <param name="tokens">The stream of tokens produced by the Lexer.</param>
        /// <returns>A formatted JSON string representing the token data.</returns>
        public static string Print(IEnumerable<Token> tokens)
        {
            return JsonSerializer.Serialize(tokens, _options);
        }
    }
}