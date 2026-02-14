namespace Sage.Ast
{
    /// <summary>
    /// Represents a formal parameter definition within a function signature.
    /// Stores the parameter's identifier and its associated Sage data type.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="type">The Sage type name for the parameter (e.g., "i32", "f64").</param>
    public class ParameterNode(string name, string type)
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the Sage data type associated with this parameter.
        /// </summary>
        public string Type { get; } = type;

        /// <summary>
        /// Returns a string representation of the parameter for debugging.
        /// </summary>
        public override string ToString() => $"{Name}: {Type}";
    }
}