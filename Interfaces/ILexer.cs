using Sage.Core;

namespace Sage.Interfaces
{
    /// <summary>
    /// Defines the contract for the Lexical Analyzer (Scanner).
    /// Responsible for converting raw source text into a stream of meaningful tokens.
    /// </summary>
    public interface ILexer
    {
        /// <summary>
        /// Scans the input text and produces a list of tokens based on the Sage grammar.
        /// </summary>
        /// <returns>A list of <see cref="Token"/> objects identified during the scanning process.</returns>
        List<Token> Tokenize();
    }
}