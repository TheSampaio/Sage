namespace Sage.Core
{
    public class SymbolMetadata
    {
        public string Type { get; }
        public bool IsFunction { get; }
        public bool IsExtern { get; }

        public SymbolMetadata(string type, bool isFunction, bool isExtern)
        {
            Type = type;
            IsFunction = isFunction;
            IsExtern = isExtern;
        }
    }
}