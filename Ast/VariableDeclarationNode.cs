using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents a variable or constant declaration within the Sage language.
    /// This node captures the type, identifier name, initialization expression, 
    /// and the mutability state (var vs. const).
    /// </summary>
    /// <param name="type">The Sage data type (e.g., "i32", "b8", "f64").</param>
    /// <param name="name">The unique identifier name for the variable or constant.</param>
    /// <param name="initializer">The expression node providing the mandatory initial value.</param>
    /// <param name="isConstant">True if the declaration is immutable (const), false if mutable (var).</param>
    public class VariableDeclarationNode(
        string type,
        string name,
        AstNode initializer,
        bool isConstant) : AstNode
    {
        /// <summary>
        /// Gets the declared Sage data type.
        /// </summary>
        public string Type { get; } = type;

        /// <summary>
        /// Gets the identifier name assigned to the variable or constant.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the expression tree that initializes the variable.
        /// Note: In Sage, all declarations require an explicit initializer.
        /// </summary>
        public AstNode Initializer { get; } = initializer;

        /// <summary>
        /// Gets a value indicating whether this declaration is a constant.
        /// Constants cannot be reassigned after their initial declaration.
        /// </summary>
        public bool IsConstant { get; } = isConstant;

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this node.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor.</typeparam>
        /// <param name="visitor">The visitor instance implementation.</param>
        /// <returns>A result of type T produced by the visitor.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}