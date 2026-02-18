using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a standalone expression executed as a statement.
    /// </summary>
    public class ExpressionStatementNode(AstNode expression) : AstNode
    {
        public AstNode Expression { get; } = expression;
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}