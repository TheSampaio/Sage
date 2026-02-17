namespace Sage.Core
{
    public class SymbolTable
    {
        private readonly Stack<Dictionary<string, SymbolMetadata>> _scopes = new();

        public SymbolTable() { EnterScope(); }

        public void EnterScope() => _scopes.Push(new Dictionary<string, SymbolMetadata>());

        public void ExitScope() { if (_scopes.Count > 1) _scopes.Pop(); }

        // Agora definimos se é função e se é externa
        public void Define(string name, string type, bool isFunction = false, bool isExtern = false)
        {
            if (_scopes.Count > 0)
            {
                _scopes.Peek()[name] = new SymbolMetadata(type, isFunction, isExtern);
            }
        }

        public SymbolMetadata? Resolve(string name)
        {
            foreach (var scope in _scopes)
            {
                if (scope.TryGetValue(name, out var metadata)) return metadata;
            }
            return null;
        }

        public bool IsDefinedInCurrentScope(string name) => _scopes.Count > 0 && _scopes.Peek().ContainsKey(name);
    }
}