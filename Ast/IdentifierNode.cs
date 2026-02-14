using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a named reference to a variable, parameter, or function within the source code.
    /// This node is used whenever an identifier is accessed in an expression.
    /// </summary>
    /// <param name="name">The name of the identifier as found in the source code.</param>
    public class IdentifierNode(string name) : AstNode
    {
        /// <summary>
        /// Gets the name of the identifier.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this identifier.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on this node.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}