using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a conditional control flow structure (if-else).
    /// It evaluates a condition and executes the corresponding branch.
    /// </summary>
    /// <param name="condition">The boolean expression to be evaluated.</param>
    /// <param name="thenBranch">The block of code to execute if the condition evaluates to true.</param>
    /// <param name="elseBranch">The optional block of code to execute if the condition evaluates to false.</param>
    public class IfNode(
        AstNode condition,
        BlockNode thenBranch,
        BlockNode? elseBranch) : AstNode
    {
        /// <summary>
        /// Gets the condition expression.
        /// </summary>
        public AstNode Condition { get; } = condition;

        /// <summary>
        /// Gets the "then" branch executed on a true result.
        /// </summary>
        public BlockNode ThenBranch { get; } = thenBranch;

        /// <summary>
        /// Gets the optional "else" branch executed on a false result.
        /// Returns null if no else branch was provided.
        /// </summary>
        public BlockNode? ElseBranch { get; } = elseBranch;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this node.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor.</typeparam>
        /// <param name="visitor">The visitor instance implementation.</param>
        /// <returns>A result of type T produced by the visitor.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}