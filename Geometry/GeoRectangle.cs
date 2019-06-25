using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Geometry
{
	public class GeoRectangle : GeoPolygon
	{
		public Double Height { get { return GeoLines[0].Length; } }

		public Double Width { get { return GeoLines[1].Length; } }

		public GeoPoint NorthWest { get { return GeoPoints.NorthWest; } }

		public GeoPoint NorthEast { get { return GeoPoints.NorthEast; } }

		public GeoPoint SouthEast { get { return GeoPoints.SouthEast; } }

		public GeoPoint SouthWest { get { return GeoPoints.SouthWest; } }

		public GeoPoint Center { get { return Diagonals[0].MidPoint as GeoPoint; } }

		public GeoLine ShortestLeg
		{
			get
			{
				GeoLine l1 = new GeoLine(_points[0], _points[1]);
				GeoLine l2 = new GeoLine(_points[1], _points[2]);
				return l1.Length > l2.Length ? l2 : l1;
			}
		}

		public GeoLine LongestLeg
		{
			get
			{
				GeoLine l1 = new GeoLine(_points[0], _points[1]);
				GeoLine l2 = new GeoLine(_points[1], _points[2]);
				return l1.Length > l2.Length ? l1 : l2;
			}
		}

		public GeoLineList Diagonals 
		{ 
			get 
			{ 
				return new GeoLineList() 
				{
					new GeoLine(GeoLines[0].P1, GeoLines[2].P1),
					new GeoLine(GeoLines[1].P1, GeoLines[3].P1),
				};
			}
		}

		public GeoRectangle(PointDList points)
			: this(new GeoPointList(points)) {}

		public GeoRectangle(GeoPoint p1, GeoPoint p2, GeoPoint p3, GeoPoint p4)
			: base(new GeoPointList() { p1, p2, p3, p4 } ) {}

		public GeoRectangle(GeoPointList points)
			: base(new GeoPointList() { points.NorthWest, points.NorthEast, points.SouthEast, points.SouthWest } )
		{
			if(points.Count != 4)
			{
				throw new InvalidOperationException("GeoRectangle MUST consist of four points");
			}
		}

		public void Expand(Double meters)
		{
			/** SPSP THIS WILL NOT WORK */
			GeoLine cross1 = new GeoLine(GeoPoints[0], GeoPoints[2]);
			GeoLine cross2 = new GeoLine(GeoPoints[1], GeoPoints[3]);

			GeoPoints[0].Move(Angle.Reverse(cross1.Bearing), meters);
			GeoPoints[1].Move(Angle.Reverse(cross2.Bearing), meters);
			GeoPoints[2].Move(cross1.Bearing, meters);
			GeoPoints[3].Move(cross2.Bearing, meters);
		}

		//public GeoEllipse ToGeoEllipse()
		//{
		//    GeoEllipse ellipse = new GeoEllipse(
		//}

		public override string ToString()
		{
			return String.Format("NW: {0}  NE: {1}  SE: {2}  SW: {3}", NorthWest, NorthEast, SouthEast, SouthWest);
		}

	}
}
