using System.Diagnostics;

namespace Passtable.Tools
{
    public static class GitUtils
    {
        public static string GetBranchName()
        {
            var startInfo = new ProcessStartInfo("git.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Arguments = "rev-parse --abbrev-ref HEAD"
            };

            var process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();

            return process.StandardOutput.ReadLine();
        }
    }
}