using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a variable declaration statement within the Sage language.
    /// Defines the variable's name, its static type, and an mandatory initial value.
    /// </summary>
    /// <param name="type">The Sage data type of the variable (e.g., "i32", "string").</param>
    /// <param name="name">The identifier name assigned to the variable.</param>
    /// <param name="initializer">The expression node that provides the initial value.</param>
    public class VariableDeclarationNode(string type, string name, AstNode initializer) : AstNode
    {
        /// <summary>
        /// Gets the Sage data type for this variable.
        /// </summary>
        public string Type { get; } = type;

        /// <summary>
        /// Gets the identifier name of the variable.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the expression used to initialize the variable.
        /// </summary>
        public AstNode Initializer { get; } = initializer;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this variable declaration.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor's operation.</typeparam>
        /// <param name="visitor">An implementation of <see cref="IAstVisitor{T}"/>.</param>
        /// <returns>The result of the visitor's operation on this node.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}