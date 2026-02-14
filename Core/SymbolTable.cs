using System.Collections.Generic;

namespace Sage.Core
{
    /// <summary>
    /// Manages nested identifier scopes using a stack-based approach.
    /// Supports scoping rules for variables, allowing for global and local visibility.
    /// </summary>
    public class SymbolTable
    {
        /// <summary>
        /// A stack of scopes, where each scope is a dictionary mapping identifier names to their associated Sage types.
        /// </summary>
        private readonly Stack<Dictionary<string, string>> _scopes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolTable"/> class and enters the global scope.
        /// </summary>
        public SymbolTable()
        {
            EnterScope();
        }

        /// <summary>
        /// Pushes a new scope level onto the stack.
        /// </summary>
        public void EnterScope() => _scopes.Push(new Dictionary<string, string>());

        /// <summary>
        /// Removes the current scope level from the stack.
        /// Prevents the removal of the final global scope level.
        /// </summary>
        public void ExitScope()
        {
            if (_scopes.Count > 1)
                _scopes.Pop();
        }

        /// <summary>
        /// Defines a new identifier along with its associated Sage type within the current active scope.
        /// </summary>
        /// <param name="name">The unique identifier name to define in the current scope.</param>
        /// <param name="type">The Sage data type associated with this identifier (e.g., "i32", "f64", "str").</param>
        public void Define(string name, string type)
        {
            if (_scopes.Count > 0)
            {
                _scopes.Peek()[name] = type;
            }
        }

        /// <summary>
        /// Attempts to resolve an identifier's type by searching from the innermost scope outward to the global scope.
        /// </summary>
        /// <param name="name">The name of the identifier to search for.</param>
        /// <returns>A string representing the identifier's Sage type if found; otherwise, returns <c>null</c>.</returns>
        public string? Resolve(string name)
        {
            foreach (var scope in _scopes)
            {
                if (scope.TryGetValue(name, out var type))
                {
                    return type;
                }
            }
            return null;
        }
    }
}