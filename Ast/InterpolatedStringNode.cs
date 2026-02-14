using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a string literal that contains embedded expressions to be evaluated 
    /// and concatenated at runtime (e.g., "{name} is {age} years old").
    /// </summary>
    public class InterpolatedStringNode : AstNode
    {
        /// <summary>
        /// Gets the sequence of parts comprising the interpolated string.
        /// These may include <see cref="LiteralNode"/> for raw text and 
        /// various expression nodes for the interpolated values.
        /// </summary>
        public List<AstNode> Parts { get; } = [];

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this interpolated string.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on this node.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}