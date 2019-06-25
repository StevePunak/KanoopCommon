using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;
using KanoopCommon.Database;
using System.ComponentModel;
using KanoopCommon.Conversions;
using KanoopCommon.Logging;

namespace KanoopCommon.Geometry
{
	public enum Relationship
	{
		NoRelation = 0,
		IntersectsWith = 1,
		Contains = 2,
		ContainedBy = 3,
		AwayFrom = 4,
		Towards = 5
	};


	public class FlatGeo
	{
		
		public static Double VectorAngle(Line l1, Line l2)
		{
			Double		a = l1.P2.X - l1.P1.X;
			Double		b = l1.P2.Y - l1.P1.Y;
			Double		c = l2.P2.X - l2.P1.X;
			Double		d = l2.P2.Y - l2.P1.Y;

			Double 		z =((a*c) + (b*d)) / (Math.Sqrt((a*a) + (b*b)) * Math.Sqrt((c*c) + (d*d)));
			Double		theta = Math.Acos(z);
			Double		result = Degrees(theta);

			return result;
		}

		public static Angle Angle(Line l1, Line l2)
		{
			Double ret = Math.Atan2(l1.P2.Y - l1.P1.Y, l1.P2.X - l1.P1.X) - Math.Atan2(l2.P2.Y - l2.P1.Y, l2.P2.X - l2.P1.X);
			ret = Degrees(ret);
			return new Angle(Math.Abs(ret));
		}

		/** 
		 * returns the angle subsumed by 
		 * 
		 * p1 -> vertext -> p2
		 */
		public static Angle Angle(PointD p1, PointD vertex, PointD p2)
		{
			return AngleV2(new Line(vertex, p1), new Line(vertex, p2));
		}

		public static Angle Angle(Point p1, Point vertex, Point p2) { return Angle(new PointD(p1), new PointD(vertex), new PointD(p2)); }

		public static Angle AngleV2(Line l1, Line l2)
		{
			Double ret = Math.Atan2(l1.P2.Y - l1.P1.Y, l1.P2.X - l1.P1.X) - Math.Atan2(l2.P2.Y - l2.P1.Y, l2.P2.X - l2.P1.X);
			ret = Degrees(ret);
			if(ret < 0)
			{
				ret += 360;
			}
			return new Angle(Math.Abs(ret));
		}

		public static Double Distance(Point p1, Point p2) { return Distance(new PointD(p1), new PointD(p2)); }
		
		public static Double Distance(PointD p1, PointD p2)
		{
			Line l = new Line(p1, p2);
			return l.Length;
		}

		public static Double Degrees(Double angle)
		{
			return angle * (180 / Math.PI);
		}

		public static Double Radians(Double angle)
		{
			return Math.PI * angle / 180;
		}

		public static Double AngularDifference(Line l1, Line l2)
		{
			return Math.Abs((l1.Bearing + 360) - (l2.Bearing + 360));
		}

		[Obsolete("Use GetPoint - BEWARE Parameter Order Is Changed")]
		public static PointD GetPointFromLocation(PointD from, Double degBearing, Double distance)
		{
			Double radBearing = Radians(degBearing);
			return new PointD(from.X + Math.Sin(radBearing) * distance, from.Y - Math.Cos(radBearing) * distance);
		}

		public static PointD GetPoint(PointD from, Double bearing, Double distance)
		{
			Double radBearing = Radians(bearing);
			return new PointD(from.X + Math.Sin(radBearing) * distance, from.Y - Math.Cos(radBearing) * distance);
		}

		/**
		 * get a point on the arc defined by 'circle' that is 'degrees' deg,
		 * from 'from', in the direction of the point 'direction'
		 */
		public static PointD GetPointOnArc(Circle circle, PointD from, Angle degrees, Relationship relationship, PointD referencePoint)
		{
			Line start = new Line(circle.Center, from);
			Line refLine = new Line(circle.Center, referencePoint);

			Angle newAngle = FlatGeo.ChangeAngle(start, refLine, new Angle(degrees.Degrees * -1));
			return GetPoint(circle.Center, newAngle.Degrees, circle.Radius);
		}

		public static Angle ChangeAngle(Line l1, Line l2, Angle delta)
		{
			Triangle t = new Triangle(l1.P1, l1.P2, l2.P2);
			Angle b1 = new Angle(t.AtoB.Bearing);
			Angle b2 = new Angle(t.AtoC.Bearing);
			
			Angle diff = b2 - b1;

			Angle newAngle;
			if(diff.Degrees > 180)
			{
				/** move clockwise from b2 */
				newAngle = b1 - delta;
			}
			else
			{
				/** move anti-clockwise from b2 */
				newAngle = b1 + delta;
			}

			return newAngle;
		}

		public static Double GetDistance(PointD p1, PointD p2)
		{
			return new Line(p1, p2).Length;
		}

		public static bool GetIntersection(Line l1, Line l2, out PointD retPoint)
		{
			retPoint = new PointD();
			bool bRet = false;

			Double A1, B1, C1;
			GetLineABC(l1, out A1, out B1, out C1);

			Double A2, B2, C2;
			GetLineABC(l2, out A2, out B2, out C2);

			Double det = A1*B2 - A2*B1;
			if(det != 0)
			{
				retPoint.X = ( (B2*C1 - B1*C2) / det);
				retPoint.Y = ( (A1*C2 - A2*C1) / det);

				if(	retPoint.X >= Math.Min(l1.P1.X, l1.P2.X) && retPoint.X <= Math.Max(l1.P1.X, l1.P2.X) &&
					retPoint.X >= Math.Min(l2.P1.X, l2.P2.X) && retPoint.X <= Math.Max(l2.P1.X, l2.P2.X) &&
					retPoint.Y >= Math.Min(l1.P1.Y, l1.P2.Y) && retPoint.Y <= Math.Max(l1.P1.Y, l1.P2.Y) &&
					retPoint.Y >= Math.Min(l2.P1.Y, l2.P2.Y) && retPoint.Y <= Math.Max(l2.P1.Y, l2.P2.Y) )
				{
					bRet = true;
				}
			}
			// else, its parallel
			return bRet;
		}

		public static bool IsPointInCircle(PointD point, Circle circle)
		{
			Line l = new Line(circle.Center, point);
			return (l.Length <= circle.Radius);
		}

		public static void GetLineABC(Line line, out Double A, out Double B, out Double C)
		{
			A = line.P2.Y - line.P1.Y;
			B = line.P1.X - line.P2.X;
			C = A * line.P1.X + B * line.P1.Y;
		//	C = line.P2.X * line.P1.Y - line.P1.X * line.P2.Y;
		}

	}
}
