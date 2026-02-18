using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a conditional branching structure.
    /// </summary>
    /// <param name="condition">The boolean expression evaluated to determine branch execution.</param>
    /// <param name="thenBranch">The block executed if the condition is true.</param>
    /// <param name="elseBranch">The optional block or nested if statement executed if the condition is false.</param>
    public class IfNode(AstNode condition, BlockNode thenBranch, AstNode? elseBranch) : AstNode
    {
        public AstNode Condition { get; } = condition;
        public BlockNode ThenBranch { get; } = thenBranch;

        /// <summary>
        /// Can be a BlockNode (final else) or another IfNode (else if).
        /// </summary>
        public AstNode? ElseBranch { get; } = elseBranch;

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}