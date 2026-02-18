using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents an explicit type conversion expression.
    /// </summary>
    /// <param name="expression">The expression to be cast.</param>
    /// <param name="targetType">The destination type name.</param>
    public class CastExpressionNode(AstNode expression, string targetType) : AstNode
    {
        /// <summary>Gets the expression being converted.</summary>
        public AstNode Expression { get; } = expression;

        /// <summary>Gets the target type name for the conversion.</summary>
        public string TargetType { get; } = targetType;

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}