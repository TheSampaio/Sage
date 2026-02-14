namespace Sage.Core
{
    /// <summary>
    /// Manages nested identifier scopes using a stack-based approach.
    /// Supports scoping rules for variables, allowing for global and local visibility.
    /// </summary>
    public class SymbolTable
    {
        /// <summary>
        /// A stack of scopes, where each scope is a set of defined identifier names.
        /// </summary>
        private readonly Stack<HashSet<string>> _scopes = new();

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
        public void EnterScope() => _scopes.Push([]);

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
        /// Defines a new identifier within the current active scope.
        /// </summary>
        /// <param name="name">The identifier name to define.</param>
        public void Define(string name)
        {
            if (_scopes.Count > 0)
                _scopes.Peek().Add(name);
        }

        /// <summary>
        /// Attempts to resolve an identifier by searching from the innermost scope outward to the global scope.
        /// </summary>
        /// <param name="name">The identifier name to resolve.</param>
        /// <returns>True if the identifier is found in any accessible scope; otherwise, false.</returns>
        public bool Resolve(string name)
        {
            foreach (var scope in _scopes)
            {
                if (scope.Contains(name))
                    return true;
            }
            return false;
        }
    }
}