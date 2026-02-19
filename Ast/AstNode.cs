using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents the base class for all nodes in the Sage Abstract Syntax Tree (AST).
    /// Implements the element side of the Visitor pattern to allow for decoupled operations
    /// such as code generation, semantic analysis, and tree printing.
    /// </summary>
    public abstract class AstNode
    {
        /// <summary>
        /// Gets the line number in the source code where this node starts.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets the column number in the source code where this node starts.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Gets or sets the resolved type of this node.
        /// Populated during Semantic Analysis to aid Code Generation.
        /// </summary>
        public string? VariableType { get; set; }

        /// <summary>
        /// Dispatches the node to the appropriate visit method on the provided visitor.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on this node.</returns>
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }
}