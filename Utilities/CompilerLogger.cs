using System.Diagnostics;

namespace Sage.Utilities
{
    /// <summary>
    /// Provides standardized, color-coded console logging for the Sage compiler pipeline.
    /// </summary>
    internal static class CompilerLogger
    {
        public static void LogStep(string message) => Console.WriteLine(message);

        public static void LogInfo(string message) => Console.WriteLine($"[INFO] {message}");

        /// <summary>
        /// Logs non-fatal issues that don't stop the compilation process.
        /// </summary>
        public static void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARNING] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Logs debug information. These calls are automatically stripped 
        /// from the binary in Release mode.
        /// </summary>
        [Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   [DEBUG] {message}");
            Console.ResetColor();
        }

        public static void LogSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] {message}");
            Console.ResetColor();
        }

        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Logs a critical failure and displays the stack trace for deep debugging.
        /// </summary>
        public static void LogFatal(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[FATAL ERROR] {ex.Message}");

            if (ex.StackTrace != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(ex.StackTrace);
            }

            Console.ResetColor();
        }
    }
}