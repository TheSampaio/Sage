using Sage.Utilities;

namespace Sage.Core
{
    /// <summary>
    /// Provides a wrapper for the system's native linker (e.g., LINK.exe for MSVC or LD for GCC).
    /// Responsible for combining compiled object files and external libraries into a final executable binary.
    /// </summary>
    /// <param name="linkerPath">The path or name of the linker executable. Defaults to "link.exe".</param>
    public sealed class NativeLinker(string linkerPath = "link.exe")
    {
        private readonly string _linkerPath = linkerPath;

        /// <summary>
        /// Invokes the native linker to produce an executable from object files.
        /// </summary>
        /// <param name="objectDirectory">The directory containing the <c>.obj</c> files to be linked.</param>
        /// <param name="outputExecutable">The absolute path where the final executable should be created.</param>
        /// <param name="extraLibraries">Optional collection of additional library files (<c>.lib</c>) to include in the link process.</param>
        /// <exception cref="DirectoryNotFoundException">Thrown if the specified object directory does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no object files are found in the target directory.</exception>
        /// <exception cref="Exception">Thrown if the linking process fails.</exception>
        public void Link(string objectDirectory, string outputExecutable, IEnumerable<string>? extraLibraries = null)
        {
            if (!Directory.Exists(objectDirectory))
            {
                throw new DirectoryNotFoundException($"Object directory not found: {objectDirectory}");
            }

            // Collect all object files in the directory
            var objectFiles = Directory.GetFiles(objectDirectory, "*.obj");
            if (objectFiles.Length == 0)
            {
                throw new InvalidOperationException("No object files found to link. Ensure the compiler backend has finished successfully.");
            }

            CompilerLogger.LogInfo("Linking object files...");

            // Prepare the argument string for the linker
            string objList = string.Join(" ", objectFiles.Select(f => $"\"{f}\""));
            string libList = extraLibraries != null ? string.Join(" ", extraLibraries) : "";

            // Constructing the command line: 
            // /OUT specifies the target; /nologo suppresses the startup banner for cleaner output.
            string args = $"{objList} /OUT:\"{outputExecutable}\" /nologo {libList}";

            try
            {
                // Delegate the actual OS execution to the ProcessExecutor
                ProcessExecutor.Run(_linkerPath, args);
                CompilerLogger.LogSuccess($"Binary generated successfully: {outputExecutable}");
            }
            catch (Exception ex)
            {
                // Log the failure to the console and wrap the exception for higher-level handling
                CompilerLogger.LogFatal(ex);
                throw new Exception("Linking failed. Verify that the linker is in your PATH and object files are compatible.", ex);
            }
        }
    }
}