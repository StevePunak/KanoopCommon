using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// using KanoopCommon.Services;
//using KanoopCommon.IPC;
using KanoopCommon.Logging;
using System.Net;
using System.IO;
using System.Threading;
using KanoopCommon.Threading;
using System.Diagnostics;

namespace Linux
{
	public class LinuxServiceController
	{
		#region Constants

		const String CMD_START =							"start";
		const String CMD_STOP =								"stop";
		const String CMD_STATUS =							"status";

		const String NO_COLORS =							"col -b";

		private const int PLINK_WAIT_MS = 2500;

		#endregion

		#region Private Member Variables

		private string _host;
		private string _user;
		private string _plinkPath;

		#endregion

		#region Consturctors

		static LinuxServiceController()
		{
		}

		public LinuxServiceController(String plinkPath, String host)
			: this(plinkPath, host, Environment.UserName.ToLower()) { }

		public LinuxServiceController(String plinkPath, string host, string user)
		{
			_host = host;
			_user = user;
			_plinkPath = plinkPath;
		}

		#endregion

		#region Public Start / Stop / Status Methods

		public bool Start(string serviceName)
		{
			string a, b;

			ProcessStartInfo pinfo = CreateSystemCtlStart(serviceName);
			return SendRemoteSshCommand(pinfo, "y\n", false, true, out a, out b);
		}

		public bool Stop(string serviceName)
		{
			string a, b;

			ProcessStartInfo pinfo = CreateSystemCtlStop(serviceName);
			return SendRemoteSshCommand(pinfo, "y\n", false, true, out a, out b);
		}

		public LinuxProcessList GetProcessStatus()
		{
			return GetProcessStatus(new List<UInt32>());
		}

		public LinuxProcessList GetProcessStatus(List<UInt32> pids)
		{
			string stdout, stderr;

			SendRemoteSshCommand(CreateProcessInformation(CreateProcessListCommand(pids)), "y\n", false, false, out stdout, out stderr);
			SendRemoteSshCommand(CreateProcessInformation(CreateReadProcessListCommand()), "y\n", true, true, out stdout, out stderr);
			
			LinuxProcessList processes = new LinuxProcessList();

			using(TextReader tr = new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(stdout))))
			{
				String line;
				while((line = tr.ReadLine()) != null)
				{
					LinuxProcess info;
					if(LinuxProcess.TryParse(line, out info))
					{
						processes.Add(info);
					}
				}
			}

			return processes;
		}

		#endregion

		#region SSH Interactions

		private bool SendRemoteSshCommand(ProcessStartInfo startInfo, string stdin, bool readStdOut, bool readStdErr, out string stdout, out string stderr)
		{
			bool result = false;
			stderr = stdout = null;

			try
			{
				using (Process p = Process.Start(startInfo))
				{
					p.StandardInput.Write(stdin);

					if (readStdErr)
					{
						string tempStderr = string.Empty;
						Thread stderrThread = new Thread((ThreadStart)delegate()
						{
							if (readStdErr)
							{
								tempStderr = p.StandardError.ReadToEnd();
							}
						});
						stderrThread.Start();

						string tempStdout = string.Empty;
						Thread stdoutThread = new Thread((ThreadStart)delegate()
						{
							if (readStdOut)
							{
								tempStdout = p.StandardOutput.ReadToEnd();
							}
						});
						stdoutThread.Start();

						stdoutThread.Join();
						stderrThread.Join();
						stdout = tempStdout;
						stderr = tempStderr;
					}

					p.WaitForExit(PLINK_WAIT_MS);
				}

				if (stderr != null && stderr.Contains("ERROR"))
				{
					result = false;
				}
				else
				{
					result = true;
				}

			}
			catch (Exception e)
			{
				stderr = e.Message + Environment.NewLine + e.StackTrace;
			}

			return result;
		}

		#endregion

		#region Process Creation

		private ProcessStartInfo CreateSystemCtlStart(string unit)
		{
			return CreateProcessInformation(CreateSystemCtlCommand(CMD_START, unit));
		}

		private ProcessStartInfo CreateSystemCtlStop(string unit)
		{
			return CreateProcessInformation(CreateSystemCtlCommand(CMD_STOP, unit));
		}

		private ProcessStartInfo CreateSystemCtlStatus(string unit)
		{
			return CreateProcessInformation(CreateSystemCtlCommand(CMD_STATUS, unit));
		}

		private ProcessStartInfo CreateProcessInformation(String command)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo()
			{
				FileName = _plinkPath,
				Arguments = command,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
			};

			Log.SysLogText(LogLevel.DEBUG, "\"{0}\" {1}", startInfo.FileName, startInfo.Arguments);
			Debug.WriteLine(startInfo.FileName + " " + startInfo.Arguments);

			return startInfo;
		}

		
		#endregion

		#region Command String Creation

		private String PlinkPrefix { get { return string.Format("-A -t {0}@{1} ", _user, _host); } }

		private String CreateSystemCtlCommand(string command, string unit)
		{
			return string.Format("{0}\"sudo systemctl {1} {2} | {3}\"", PlinkPrefix, command, unit, NO_COLORS);
		}

		private String CreateProcessListCommand(List<UInt32> pids)
		{
			StringBuilder sb = new StringBuilder();
			String command = String.Empty;
			if(pids.Count > 0)
			{
				sb.Append("-p '");
				foreach(UInt32 pid in pids)
				{
					sb.AppendFormat("{0} ", pid);
				}
				command = sb.ToString().Trim() + '\'';
			}
			else
			{
				command = "ax";
			}
			return CreateProcessListCommand(command);
		}

		private String CreateProcessListCommand(String additionalParameters = null)
		{
			return string.Format("{0}\"ps {1} -o pid,user,pcpu,%mem,vsz,rss,tty,stat,etime,cmd > {2}\"", 
				PlinkPrefix, 
				String.IsNullOrEmpty(additionalParameters) ? String.Empty : additionalParameters,
				TempFileName("ps"));
		}

		private String CreateReadProcessListCommand()
		{
			return string.Format("{0}\"cat {1}\"", PlinkPrefix, TempFileName("ps"));
		}

		private String CreateListUnitsCommand()
		{
			return string.Format("{0}\"systemctl -a --no-pager list-units\"", PlinkPrefix);
		}

		private String TempFileName(String prefix)
		{
			return String.Format("/tmp/{0}.{1}", prefix, (UInt32)GetHashCode());
		}

		#endregion

	}
}
