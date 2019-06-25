using KanoopCommon.CommonObjects;
using KanoopCommon.Conversions;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System;
using KanoopCommon.Types;

namespace KanoopCommon.Geometry
{
	public class Circle
	{
		#region Public Properties

		PointD	_center;
		public PointD Center
		{
			get { return _center; }
// SPSPREMOVE			set { m_Center = value; }
		}

		public Double Circumference
		{
			get { return 2 * Math.PI * _radius; }
		}

		Double	_radius;
		public Double Radius
		{
			get { return _radius; }
			set { _radius = value; }
		}

		public Double Diameter { get { return Radius * 2; } }

		public Double Area
		{
			get { return Math.PI * (_radius * _radius); }
		}

		#endregion

		#region Constructors

		public Circle() : this(new PointD(), 0) { }

		public Circle(Point center, Double radius)
			: this(new PointD(center), radius) {}

		public Circle(PointD center, Double radius)
		{
			_center = center;
			_radius = radius;
		}

		#endregion

		#region Public Geometery Methods

		public bool Contains(PointD point)
		{
			return point.DistanceTo(Center) < Radius;
		}

		public bool Contains(Point point)
		{
			return new PointD(point).DistanceTo(Center) < Radius;
		}

		public Relationship RelationTo(Circle otherCircle)
		{
			Relationship	ret = Relationship.NoRelation;

			Line			centers = new Line(_center, otherCircle._center);

			if(centers.Length > this.Radius + otherCircle.Radius)
			{
				/** circles do not intersect */
			}
			else if(centers.Length < (double)Math.Abs(Convert.ToDecimal(this.Radius - otherCircle.Radius)))
			{
				/** one circle is contained within another */
				if(this.Radius > otherCircle.Radius)
				{
					ret = Relationship.Contains;
				}
				else
				{
					ret = Relationship.ContainedBy;
				}
			}
			else
			{
				/** circles intersect */
				ret = Relationship.IntersectsWith;
			}
			return ret;
		}

		public void Move(PointD where)
		{
			_center = new PointD(where);
		}

		public bool Intersects(Circle otherCircle)
		{
			bool	bRet = false;

			Line	centers = new Line(_center, otherCircle._center);

			if(centers.Length > this.Radius + otherCircle.Radius)
			{
				/** circles do not intersect */
			}
			else if(centers.Length < (double)Math.Abs(Convert.ToDecimal(this.Radius - otherCircle.Radius)))
			{
				/** one circle is contained within another */
			}
			else
			{
				/** circles intersect */
				bRet = true;
			}
			return bRet;
		}

		/// <summary>
		/// Return the closest point on the edge of the circle to the given point
		/// </summary>
		/// <param name="to"></param>
		/// <returns></returns>
		public PointD ClosestPointOnEdge(PointD to)
		{
			PointD closest = Center.GetPointAt(Center.BearingTo(to), Radius);
			return closest;
		}

		/// <summary>
		/// Return the closest point on the edge of the circle to the given point
		/// </summary>
		/// <param name="to"></param>
		/// <returns></returns>
		public PointD ClosestPointOnEdge(Point to)
		{
			return ClosestPointOnEdge(new PointD(to));
		}

		/// <summary>
		/// Find the points where the two circles intersect.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="intersection1"></param>
		/// <param name="intersection2"></param>
		/// <returns></returns>
		public IntersectionResult FindIntersections(Circle other, out PointD intersection1, out PointD intersection2)
		{
			IntersectionResult result = IntersectionResult.Invalid;

			intersection1 = new PointD(Double.NaN, Double.NaN);
			intersection2 = new PointD(Double.NaN, Double.NaN);

			Double cx0 = this.Center.X;
			Double cy0 = this.Center.Y;
			Double radius0 = this.Radius;

		    Double cx1 = other.Center.X;
			Double cy1 = other.Center.Y;
			Double radius1 = other.Radius;

			// Find the distance between the centers.
			Double	dx = cx0 - cx1;
			Double	dy = cy0 - cy1;
			Double	dist = Math.Sqrt(dx * dx + dy * dy);

			// See how many solutions there are.
			if(dist > radius0 + radius1)
			{
				// No solutions, the circles are too far apart.
				result = IntersectionResult.TooFarApart;
			}
			else if(dist < Math.Abs(radius0 - radius1))
			{
				// No solutions, one circle contains the other.
				result = IntersectionResult.ContainedBy;
			}
			else if((dist == 0) && (radius0 == radius1))
			{
				// No solutions, the circles coincide.
				result = IntersectionResult.Same;
			}
			else
			{
				// Find a and h.
				double	a = (radius0 * radius0 -
				             radius1 * radius1 + dist * dist) / (2 * dist);
				double	h = Math.Sqrt(radius0 * radius0 - a * a);

				// Find P2.
				double	cx2 = cx0 + a * (cx1 - cx0) / dist;
				double	cy2 = cy0 + a * (cy1 - cy0) / dist;

				// Get the points P3.
				intersection1 = new PointD(
				    (Double)(cx2 + h * (cy1 - cy0) / dist),
				    (Double)(cy2 - h * (cx1 - cx0) / dist));
				intersection2 = new PointD(
				    (Double)(cx2 - h * (cy1 - cy0) / dist),
				    (Double)(cy2 + h * (cx1 - cx0) / dist));

				// See if we have 1 or 2 solutions.
				if(dist == radius0 + radius1)
				{
					result = IntersectionResult.OneIntersection;
				}
				else
				{
					result = IntersectionResult.TwoIntersections;
				}
			}

			return result;
		}

		public bool Intersects(Line line)
		{
			PointD i1, i2;
			return Intersects(line, out i1, out i2) > 0;
		}

		public int Intersects(Line line, out PointD intersection1, out PointD intersection2)
		{
			int		intersections = 0;

			Double	dx, dy, A, B, C, det, t;

			dx = line.P2.X - line.P1.X;
			dy = line.P2.Y - line.P1.Y;

			A = dx * dx + dy * dy;
			B = 2 * (dx * (line.P1.X - Center.X) + dy * (line.P1.Y - Center.Y));
			C = (line.P1.X - Center.X) * (line.P1.X - Center.X) + (line.P1.Y - Center.Y) * (line.P1.Y - Center.Y) - Radius * Radius;

			det = B * B - 4 * A * C;

			if((A <= 0.0000001) || (det < 0))
			{
				// No real solutions.
				intersection1 = new PointD(float.NaN, float.NaN);
				intersection2 = new PointD(float.NaN, float.NaN);
			}
			else if(det == 0)
			{
				// One solution.
				t = -B / (2 * A);
				intersection1 = new PointD(line.P1.X + t * dx, line.P1.Y + t * dy);
				intersection2 = new PointD(float.NaN, float.NaN);
				intersections = 1;
			}
			else
			{
				// Two solutions.
				t = (float)((-B + Math.Sqrt(det)) / (2 * A));
				intersection1 = new PointD(line.P1.X + t * dx, line.P1.Y + t * dy);
				t = (float)((-B - Math.Sqrt(det)) / (2 * A));
				intersection2 = new PointD(line.P1.X + t * dx, line.P1.Y + t * dy);
				intersections = 2;
			}
			return intersections;
		}

		public RectangleD GetBoundingRectangle()
		{
			return new RectangleD(((PointD)_center).GetPointAt(EarthGeo.NorthWest, _radius),
								  ((PointD)_center).GetPointAt(EarthGeo.NorthEast, _radius),
								  ((PointD)_center).GetPointAt(EarthGeo.SouthEast, _radius),
								  ((PointD)_center).GetPointAt(EarthGeo.SouthWest, _radius));
		}

		#endregion

		#region Utility

		public static bool TryParse(String str, out Circle circle)
		{
			circle = null;

			String[]	parts = str.Split('-');
			PointD		p;
			Double		r;
			if(parts.Length == 2 && PointD.TryParse(parts[0].Trim(), out p) && Parser.TryParse(parts[1].Trim(), out r))
			{
				circle = new Circle(p, r);
			}

			return circle != null;
		}

		public Circle Clone()
		{
			return new Circle(Center.Clone(), _radius);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if(obj is Circle == false)
			{
				throw new InvalidCastException("Other object is not Circle");
			}
			return Center.Equals(((Circle)obj).Center) && Radius.Equals(((Circle)obj).Radius);
		}

		public override string ToString()
		{
			return String.Format("{0} - {1:0.00}", _center, _radius);
		}

		#endregion
	}
}
