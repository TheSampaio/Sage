using Sage.Ast;

namespace Sage.Interfaces
{
    public interface ICodeGenerator
    {
        string Generate(ProgramNode ast);
    }
}
