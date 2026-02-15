using System.Diagnostics;
using Sage.Core;
using Sage.Enums;

namespace Sage.Utilities
{
    /// <summary>
    /// Provides standardized, color-coded console logging for the Sage compiler pipeline.
    /// Supports semantic error formatting, progress tracking, and debug tracing.
    /// </summary>
    public static class CompilerLogger
    {
        /// <summary>Logs a major step in the compilation process (e.g., "1. Tokenizing...").</summary>
        public static void LogStep(string message) => Console.WriteLine(message);

        /// <summary>Logs general information about the compilation context.</summary>
        public static void LogInfo(string message) => Console.WriteLine($"[INFO] {message}");

        /// <summary>
        /// Logs a non-fatal warning that does not halt the compilation.
        /// </summary>
        public static void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARNING] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Logs a specific syntax or semantic error associated with a source token.
        /// Follows the standard format: file(line,col): error CODE: Message
        /// </summary>
        public static void LogError(Token token, string code, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            // Standard format allows IDEs to parse the location automatically
            Console.WriteLine($"main.sg({token.Line},{token.Column}): error {code}: {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Logs a generic system or infrastructure error (e.g., IO failures).
        /// </summary>
        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[SYS ERROR] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Logs a successful operation or completed phase.
        /// </summary>
        public static void LogSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Logs a critical failure, handling both Sage-specific and generic C# exceptions.
        /// </summary>
        public static void LogFatal(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            if (ex is CompilerException sce)
            {
                // Format the error nicely using our token-based system
                var token = sce.OffendingToken ?? new Token(TokenType.Unknown, "?", 0, 0);
                LogError(token, sce.ErrorCode, sce.Message);
            }
            else
            {
                // Generic system crash
                Console.WriteLine($"\n[FATAL ERROR] {ex.Message}");
                if (ex.StackTrace != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(ex.StackTrace);
                }
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Logs internal debug information. Stripped from production builds.
        /// </summary>
        [Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"    [DEBUG] {message}");
            Console.ResetColor();
        }
    }
}