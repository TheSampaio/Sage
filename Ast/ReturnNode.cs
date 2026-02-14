using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a return statement within a function.
    /// It specifies the expression whose value is sent back to the caller 
    /// and signals the termination of the current function's execution.
    /// </summary>
    /// <param name="expression">The expression node to be evaluated and returned.</param>
    public class ReturnNode(AstNode expression) : AstNode
    {
        /// <summary>
        /// Gets the expression being returned by the statement.
        /// </summary>
        public AstNode Expression { get; } = expression;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this return statement.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on this node.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}