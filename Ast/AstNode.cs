using Sage.Interfaces;

namespace Sage.Ast
{
    public abstract class AstNode
    {
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }
}
