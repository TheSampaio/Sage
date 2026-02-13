using Sage.Ast;

namespace Sage.Interfaces
{
    public interface IParser
    {
        ProgramNode Parse();
    }
}
