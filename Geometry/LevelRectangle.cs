using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Geometry
{
	public class LevelRectangle : RectangleD
	{
		PointD _location;
		public PointD Location { get { return _location; } }

		Double _width;
		public override double Width { get { return _width; } }

		Double _height;
		public override double Height { get { return _height; } }

		public LevelRectangle(Double x, Double y, Double width, Double height)
			: base(x, y, width, height)
		{
			_location = UpperLeft;
			_width = width;
			_height = height;
		}

		public override bool Contains(PointD point)
		{
			return point.X >= _location.X &&
					point.X <= _location.X + _width &&
					point.Y >= _location.Y &&
					point.Y <= _location.Y + _height;
		}
	}
}
