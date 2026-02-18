namespace Sage.Utilities
{
    public class CompilerConfig
    {
        public string InputPath { get; }
        public bool IsDebugMode { get; }
        public bool BuildNative { get; }
        public bool RunOnSuccess { get; }

        public CompilerConfig(string inputPath, bool isDebugMode, bool buildNative, bool runOnSuccess)
        {
            InputPath = inputPath;
            IsDebugMode = isDebugMode;
            BuildNative = buildNative;
            RunOnSuccess = runOnSuccess;
        }
    }
}