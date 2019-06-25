using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Extensions
{
	public static class IPAddressExtensions
	{
		public static bool TryResolve(String hostname, out IPAddress address)
		{
			address = null;
			IPHostEntry hostEntry;

			hostEntry = Dns.GetHostEntry(hostname);

			//you might get more than one ip for a hostname since 
			//DNS supports more than one record

			if(hostEntry.AddressList.Length > 0)
			{
				address = hostEntry.AddressList[0];
			}
			return address != null;
		}
	}
}
