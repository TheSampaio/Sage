using System;
using System.IO;
using Sage.Utilities;

namespace Sage.Core
{
    /// <summary>
    /// Wrapper for the system's native C compiler (GCC).
    /// Handles the transformation of generated C code into a final executable binary.
    /// </summary>
    public sealed class NativeCompiler
    {
        private readonly string _compilerCommand;

        public NativeCompiler(string compilerCommand = "gcc")
        {
            _compilerCommand = compilerCommand;
        }

        /// <summary>
        /// Compiles a C source file directly to an executable.
        /// </summary>
        /// <param name="inputCFile">The path to the source .c file (usually in obj/).</param>
        /// <param name="outputExePath">The desired path for the resulting .exe file (usually in bin/).</param>
        /// <param name="optimize">Whether to apply compiler optimizations.</param>
        public void CompileToExecutable(string inputCFile, string outputExePath, bool optimize = true)
        {
            if (!File.Exists(inputCFile))
            {
                throw new FileNotFoundException($"Native source file not found: {inputCFile}");
            }

            // Ensure the output directory exists (double check safety)
            string? outputDir = Path.GetDirectoryName(outputExePath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Construct GCC arguments:
            // input.c    : Source file
            // -o output  : Output executable name
            // -std=c11   : Enforce modern C standards
            // -O2        : Optimization level 2
            // -w         : Suppress warnings
            string optimizationFlag = optimize ? "-O2" : "-O0";
            string args = $"\"{inputCFile}\" -o \"{outputExePath}\" -std=c11 {optimizationFlag} -w";

            CompilerLogger.LogInfo($"Invoking native toolchain: {_compilerCommand}");

            try
            {
                ProcessExecutor.Run(_compilerCommand, args);
                CompilerLogger.LogSuccess($"Native binary successfully built: {outputExePath}");
            }
            catch (Exception)
            {
                CompilerLogger.LogError("Native compilation failed. Please check if GCC is installed and in your PATH.");
                throw;
            }
        }
    }
}