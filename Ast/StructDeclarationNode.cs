using Sage.Interfaces;

namespace Sage.Ast
{
    public class StructDeclarationNode(string name, List<VariableDeclarationNode> fields) : AstNode
    {
        public string Name { get; } = name;
        public List<VariableDeclarationNode> Fields { get; } = fields;

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}