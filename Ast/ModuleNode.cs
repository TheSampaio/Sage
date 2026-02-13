using Sage.Interfaces;

namespace Sage.Ast
{
    public class ModuleNode : AstNode
    {
        public string Name { get; }
        public List<FunctionDeclarationNode> Functions { get; } = new();

        public ModuleNode(string name) => Name = name;
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}
