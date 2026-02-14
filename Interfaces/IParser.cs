using Sage.Ast;

namespace Sage.Interfaces
{
    /// <summary>
    /// Defines the contract for the Syntax Analyzer (Parser).
    /// Responsible for converting a stream of tokens into a structured Abstract Syntax Tree (AST).
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Analyzes the token stream and constructs the root <see cref="ProgramNode"/> 
        /// representing the hierarchy of the source code.
        /// </summary>
        /// <returns>A <see cref="ProgramNode"/> containing the full AST of the program.</returns>
        /// <exception cref="System.Exception">Thrown when a syntax error is encountered during parsing.</exception>
        ProgramNode Parse();
    }
}