using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a statement that consists of a single expression followed by a terminator.
    /// This node wraps expressions (like function calls or assignments) to be used in statement contexts.
    /// </summary>
    /// <param name="expression">The underlying expression contained within the statement.</param>
    public class ExpressionStatementNode(AstNode expression) : AstNode
    {
        /// <summary>
        /// Gets the expression that constitutes this statement.
        /// </summary>
        public AstNode Expression { get; } = expression;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this statement.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on this node.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}