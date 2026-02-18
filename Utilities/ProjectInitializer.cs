using System.Text.RegularExpressions;

namespace Sage.Utilities;

/// <summary>
/// Handles the scaffolding of new Sage projects.
/// </summary>
public static class ProjectInitializer
{
    private const string MainTemplate = @"use console;

func main(): none
{
    console::print_line(""Hello, World!"");
}
";

    /// <summary>
    /// Creates a new directory structure and default files for a Sage project.
    /// </summary>
    /// <param name="projectName">The name of the project to create.</param>
    public static void Initialize(string projectName)
    {
        if (!ValidateProjectName(projectName)) return;

        string currentDir = Directory.GetCurrentDirectory();
        string projectDir = Path.Combine(currentDir, projectName);

        if (Directory.Exists(projectDir))
        {
            CompilerLogger.LogError($"Directory '{projectName}' already exists.");
            return;
        }

        try
        {
            CompilerLogger.LogStep($"Creating project '{projectName}'...");

            CreateDirectoryStructure(projectDir);
            CreateProjectFiles(projectDir, projectName);

            CompilerLogger.LogSuccess($"Project '{projectName}' initialized.");

            // User Instructions
            Console.WriteLine();
            CompilerLogger.LogInfo($"Next steps:");
            Console.WriteLine($"  cd {projectName}");
            Console.WriteLine("  sage run");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            CompilerLogger.LogFatal(ex);
        }
    }

    private static bool ValidateProjectName(string name)
    {
#pragma warning disable
        if (Regex.IsMatch(name, @"^[a-zA-Z0-9_-]+$")) return true;
#pragma warning restore

        CompilerLogger.LogError($"Invalid name '{name}'. Use letters, numbers, underscores or dashes.");
        return false;
    }

    private static void CreateDirectoryStructure(string projectDir)
    {
        Directory.CreateDirectory(projectDir);
        Directory.CreateDirectory(Path.Combine(projectDir, "src"));
    }

    private static void CreateProjectFiles(string projectDir, string projectName)
    {
        File.WriteAllText(Path.Combine(projectDir, "src", "main.sg"), MainTemplate);
        File.WriteAllText(Path.Combine(projectDir, "README.md"), $"# {projectName}\n\nCreated with Sage.");
        File.WriteAllText(Path.Combine(projectDir, ".gitignore"), "/bin\n/obj\n.vs\n.vscode");
    }
}