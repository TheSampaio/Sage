using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents an array literal initialization. Example: { 0, 3, 1, 5 }
    /// </summary>
    public class ArrayInitializationNode(List<AstNode> elements) : AstNode
    {
        public List<AstNode> Elements { get; } = elements;
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}