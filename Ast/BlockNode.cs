using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a collection of statements enclosed within a scope, 
    /// typically defined by curly braces {}.
    /// </summary>
    public class BlockNode : AstNode
    {
        /// <summary>
        /// Gets the list of AST nodes (statements or declarations) contained within this block.
        /// </summary>
        public List<AstNode> Statements { get; } = [];

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this block.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on this node.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}