using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a C-style for loop structure: for (initializer; condition; increment) { body }.
    /// This node supports optional components for infinite loops or omitted expressions.
    /// </summary>
    /// <param name="initializer">The initial statement or expression executed before the loop starts.</param>
    /// <param name="condition">The boolean expression evaluated before each iteration.</param>
    /// <param name="increment">The expression executed after each loop iteration.</param>
    /// <param name="body">The block of code to be executed within the loop.</param>
    public class ForNode(
        AstNode? initializer,
        AstNode? condition,
        AstNode? increment,
        BlockNode body) : AstNode
    {
        /// <summary>
        /// Gets the loop initialization node. Usually a variable declaration or assignment.
        /// </summary>
        public AstNode? Initializer { get; } = initializer;

        /// <summary>
        /// Gets the loop continuation condition. If null, the loop defaults to true.
        /// </summary>
        public AstNode? Condition { get; } = condition;

        /// <summary>
        /// Gets the loop increment or update expression.
        /// </summary>
        public AstNode? Increment { get; } = increment;

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