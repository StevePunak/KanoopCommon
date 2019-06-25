using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Extensions
{
	public static class PointExtensions
	{
		public static int ByteArraySize = sizeof(Int32) + sizeof(Int32);

		public static Point OffsetPoint(this Point from, int x, int y)
		{
			return new Point(from.X + x, from.Y + y);
		}

		public static byte[] Serialize(this Point point)
		{
			byte[] serialized = new byte[ByteArraySize];
			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(serialized)))
			{
				bw.Write(point.X);
				bw.Write(point.Y);
			}
			return serialized;
		}

		public static Point Deserialize(BinaryReader br)
		{
			Int32 x = br.ReadInt32();
			Int32 y = br.ReadInt32();
			return new Point(x, y);
		}
	}
}
