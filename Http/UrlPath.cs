using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Http
{
	public class UrlPath
	{
		public static String Combine(params object[] parts)
		{
			List<String> strs = new List<String>();
			foreach(Object part in parts)
			{
				strs.Add(part.ToString());
			}
			return Combine(strs);
		}

		private static String Combine(List<String> parts)
		{
			StringBuilder sb = new StringBuilder();
			for(int x = 0;x < parts.Count;x++)
			{
				sb.Append(parts[x]);
				if(x < parts.Count - 1)
					sb.Append('/');
			}
			return sb.ToString();
		}
	}
}
