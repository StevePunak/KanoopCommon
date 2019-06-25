using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Geometry
{
	public class Triangle : Polygon
	{
		public PointD A	{ get { return Lines[0].P1 as PointD; } }
		public PointD B	{ get { return Lines[1].P1 as PointD; } }
		public PointD C	{ get { return Lines[2].P1 as PointD; } }

		public Line AtoB { get { return new Line(A, B); } }
		public Line AtoC { get { return new Line(A, C); } }
		public Line BtoC { get { return new Line(B, C); } }
		public Line BtoA { get { return new Line(B, A); } }
		public Line CtoA { get { return new Line(C, A); } }
		public Line CtoB { get { return new Line(C, B); } }

		public Triangle(PointD pA, PointD pB, PointD pC)
			: base(new LineList()
			{ 
				new Line(pA, pB),
				new Line(pB, pC),
				new Line(pC, pA)
			})
		{
		}

		public Angle InteriorAngle(PointD point)
		{
			LineList lines = GetLinesAdjacentTo(point);
			Angle ret = FlatGeo.Angle(lines[0], lines[1]);
			if(ret.Degrees > 180)
			{
				ret.Degrees = 360 - ret.Degrees;
			}
			return ret;
		}


	}
}
