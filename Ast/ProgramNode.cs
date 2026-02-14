using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents the root node of a Sage source file.
    /// This is the entry point for the Abstract Syntax Tree (AST) and contains 
    /// the top-level statements, modules, and function declarations of the program.
    /// </summary>
    public class ProgramNode : AstNode
    {
        /// <summary>
        /// Gets the collection of top-level statements or declarations that constitute the program.
        /// </summary>
        public List<AstNode> Statements { get; } = [];

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for the entire program.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on the program root.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}