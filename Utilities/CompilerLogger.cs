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
        /// <summary>
        /// Logs a major milestone in the compilation process to the console.
        /// </summary>
        /// <param name="message">The progress message to display.</param>
        public static void LogStep(string message) => Console.WriteLine(message);

        /// <summary>
        /// Logs general information regarding the current compilation context.
        /// </summary>
        /// <param name="message">The informational message to display.</param>
        public static void LogInfo(string message) => Console.WriteLine($"[INFO] {message}");

        /// <summary>
        /// Logs a non-fatal warning in yellow. Warnings do not halt the compilation process.
        /// </summary>
        /// <param name="message">The warning message to display.</param>
        public static void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARNING] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Logs a specific code error linked to a source token. 
        /// Formatted as 'file(line,col): error CODE: Message' for IDE compatibility.
        /// </summary>
        /// <param name="token">The token where the error was detected.</param>
        /// <param name="code">The unique Sage error code (e.g., S105).</param>
        /// <param name="message">A descriptive error message.</param>
        public static void LogError(Token token, string code, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            // Standard format allows IDEs like VS Code to link directly to the source file
            Console.WriteLine($"main.sg({token.Line},{token.Column}): error {code}: {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Logs a generic system error, such as IO failures or missing environment tools.
        /// </summary>
        /// <param name="message">The system error message to display.</param>
        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[SYS ERROR] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Logs a successful operation or phase completion in green.
        /// </summary>
        /// <param name="message">The success message to display.</param>
        public static void LogSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Handles and logs critical failures, supporting both Sage-specific and native exceptions.
        /// Ensures correct line/column reporting even if a token is not available.
        /// </summary>
        /// <param name="ex">The caught exception to be logged.</param>
        public static void LogFatal(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            if (ex is CompilerException sce)
            {
                if (sce.OffendingToken == null)
                {
                    // Fallback: Create a virtual token to maintain standard error formatting
                    var tempToken = new Token(TokenType.Unknown, "", sce.Line, sce.Column);
                    LogError(tempToken, sce.ErrorCode, sce.Message);
                }
                else
                {
                    LogError(sce.OffendingToken, sce.ErrorCode, sce.Message);
                }
            }
            else
            {
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
        /// Logs internal debug information in dark gray. 
        /// This method is stripped from production builds via the Conditional attribute.
        /// </summary>
        /// <param name="message">The debug message to display.</param>
        [Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"    [DEBUG] {message}");
            Console.ResetColor();
        }
    }
}