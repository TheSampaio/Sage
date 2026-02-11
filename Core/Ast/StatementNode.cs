namespace Sage.Core.AST
{
    // Base class for instructions
    public abstract class StatementNode : AstNode { }

    // The root node that contains the entire file
    public class ProgramNode : AstNode
    {
        public List<StatementNode> Statements { get; } = new List<StatementNode>();
    }

    // Represents a block of code { ... }
    public class BlockNode : StatementNode
    {
        public List<StatementNode> Statements { get; } = new List<StatementNode>();
    }

    // Represents: use Console;
    public class UseNode : StatementNode
    {
        public string ModuleName { get; }
        public UseNode(string name) => ModuleName = name;
    }

    // Represents: func Main() -> i32 { ... }
    public class FunctionDeclarationNode : StatementNode
    {
        public string Name { get; }
        public string ReturnType { get; }
        public BlockNode Body { get; }

        // Future: Parameter List
        public FunctionDeclarationNode(string name, string returnType, BlockNode body)
        {
            Name = name;
            ReturnType = returnType;
            Body = body;
        }
    }

    // Represents: i32 x = 10;
    public class VariableDeclarationNode : StatementNode
    {
        public string Type { get; }
        public string Name { get; }
        public ExpressionNode Initializer { get; }

        public VariableDeclarationNode(string type, string name, ExpressionNode initializer)
        {
            Type = type;
            Name = name;
            Initializer = initializer;
        }
    }

    // Represents: return 0;
    public class ReturnNode : StatementNode
    {
        public ExpressionNode Expression { get; } // Can be null if return;
        public ReturnNode(ExpressionNode expr) => Expression = expr;
    }

    // TODO: Add this class to allow expressions like statements (Ex: Function calls)
    public class ExpressionStatementNode : StatementNode
    {
        public ExpressionNode Expression { get; }

        public ExpressionStatementNode(ExpressionNode expression)
        {
            Expression = expression;
        }
    }
}