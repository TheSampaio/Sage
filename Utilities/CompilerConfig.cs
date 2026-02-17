namespace Sage.Utilities
{
    /// <summary>
    /// Encapsulates the configuration and environmental context for a single compilation execution.
    /// </summary>
    /// <param name="inputPath">The absolute path to the .sg source file.</param>
    /// <param name="isDebugMode">If true, enables debug artifacts like .ast and .tok files.</param>
    /// <param name="buildNative">If true, invokes GCC to create an executable.</param>
    /// <param name="runOnSuccess">If true, runs the resulting executable immediately after linking.</param>
    public class CompilerConfig(string inputPath, bool isDebugMode, bool buildNative, bool runOnSuccess)
    {
        /// <summary>
        /// Gets the full path to the source file being compiled.
        /// </summary>
        public string InputPath { get; } = inputPath;

        /// <summary>
        /// Gets the base path used for all output files (.c, .ast, .tok).
        /// </summary>
        public string OutputBaseName { get; } = inputPath;

        /// <summary>
        /// Gets a value indicating whether the compiler should output diagnostic files.
        /// </summary>
        public bool IsDebugMode { get; } = isDebugMode;

        /// <summary>
        /// If true, attempts to invoke the native C compiler (cl.exe/gcc) after transpilation.
        /// </summary>
        public bool BuildNative { get; } = buildNative;

        /// <summary>
        /// If true, the compiler will execute the binary after a successful build.
        /// </summary>
        public bool RunOnSuccess { get; } = runOnSuccess;
    }
}