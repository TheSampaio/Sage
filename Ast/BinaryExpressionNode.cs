using Sage.Enums;
using Sage.Interfaces;

namespace Sage.Ast
{
    public class BinaryExpressionNode : AstNode
    {
        public AstNode Left { get; }
        public TokenType Operator { get; }
        public AstNode Right { get; }
        public BinaryExpressionNode(AstNode l, TokenType op, AstNode r) { Left = l; Operator = op; Right = r; }
        public override T Accept<T>(IAstVisitor<T> v) => v.Visit(this);
    }
}
