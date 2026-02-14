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
        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the list of function declarations defined within this module's scope.
        /// </summary>
        public List<FunctionDeclarationNode> Functions { get; } = [];

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this module.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on this node.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}