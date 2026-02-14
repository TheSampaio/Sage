using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a 'use' directive, which imports an external module or library 
    /// into the current source file scope.
    /// </summary>
    /// <param name="module">The name of the module to be imported.</param>
    public class UseNode(string module) : AstNode
    {
        /// <summary>
        /// Gets the name of the module identifier being imported.
        /// </summary>
        public string Module { get; } = module;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this directive.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on this node.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}