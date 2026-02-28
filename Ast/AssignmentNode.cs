using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents an assignment operation where a value is assigned to a target expression.
    /// Example: x = 10; cars[0] = "Astra"; obj.prop = 5;
    /// </summary>
    public class AssignmentNode(AstNode target, AstNode expression) : AstNode
    {
        public AstNode Target { get; } = target;

        /// <summary>
        /// Gets the expression tree that evaluates to the assigned value.
        /// </summary>
        public AstNode Expression { get; } = expression;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this node.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor.</typeparam>
        /// <param name="visitor">The visitor instance implementation.</param>
        /// <returns>A result of type T produced by the visitor.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}