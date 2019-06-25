using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.Serialization;

namespace KanoopCommon.Linux
{
	[IsSerializable]
	public class SSHCredentials
	{
		public String Host { get; set; }

		public String UserName { get; set; }

		public String KeyFile { get; set; }

		public bool IsTunnel { get; set; }

		public String TunnelHost { get; set; }

		public UInt16 LocalTunnelPort { get; set; }

		public UInt16 RemoteTunnelPort { get; set; }

		[IgnoreForSerialization]
		public bool IsEmpty { get { return String.IsNullOrEmpty(Host); } }

		public SSHCredentials()
			: this(String.Empty, String.Empty, String.Empty, false, String.Empty, 0, 0) { }
		public SSHCredentials(String host, String user, String keyFile)
			: this(host, user, keyFile, false, String.Empty, 0, 0) { }

		public SSHCredentials(String host, String user, String keyFile, bool isTunnel, String tunnelHost, UInt16 localPort, UInt16 remotePort)
		{
			Host = host;
			UserName = user;
			KeyFile = keyFile;
			IsTunnel = isTunnel;
			TunnelHost = tunnelHost;
			LocalTunnelPort = localPort;
			RemoteTunnelPort = remotePort;
		}

		public override string ToString()
		{
			return String.Format("SSH to {0}@{1} port {2}=>{3}", UserName, Host, LocalTunnelPort, RemoteTunnelPort);
		}
	}
}
