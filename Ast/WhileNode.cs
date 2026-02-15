using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a while loop structure. 
    /// The body is executed repeatedly as long as the condition evaluates to true.
    /// </summary>
    /// <param name="condition">The boolean expression evaluated before each iteration.</param>
    /// <param name="body">The block of code to execute while the condition is met.</param>
    public class WhileNode(AstNode condition, BlockNode body) : AstNode
    {
        /// <summary>
        /// Gets the condition expression that controls the loop execution.
        /// </summary>
        public AstNode Condition { get; } = condition;

        /// <summary>
        /// Gets the body of the loop.
        /// </summary>
        public BlockNode Body { get; } = body;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this node.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor.</typeparam>
        /// <param name="visitor">The visitor instance implementation.</param>
        /// <returns>A result of type T produced by the visitor.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}