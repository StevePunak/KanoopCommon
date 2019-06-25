using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Extensions
{
	public static class ColorExtensions
	{
		public static int ByteArraySize = sizeof(int);

		public static byte[] Serialize(this Color color)
		{
			return BitConverter.GetBytes(color.ToArgb());
		}

		public static Color Deserialize(byte[] serialized)
		{
			Color color = Color.FromArgb(BitConverter.ToInt32(serialized, 0));
			return color;
		}
	}
}
