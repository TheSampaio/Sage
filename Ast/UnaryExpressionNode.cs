using Sage.Enums;
using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents an expression with a single operator and one operand.
    /// Handles prefix operations (e.g., -x, !flag) and postfix operations (e.g., i++).
    /// </summary>
    /// <param name="operatorType">The token type representing the unary operator.</param>
    /// <param name="operand">The expression the operator is applied to.</param>
    /// <param name="isPostfix">Indicates if the operator appears after the operand.</param>
    public class UnaryExpressionNode(
        TokenType operatorType,
        AstNode operand,
        bool isPostfix = false) : AstNode
    {
        /// <summary>
        /// Gets the operator type for this expression.
        /// </summary>
        public TokenType Operator { get; } = operatorType;

        /// <summary>
        /// Gets the operand expression.
        /// </summary>
        public AstNode Operand { get; } = operand;

        /// <summary>
        /// Gets a value indicating whether this is a postfix operation.
        /// If false, it is treated as a prefix operation.
        /// </summary>
        public bool IsPostfix { get; } = isPostfix;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this node.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor.</typeparam>
        /// <param name="visitor">The visitor instance implementation.</param>
        /// <returns>A result of type T produced by the visitor.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}