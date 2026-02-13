using Sage.Interfaces;

namespace Sage.Ast
{
    public class InterpolatedStringNode : AstNode
    {
        public List<AstNode> Parts { get; } = new();
        public override T Accept<T>(IAstVisitor<T> v) => v.Visit(this);
    }
}
