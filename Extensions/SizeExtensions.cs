using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Extensions
{
	public static class SizeExtensions
	{
		public static int ByteArraySize = sizeof(Int32) + sizeof(Int32);

		public static Size MinimumSize(this Size size)
		{
			int min = Math.Min(size.Height, size.Width);
			return new Size(min, min);
		}

		public static Size Scale(this Size size, Double scale)
		{
			Double width = (Double)size.Width * scale;
			Double height = (Double)size.Height * scale;
			return new Size((int)Math.Round(width), (int)Math.Round(height));
		}

		public static Size Shrink(this Size size, int pixels)
		{
			return new Size(size.Width - pixels, size.Height - pixels);
		}

		public static Size Grow(this Size size, int pixels)
		{
			return new Size(size.Width + pixels, size.Height + pixels);
		}

		public static Size Validate(this Size value, Size max)
		{
			return new Size(Math.Min(max.Width, value.Width), Math.Min(max.Height, value.Height));
		}

		public static byte[] Serialize(this Size size)
		{
			byte[] serialized = new byte[ByteArraySize];
			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(serialized)))
			{
				bw.Write(size.Width);
				bw.Write(size.Height);
			}
			return serialized;
		}

		public static Size Deserialize(BinaryReader br)
		{
			Int32 width = br.ReadInt32();
			Int32 height = br.ReadInt32();
			return new Size(width, height);
		}
	}
}
