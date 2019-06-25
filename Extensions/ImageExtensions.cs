using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace KanoopCommon.Extensions
{
	public static class ImageExtensions
	{
		public static ImageFormat GetImageFormat(this Image image)
		{             
			if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
				return System.Drawing.Imaging.ImageFormat.Jpeg;
			if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
				return System.Drawing.Imaging.ImageFormat.Bmp;
			if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
				return System.Drawing.Imaging.ImageFormat.Png;
			if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Emf))
				return System.Drawing.Imaging.ImageFormat.Emf;
			if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Exif))
				return System.Drawing.Imaging.ImageFormat.Exif;
			if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
				return System.Drawing.Imaging.ImageFormat.Gif;
			if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Icon))
				return System.Drawing.Imaging.ImageFormat.Icon;
			if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp))
				return System.Drawing.Imaging.ImageFormat.MemoryBmp;
			if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
				return System.Drawing.Imaging.ImageFormat.Tiff;
			else
				return System.Drawing.Imaging.ImageFormat.Wmf;            
		}

		public static byte[] ToByteArray(this Image image)
		{
			return image.ToByteArray(image.GetImageFormat());
		}

		public static byte[] ToByteArray(this Image image, ImageFormat format)
		{
			byte[] array;
			using(MemoryStream ms = new MemoryStream())
			{
				image.Save(ms, format);
				array = ms.ToArray();
			}
			return array;
		}

		public static void SaveIfDirectoryExists(this Image image, String fileName)
		{
			if(Directory.Exists(Path.GetDirectoryName(fileName)))
			{
				image.Save(fileName);
			}
		}

	}
}
