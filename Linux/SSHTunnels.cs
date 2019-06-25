using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.Database;
using KanoopCommon.Logging;
using Renci.SshNet;

namespace KanoopCommon.Linux
{
	public class SSHTunnels
	{
		#region Local Classes

		class SSHClientList : List<SshClient>
		{
			public bool TryGetClient(SSHCredentials credentials, out SshClient client)
			{
				client = null;
				SshClient deleteClient = null;

				foreach(SshClient c in this)
				{
					if(c.ConnectionInfo.Host == credentials.Host)
					{
						if(c.IsConnected == true)
						{
							client = c;
						}
						else
						{
							deleteClient = c;
						}
						break;
					}
				}

				if(deleteClient != null)
				{
					Remove(deleteClient);
				}

				return client != null;
			}
		}

		#endregion
		
		#region Member Variables

		static SSHClientList _clients;

		#endregion

		#region Constructor

		static SSHTunnels()
		{
			_clients = new SSHClientList();
		}

		#endregion

		static public bool EnsureTunnelExists(SqlDBCredentials credentials)
		{
			SSHCredentials sshCredentials = credentials.SSHCredentials;
			SshClient client = null;

			if(_clients.TryGetClient(sshCredentials, out client) == false)
			{
				PrivateKeyConnectionInfo connectionInfo = new PrivateKeyConnectionInfo(
					sshCredentials.Host, 
					sshCredentials.UserName, 
					new PrivateKeyFile[] { new PrivateKeyFile(sshCredentials.KeyFile) } );

				client = new SshClient(connectionInfo);
				client.KeepAliveInterval = TimeSpan.FromMinutes(1);
				client.ErrorOccurred += Client_ErrorOccurred;
				client.HostKeyReceived += Client_HostKeyReceived;
				client.Connect();

				if(client.IsConnected)
				{
					ForwardedPortLocal port = new ForwardedPortLocal(
										IPAddress.Loopback.ToString(),
										 sshCredentials.LocalTunnelPort, 
										 sshCredentials.TunnelHost, 3306);
					client.AddForwardedPort(port);
					port.Start();
					if(port.IsStarted == false)
					{
						client.Disconnect();
						client = null;
						throw new Exception("Could not forward port");
					}
					_clients.Add(client);
				}
			}
			return client != null;
		}

		private static void Client_HostKeyReceived(object sender, Renci.SshNet.Common.HostKeyEventArgs e)
		{
			
		}

		private static void Client_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
		{
		}
	}
}
