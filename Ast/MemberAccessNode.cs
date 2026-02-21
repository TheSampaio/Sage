using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a member access expression in the AST,
    /// such as accessing a property or field from an object (e.g., obj.property).
    /// </summary>
    /// <param name="objectNode">
    /// The expression representing the target object whose member is being accessed.
    /// </param>
    /// <param name="propertyName">
    /// The name of the member (field or property) being accessed.
    /// </param>
    public class MemberAccessNode(AstNode objectNode, string propertyName) : AstNode
    {
        /// <summary>
        /// Gets the AST node representing the object instance.
        /// </summary>
        public AstNode Object { get; } = objectNode;

        /// <summary>
        /// Gets the name of the member being accessed.
        /// </summary>
        public string PropertyName { get; } = propertyName;

        /// <summary>
        /// Accepts a visitor and dispatches execution to the corresponding visit method.
        /// </summary>
        /// <typeparam name="T">The return type produced by the visitor.</typeparam>
        /// <param name="visitor">The visitor instance processing this node.</param>
        /// <returns>The result of the visitor's operation.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}