using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a module definition in the Sage language. 
    /// Acts as a namespace container for a collection of function declarations.
    /// </summary>
    /// <param name="name">The identifier name of the module.</param>
    public class ModuleNode(string name) : AstNode
    {
        public string Name { get; } = name;

        // Lista unificada. Contém FunctionDeclarationNode e ExternBlockNode
        public List<AstNode> Members { get; } = [];

        // (Opcional) Helper para manter compatibilidade se algum código antigo ainda usar .Functions
        public IEnumerable<FunctionDeclarationNode> Functions => Members.OfType<FunctionDeclarationNode>();

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}