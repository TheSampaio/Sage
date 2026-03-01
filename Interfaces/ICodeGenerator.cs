using Sage.Ast;

namespace Sage.Interfaces
{
    /// <summary>
    /// Defines the contract for the compiler's code generation backend.
    /// This component is responsible for transforming a validated Abstract Syntax Tree (AST) 
    /// into the final target source code.
    /// </summary>
    public interface ICodeGenerator
    {
        /// <summary>
        /// Generates target source code from a given program AST.
        /// </summary>
        /// <param name="ast">The root node of the validated Abstract Syntax Tree to be processed.</param>
        /// <param name="moduleName">The name assigned to the generated module or file.</param>
        /// <returns>A string containing the formatted source code in the target language.</returns>
        string Generate(ProgramNode ast, string moduleName);
    }
}