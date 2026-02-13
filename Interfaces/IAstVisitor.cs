using Sage.Ast;

namespace Sage.Interfaces
{
    public interface IAstVisitor<T>
    {
        T Visit(ProgramNode node);
        T Visit(ModuleNode node);
        T Visit(FunctionDeclarationNode node);
        T Visit(BlockNode node);
        T Visit(VariableDeclarationNode node);
        T Visit(ReturnNode node);
        T Visit(ExpressionStatementNode node);
        T Visit(UseNode node);
        T Visit(BinaryExpressionNode node);
        T Visit(LiteralNode node);
        T Visit(IdentifierNode node);
        T Visit(FunctionCallNode node);
        T Visit(InterpolatedStringNode node);
    }
}
