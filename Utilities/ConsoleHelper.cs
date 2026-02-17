using System.Runtime.InteropServices;

namespace Sage.Utilities
{
    /// <summary>
    /// Encapsulates Windows API P/Invokes to enable Ansi Color support.
    /// </summary>
    internal static class ConsoleHelper
    {
#pragma warning disable
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
#pragma warning restore

        public static void EnableAnsiSupport()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
                if (GetConsoleMode(handle, out uint mode))
                    SetConsoleMode(handle, mode | 0x0004); // ENABLE_VIRTUAL_TERMINAL_PROCESSING
            }
        }
    }
}
