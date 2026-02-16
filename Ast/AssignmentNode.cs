using Sage.Interfaces;

namespace Sage.Ast
{
    /// <summary>
    /// Represents an assignment operation where a value is assigned to a named identifier.
    /// Example: x = 10 + 5;
    /// </summary>
    /// <param name="name">The name of the target identifier.</param>
    /// <param name="expression">The expression being assigned to the target.</param>
    public class AssignmentNode(string name, AstNode expression) : AstNode
    {
        /// <summary>
        /// Gets the name of the variable or identifier receiving the value.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the expression tree that evaluates to the assigned value.
        /// </summary>
        public AstNode Expression { get; } = expression;

        /// <summary>
        /// Gets or sets the resolved type of the variable. 
        /// Populated during Semantic Analysis to aid Code Generation (e.g., for casting).
        /// </summary>
        public string? VariableType { get; set; }

        /// <summary>
        /// Dispatches the visitor to the appropriate visit method for this node.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor.</typeparam>
        /// <param name="visitor">The visitor instance implementation.</param>
        /// <returns>A result of type T produced by the visitor.</returns>
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}