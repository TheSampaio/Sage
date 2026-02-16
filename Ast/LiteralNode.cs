using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a constant value of a specific type within the source code, 
    /// such as an integer, a floating-point number, or a string.
    /// </summary>
    /// <param name="value">The actual data value stored in the node.</param>
    /// <param name="typeName">The Sage type associated with this literal (e.g., "i32", "f32", "string").</param>
    public class LiteralNode(object value, string typeName) : AstNode
    {
        /// <summary>
        /// Gets the constant value of this node.
        /// </summary>
        public object Value { get; } = value;

        /// <summary>
        /// Gets or sets the Sage type name.
        /// Mutable to allow the Semantic Analyzer to refine the type (e.g., i32 -> f32).
        /// </summary>
        public string TypeName { get; set; } = typeName;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this literal.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on this node.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}