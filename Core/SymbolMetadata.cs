namespace Sage.Core
{
    /// <summary>
    /// Encapsulates the essential metadata for an identifier stored in the <see cref="SymbolTable"/>.
    /// This includes its data type, classification as a function or variable, and its linkage (internal vs. external).
    /// </summary>
    /// <param name="type">The Sage data type name (e.g., "i32", "f64").</param>
    /// <param name="isFunction">Indicates whether the symbol represents a callable function.</param>
    /// <param name="isExtern">Indicates whether the symbol is an external C function (C interop).</param>
    public class SymbolMetadata(string type, bool isFunction, bool isExtern)
    {
        /// <summary>
        /// Gets the Sage data type associated with this symbol.
        /// </summary>
        public string Type { get; } = type;

        /// <summary>
        /// Gets a value indicating whether this symbol is a function.
        /// If false, the symbol is treated as a variable or constant.
        /// </summary>
        public bool IsFunction { get; } = isFunction;

        /// <summary>
        /// Gets a value indicating whether this symbol refers to an external implementation.
        /// External symbols are primarily used for C standard library interop.
        /// </summary>
        public bool IsExtern { get; } = isExtern;
    }
}