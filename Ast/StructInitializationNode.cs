using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents the instantiation of a struct using an object literal syntax.
    /// Example: { name = "Alice", age = 30 }
    /// </summary>
    public class StructInitializationNode(Dictionary<string, AstNode> fields) : AstNode
    {
        public Dictionary<string, AstNode> Fields { get; } = fields;

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}