using System;
using System.Diagnostics;
using System.Text;
using Sage.Utilities;

namespace Sage.Core
{
    internal static class ProcessExecutor
    {
        public static void Run(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) error.AppendLine(e.Data); };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                throw new FileNotFoundException($"Tool not found or not in PATH: {fileName}");
            }

            if (process.ExitCode != 0)
            {
                // Se falhar, mostramos o erro completo
                throw new InvalidOperationException($"Process failed ({fileName}):\n{error}\n{output}");
            }

            // Logs informativos (opcional, para não poluir demais)
            if (output.Length > 0) CompilerLogger.LogDebug($"[{fileName}] {output}");
        }
    }
}