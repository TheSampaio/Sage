using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents an FFI block declaring external C functions and structs.
    /// Example: extern stdio("stdio.h") { ... }
    /// </summary>
    public class ExternBlockNode(string alias, string header, List<AstNode> declarations) : AstNode
    {
        public string Alias { get; } = alias;
        public string Header { get; } = header;
        public List<AstNode> Declarations { get; } = declarations;

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}