using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.Geometry;

namespace KanoopCommon.Extensions
{
	public static class BitmapExtensions
	{
		public static PointD Center(this Image bitmap)
		{
			return new PointD(bitmap.Width / 2, bitmap.Height / 2);
		}

		public static Rectangle RectangleForCircle(this Image image, Point center, int radius)
		{
			return new Rectangle(center.X - radius,center.Y - radius, radius * 2, radius * 2);
		}

		public static Bitmap Resize(this Bitmap bitmap, SizeF size)
		{
			return bitmap.Resize(size.Width, size.Height);
		}

		public static Bitmap Resize(this Bitmap bitmap, float width, float height)
		{
			return Resize(bitmap, Color.Transparent, width, height);
		}

		public static Bitmap Resize(this Bitmap bitmap, Color backColor, float width, float height)
		{
			SolidBrush brush = new SolidBrush(backColor);
			float scale = Math.Min(width / bitmap.Width, height / bitmap.Height);

			Bitmap ret = new Bitmap((int) width, (int) height);
			using(Graphics gr = Graphics.FromImage(ret))
			{
				gr.InterpolationMode = InterpolationMode.High;
				gr.CompositingQuality = CompositingQuality.HighQuality;
				gr.SmoothingMode = SmoothingMode.AntiAlias;

				int scaleWidth = (int)(bitmap.Width * scale);
				int scaleHeight = (int)(bitmap.Height * scale);

				gr.FillRectangle(brush, new RectangleF(0, 0, width, height));
				gr.DrawImage(bitmap, ((int)width - scaleWidth) / 2, ((int)height / scaleHeight) / 2, scaleWidth, scaleHeight);
			}

			return ret;
		}

		public static Bitmap Rotate(this Bitmap bitmap, Double angle)
		{
//			int longestSide = (int)(Math.Sqrt(bitmap.Width * bitmap.Width + bitmap.Height * bitmap.Height));

			//create a new empty bitmap to hold rotated image
			Bitmap returnBitmap = new Bitmap(bitmap.Width, bitmap.Height);

			//make a graphics object from the empty bitmap
			using(Graphics g = Graphics.FromImage(returnBitmap))
			{
				//move rotation point to center of image
				g.TranslateTransform((float)bitmap.Width / 2, (float)bitmap.Height / 2);

				//rotate
				g.RotateTransform((float)angle);

				//move image back
				g.TranslateTransform(-(float)bitmap.Width / 2, -(float)bitmap.Height / 2);

				//draw passed in image onto graphics object
				g.DrawImage(bitmap, new Point(0, 0));
			}

			return returnBitmap;
		}

		public static Icon GetIcon(Assembly assembly, String resourceName)
		{
			List<String> names = new List<string>(assembly.GetManifestResourceNames());
			Stream stream = assembly.GetManifestResourceStream(resourceName);
			Bitmap bitmap = new Bitmap(stream);
			IntPtr handle = bitmap.GetHicon();
			Icon icon = Icon.FromHandle(handle);
			return icon;
		}
	}
}
