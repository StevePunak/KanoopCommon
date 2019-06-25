using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Extensions
{
	public static class ConsoleExtensions
	{
		public static String ReadLineNoEcho()
		{
			StringBuilder sb = new StringBuilder();

			ConsoleKeyInfo key;
			while((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
			{
				sb.Append(key.KeyChar);
			}
			Console.WriteLine();

			String ret = sb.ToString();

			return ret;
		}

	}
}
