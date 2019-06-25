using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Geometry
{
	public class GeoSquare : GeoRectangle
	{
		public GeoSquare(GeoPointList points)
			: base(points)
		{
			if(Diagonals[0].Length != Diagonals[1].Length)
			{
				throw new GeometryException("Trying to create square with invalid shape (Diaganols != )");
			}

		}
	}
}
