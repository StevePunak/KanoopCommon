using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Geometry
{
	public class GridLocation
	{
		public PointD Point { get; set; }
		public Double Bearing { get; set; }

		public GridLocation()
			: this(new PointD(), 0) {}

		public GridLocation(PointD point, Double bearing)
		{
			Point = point;
			Bearing = bearing;
		}

		public override string ToString()
		{
			return String.Format("{0} @{1}°", Point, Bearing);
		}
	}
}
