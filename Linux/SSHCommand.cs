using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KanoopCommon.Logging;
using Renci.SshNet;

namespace KanoopCommon.Linux
{
	public class SSHCommand
	{
		#region Constants

		const String NO_COLORS =							"col -b";

		#endregion

		#region Private Properties

		#endregion

		#region Private Member Variables

		private string _host;
		private string _user;
		private string _keyPath;

		private ConnectionInfo _connectionInfo;

		#endregion

		#region Constructors

		public SSHCommand(String host)
			: this(host, Environment.UserName.ToLower()) { }

		public SSHCommand(string host, string user)
			: this(String.Empty, host, user) {}

		public SSHCommand(String keyPath, string host, string user)
		{
			_host = host;
			_user = user;
			_keyPath = keyPath;

			_connectionInfo = new ConnectionInfo(_host, _user, new AuthenticationMethod[] 
							{ 
								new PrivateKeyAuthenticationMethod(_user, new PrivateKeyFile[]
										{ new PrivateKeyFile(_keyPath) } ) } );
		}

		#endregion

		#region Public Access Methods

		public bool TrySendCommand(String command, out String stdout, out String stderr)
		{
			bool result = false;
			stdout = String.Empty;
			stderr = String.Empty;


			try
			{
				using(SshClient ssh = new SshClient(_connectionInfo))
				{
					ssh.Connect();
					using(SshCommand cmd = ssh.CreateCommand(command))
					{
						cmd.Execute();
					}
					ssh.Disconnect();
				}
			}
			catch(Exception e)
			{
				Debug.WriteLine(String.Format("EXCEPTION: {0}", e.Message));
				result = false;
			}
			return result;
		}

		#endregion

#if zero
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

					if (readStdErr || readStdOut)
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

		private String CreateCommand(string command)
		{
			return string.Format("{0}\"{1} | {2}\"", PlinkPrefix, command, NO_COLORS);
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
#endif
	}
}
