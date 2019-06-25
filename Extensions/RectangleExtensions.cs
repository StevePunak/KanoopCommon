using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using KanoopCommon.Geometry;

namespace KanoopCommon.Extensions
{
	public static class RectangleExtensions
	{
		public static int ByteArraySize = PointExtensions.ByteArraySize + SizeExtensions.ByteArraySize;

		public static Rectangle Grow(this Rectangle rectangle, int count)
		{
			Rectangle newRect = rectangle;
			newRect = new Rectangle(new Point(rectangle.X - count, rectangle.Y - count), new Size(rectangle.Width + count * 2, rectangle.Height + count * 2));
			return newRect;
		}

		public static Rectangle Shrink(this Rectangle rectangle, int count)
		{
			Rectangle newRect = rectangle;
			newRect = new Rectangle(new Point(rectangle.X + count, rectangle.Y + count), new Size(rectangle.Width - count * 2, rectangle.Height - count * 2));
			return newRect;
		}

		public static Rectangle GetCenteredRectangle(Point centerPoint, Size size)
		{
			return new Rectangle(new Point(centerPoint.X - size.Width / 2, centerPoint.Y - size.Height / 2), size);
		}

		public static byte[] Serialize(this Rectangle rectangle)
		{
			byte[] serialized = new byte[ByteArraySize];
			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(serialized)))
			{
				bw.Write(PointExtensions.Serialize(rectangle.Location));
				bw.Write(SizeExtensions.Serialize(rectangle.Size));
			}
			return serialized;
		}

		public static Rectangle Deserialize(BinaryReader br)
		{
			Point point = PointExtensions.Deserialize(br);
			Size size = SizeExtensions.Deserialize(br);
			return new Rectangle(point, size);
		}

		public static Double Area(this Rectangle rect)
		{
			return (Double)rect.Size.Height * (Double)rect.Size.Width;
		}

		public static PointD Centroid(this Rectangle rect)
		{
			return new PointD(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
		}

		public static Rectangle ExpandWithinImage(this Rectangle rect, int pixels, Size imageSize)
		{
			int left = Math.Max(0, rect.Left - pixels);
			int top = Math.Max(0, rect.Top - pixels);
			int right = Math.Min(imageSize.Width - 1, rect.Right + pixels);
			int bottom = Math.Min(imageSize.Height - 1, rect.Bottom + pixels);
			return new Rectangle(left, top, right - left, bottom - top);
		}

		public static Rectangle GetFromTwoPoints(Point point1, Point point2)
		{
			PointD p1 = new PointD(point1.X, point1.Y);
			PointD p2 = new PointD(point1.X, point2.Y);
			PointD p3 = new PointD(point2.X, point1.Y);
			PointD p4 = new PointD(point2.X, point2.Y);
			RectangleD rect = new RectangleD(p1, p2, p3, p4);
			PointD upperLeft = rect.UpperLeft;
			PointD lowerRight = rect.LowerRight;
			return new Rectangle(upperLeft.ToPoint(), new Size((int)(lowerRight.X - upperLeft.X), (int)(lowerRight.Y - upperLeft.Y)));
		}

		public static Rectangle GetFromPoint(Size imageSize, Point point, Size rectangleSize)
		{
			int left = Math.Max(0, point.X - rectangleSize.Width / 2);
			int top = Math.Max(0, point.Y - rectangleSize.Height / 2);
			int right = Math.Min(imageSize.Width - 1, point.X + rectangleSize.Width / 2);
			int bottom = Math.Min(imageSize.Height - 1, point.Y + rectangleSize.Height / 2);
			return new Rectangle(left, top, right - left, bottom - top);
		}

		public static int GetMidX(this Rectangle rect)
		{
			return rect.X + rect.Width / 2;
		}

		public static bool ContainsAny(this Rectangle rectangle, IEnumerable<Point> points)
		{
			bool result = false;
			foreach(Point point in points)
			{
				if(rectangle.Contains(point))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public static Line TopLine(this Rectangle rectangle)
		{
			return new Line(rectangle.Location, rectangle.Location.OffsetPoint(rectangle.Width, 0));
		}

		public static Line BottomLine(this Rectangle rectangle)
		{
			Point lowerLeft = rectangle.Location.OffsetPoint(0, rectangle.Height);
			return new Line(lowerLeft, lowerLeft.OffsetPoint(rectangle.Width, 0));
		}

		public static bool ContainsAny(this Rectangle rectangle, IEnumerable<PointD> points)
		{
			bool result = false;
			foreach(PointD point in points)
			{
				if(rectangle.Contains(point.ToPoint()))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public static bool ContainsAnyCornerOf(this Rectangle rectangle, Rectangle other)
		{
			if( rectangle.Contains(other.Location) || 
				rectangle.Contains(other.Location.OffsetPoint(other.Width, 0)) ||
				rectangle.Contains(other.Location.OffsetPoint(other.Width, other.Height)) ||
				rectangle.Contains(other.Location.OffsetPoint(0, other.Height)))
			{
				return true;
			}
			return false;
		}
	}
}
