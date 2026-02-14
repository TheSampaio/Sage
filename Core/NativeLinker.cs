using Sage.Utilities;

namespace Sage.Core
{
    /// <summary>
    /// Wrapper for the native linker (e.g., LINK.exe, LD).
    /// Combines object files into the final executable.
    /// </summary>
    public sealed class NativeLinker(string linkerPath = "link.exe")
    {
        private readonly string _linkerPath = linkerPath;

        public void Link(string objectDirectory, string outputExecutable, IEnumerable<string>? extraLibraries = null)
        {
            if (!Directory.Exists(objectDirectory))
                throw new DirectoryNotFoundException($"Object directory not found: {objectDirectory}");

            var objectFiles = Directory.GetFiles(objectDirectory, "*.obj");
            if (objectFiles.Length == 0)
                throw new InvalidOperationException("No object files found to link.");

            CompilerLogger.LogInfo("Linking object files...");

            string objList = string.Join(" ", objectFiles.Select(f => $"\"{f}\""));
            string libList = extraLibraries != null ? string.Join(" ", extraLibraries) : "";
            string args = $"{objList} /OUT:\"{outputExecutable}\" /nologo {libList}";

            try
            {
                ProcessExecutor.Run(_linkerPath, args);
                CompilerLogger.LogSuccess($"Binary generated successfully: {outputExecutable}");
            }
            catch (Exception ex)
            {
                CompilerLogger.LogFatal(ex);
                throw new Exception("Linking failed.", ex);
            }
        }
    }
}