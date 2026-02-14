using Sage.Ast;

namespace Sage.Interfaces
{
    /// <summary>
    /// Defines the contract for the compiler's backend.
    /// Responsible for transforming a validated Abstract Syntax Tree (AST) into target source code.
    /// </summary>
    public interface ICodeGenerator
    {
        /// <summary>
        /// Translates the provided AST into a string representation of the target language (e.g., C).
        /// </summary>
        /// <param name="ast">The root node of the program to be generated.</param>
        /// <returns>A string containing the generated source code.</returns>
        string Generate(ProgramNode ast);
    }
}