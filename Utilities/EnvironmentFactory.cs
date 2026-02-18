using System.Reflection;

namespace Sage.Utilities
{
    public static class EnvironmentFactory
    {
        public static CompilationEnvironment Create(CompilerConfig config)
        {
            string sourceDir = Path.GetDirectoryName(config.InputPath)!;

            // 1. Detect Project Root (If inside 'src', go up one level)
            string root = Path.GetFileName(sourceDir).Equals("src", StringComparison.OrdinalIgnoreCase)
                ? Directory.GetParent(sourceDir)!.FullName
                : sourceDir;

            string obj = Path.Combine(root, "obj");
            string bin = Path.Combine(root, "bin");

            // 2. Resolve Standard Library (std) Path
            // Priority A: "std" folder inside the target project (Local override)
            string std = Path.Combine(root, "std");

            if (!Directory.Exists(std))
            {
                // Priority B: "std" folder relative to the Compiler Executable (Release mode)
                string exePath = AppContext.BaseDirectory;
                std = Path.Combine(exePath, "std");

                // Priority C: "std" folder in Solution Root (Dev/Debug mode)
                // Walks up from /bin/Debug/net8.0/ to find the solution root containing "std"
                if (!Directory.Exists(std))
                {
                    var debugStd = FindStdUpwards(exePath);
                    if (debugStd != null) std = debugStd;
                }
            }

            // Ensure output directories exist
            Directory.CreateDirectory(obj);
            Directory.CreateDirectory(bin);

            return new CompilationEnvironment(root, sourceDir, obj, bin, std);
        }

        private static string? FindStdUpwards(string startPath)
        {
            DirectoryInfo? current = new DirectoryInfo(startPath);
            while (current != null)
            {
                string candidate = Path.Combine(current.FullName, "std");
                if (Directory.Exists(candidate)) return candidate;
                current = current.Parent;
            }
            return null;
        }
    }
}