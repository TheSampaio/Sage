using Sage.Enums;

namespace Sage.Core.AST
{
    // Base class for expressions
    public abstract class ExpressionNode : AstNode { }

    // Represents numbers, strings, booleans (Ex: 5, "Hello")
    public class LiteralNode : ExpressionNode
    {
        public object Value { get; }
        public string TypeName { get; } // "i32", "string"

        public LiteralNode(object value, string typeName)
        {
            Value = value;
            TypeName = typeName;
        }
    }

    // Represents the use of a variable (Ex: number01)
    public class IdentifierNode : ExpressionNode
    {
        public string Name { get; }
        public IdentifierNode(string name) => Name = name;
    }

    // Represents binary operations (Ex: a + b, x * y)
    public class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; }
        public TokenType Operator { get; }
        public ExpressionNode Right { get; }

        public BinaryExpressionNode(ExpressionNode left, TokenType op, ExpressionNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }

    // Represents function call (Ex: Console::Print("Hello"))
    public class FunctionCallNode : ExpressionNode
    {
        public string FunctionName { get; }
        public List<ExpressionNode> Arguments { get; }

        public FunctionCallNode(string name, List<ExpressionNode> args)
        {
            FunctionName = name;
            Arguments = args ?? new List<ExpressionNode>();
        }
    }
}