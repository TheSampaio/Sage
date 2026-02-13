using Sage.Core;

namespace Sage.Interfaces
{
    public interface ILexer
    {
        List<Token> Tokenize();
    }
}
