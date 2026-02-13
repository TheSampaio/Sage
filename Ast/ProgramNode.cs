using Sage.Interfaces;

namespace Sage.Ast
{
    public class ProgramNode : AstNode
    {
        public List<AstNode> Statements { get; } = new();
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}
