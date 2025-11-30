using System.Diagnostics;
using System.Text;

namespace DoodleStudio95
{
	public static class ProcUtils
	{
		public static int ExecuteProcess(
				string workingDir,
				string fileName,
				string arguments,
				int timeout,
				out string standardOutput,
				out string standardError)
			{
				int exitCode;

				var standardOutputBuilder = new StringBuilder();
				var standardErrorBuilder = new StringBuilder();

				using (var process = new Process())
				{
					process.StartInfo.UseShellExecute = false;
					if (workingDir != null)
						process.StartInfo.WorkingDirectory = workingDir;
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.RedirectStandardError = true;
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.FileName = fileName;
					process.StartInfo.Arguments = arguments;

					process.OutputDataReceived += (sender, args) => {
						if (args.Data != null)
							standardOutputBuilder.AppendLine(args.Data);
					};
					process.ErrorDataReceived += (sender, args) => {
						if (args.Data != null)
							standardErrorBuilder.AppendLine(args.Data);
					};

					process.Start();

					// This is our place of interest
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();

					if (process.WaitForExit(timeout))
					{
						process.WaitForExit();
						exitCode = process.ExitCode;
					}
					else
					{
						process.Kill();
						throw new System.TimeoutException("Process wait timeout expired");
					}
				}

				standardOutput = standardOutputBuilder.ToString();
				standardError = standardErrorBuilder.ToString();

				return exitCode;
		}
	}
}