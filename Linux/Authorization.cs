using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.Linux;
using Renci.SshNet;

namespace KanoopCommon.Linux
{
	public class Authorization
	{
		public static bool Authorize(SSHCredentials sshCredentials)
		{
			SshClient client = null;

			PrivateKeyConnectionInfo connectionInfo = new PrivateKeyConnectionInfo(
				sshCredentials.Host,
				sshCredentials.UserName,
				new PrivateKeyFile[] { new PrivateKeyFile(sshCredentials.KeyFile) } );

			client = new SshClient(connectionInfo);
			client.Connect();
			client.Disconnect();
			return true;
		}
	}
}
