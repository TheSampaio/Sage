public class CompilationEnvironment
{
    public string ProjectRoot { get; }
    public string SourceDir { get; }
    public string ObjDir { get; }
    public string BinDir { get; }
    public string StdDir { get; }

    public CompilationEnvironment(string root, string source, string obj, string bin, string std)
    {
        ProjectRoot = root;
        SourceDir = source;
        ObjDir = obj;
        BinDir = bin;
        StdDir = std;
    }
}