using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents an explicit type conversion expression (e.g., "value as i32").
    /// </summary>
    /// <param name="expression">The expression to be cast.</param>
    /// <param name="targetType">The destination Sage type.</param>
    public class CastExpressionNode(AstNode expression, string targetType) : AstNode
    {
        public AstNode Expression { get; } = expression;
        public string TargetType { get; } = targetType;

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}