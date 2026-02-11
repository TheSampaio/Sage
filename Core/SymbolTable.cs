namespace Sage.Core
{
    public class SymbolInfo(string name, string type)
    {
        public string Name { get; } = name;
        public string Type { get; } = type;
    }

    public class SymbolTable
    {
        // Stack of scopes. Each scope is a dictionary of variable names to their info.
        private readonly Stack<Dictionary<string, SymbolInfo>> _scopes = [];

        public SymbolTable()
        {
            // Initialize with a global scope
            EnterScope();
        }

        public void EnterScope()
        {
            _scopes.Push([]);
        }

        public void ExitScope()
        {
            if (_scopes.Count > 0)
                _scopes.Pop();
        }

        public void Define(string name, string type)
        {
            var currentScope = _scopes.Peek();

            if (currentScope.ContainsKey(name))
            {
                throw new Exception($"[SEMANTIC ERROR] Variable '{name}' is already defined in this scope.");
            }

            currentScope[name] = new SymbolInfo(name, type);
        }

        public SymbolInfo? Resolve(string name)
        {
            // Look for the variable starting from the current scope down to the global scope
            foreach (var scope in _scopes)
            {
                if (scope.TryGetValue(name, out var info))
                {
                    return info;
                }
            }

            return null; // Not found
        }
    }
}