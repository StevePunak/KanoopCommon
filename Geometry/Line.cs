using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System;
using System.ComponentModel;
using KanoopCommon.Extensions;
using KanoopCommon.CommonObjects;
using KanoopCommon.Logging;
using System.IO;

namespace KanoopCommon.Geometry
{
	public class LineList : List<Line>, IEnumerable<Line>
	{
		#region Constructors

		public LineList()
			: base() {}

		public LineList(IEnumerable<Line> lines)
			: base(lines) {}

		public LineList(PointDList points)
			: this(points.ToLineList()) {}

		#endregion

		#region Public Geometry Methods

		public void RemoveShorterThan(Double length)
		{
			LineList delete = new LineList();
			foreach(Line line in this)
			{
				if(line.Length < length)
				{
					delete.Add(line);
				}
			}
			foreach(Line line in delete)
			{
				Remove(line);
			}
		}

		public PointD ClosestPointTo(PointD other, out Line closestLine, out Double closestDistance)
		{
			closestDistance = Double.MaxValue;
			PointD closestPoint = null;
			closestLine = null;

			foreach(Line line in this)
			{
				Double	distance;
				PointD	p = line.ClosestPointTo(other, out distance);
				if(distance < closestDistance)
				{
					closestDistance = distance;
					closestPoint = p;
					closestLine = line;
				}
			}
			return closestPoint;
		}

		public Double AverageBearing
		{
			get
			{
				List<Double> bearings = new List<Double>();
				Double firstBearing = this[0].Bearing;

				bearings.Add(firstBearing);

				for(int x = 1;x < this.Count;x++)
				{
					Double bearing = this[x].Bearing.AngularDifference(firstBearing) < this[x].Bearing.AngularDifference(firstBearing.AddDegrees(180))
						? this[x].Bearing : this[x].Bearing.AddDegrees(180);
					bearings.Add(bearing);
				}

				return bearings.Average();
			}
		}

		public Line LeftMost
		{
			get
			{
				Line result = null;
				foreach(Line line in this)
				{
					if(result == null || line.IsLeftOf(result))
					{
						result = line;
					}
				}
				return result;
			}
		}

		public Line RightMost
		{
			get
			{
				Line result = null;
				foreach(Line line in this)
				{
					if(result == null || line.IsRightOf(result))
					{
						result = line;
					}
				}
				return result;
			}
		}

		public Line UpperMost
		{
			get
			{
				Line result = null;
				foreach(Line line in this)
				{
					if(result == null || line.IsAbove(result))
					{
						result = line;
					}
				}
				return result;
			}
		}

		public Line LowerMost
		{
			get
			{
				Line result = null;
				foreach(Line line in this)
				{
					if(result == null || line.IsBelow(result))
					{
						result = line;
					}
				}
				return result;
			}
		}

		public Line Shortest
		{
			get
			{
				Line line = this.Aggregate((l1, l2) => l1.Length < l2.Length ? l1 : l2);
				return line;
			}
		}

		/// <summary>
		/// Does any line in the list contain a line lying along the path of any line in the given list
		/// </summary>
		/// <param name="other"></param>
		/// <param name="pathWidth"></param>
		/// <returns></returns>
		public bool ContainsLinesAlongThePathOf(LineList other, double pathWidth, Double extend = 0)
		{
			bool result = false;
			for(int x = 0;x < Count && result == false;x++)
			{
				foreach(Line otherLine in other)
				{
					RectangleD rect1, rect2;
					if(this[x].LiesAlongThePathOf(otherLine, pathWidth, out rect1, out rect2, extend))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region Utility

		public void SortUpperLeft()
		{
			Sort(new PointUpperLeftComparer());
		}

		class PointUpperLeftComparer : IComparer<Line>
		{
			public int Compare(Line x, Line y)
			{
				int result = 0;
				int r1 = x.P1.X.CompareTo(y.P1.X);
				int r2 = x.P1.Y.CompareTo(y.P1.Y);
				if(r1 < 0 && r2 < 0)
					result = -1;
				else if(r1 > 0 && r2 > 0)
					result = 1;
				else if(r1 < 0 && r2 > 0)
					result = -1;
				else if(r1 > 0 && r2 < 0)
					result = 1;
				return result;
			}
		}

		public LineList Clone()
		{
			LineList lines = new LineList();

			foreach(Line line in this)
			{
				lines.Add(new Line(line.P1.Clone(), line.P2.Clone()));
			}
			return lines;
		}

		public void DumpToLog()
		{
			foreach(Line line in this)
			{
				Log.SysLogText(LogLevel.DEBUG, "{0}", line);
			}
		}

		public void RemoveInvalid()
		{
			this.RemoveAll(l => l.P1.Equals(l.P2, 2));
		}

		#endregion
	}

	public class Line
	{
		#region Constants

		public const int ByteArraySize = PointD.ByteArraySize * 2;

		#endregion

		#region Public Properties

		protected PointD	_p1;
		public PointD P1
		{
			get { return _p1; }
			set { _p1 = value; }
		}

		protected PointD	_p2;
		public PointD P2
		{
			get { return _p2; }
			set { _p2 = value as PointD; }
		}

		public List<PointD> Points { get { return new List<PointD>(){ _p1, _p2 }; } }

		public PointD MidPoint
		{
			get
			{
				return new PointD(	_p2.X - ((_p2.X - _p1.X) / 2),
				                    _p2.Y - ((_p2.Y - _p1.Y) / 2));
			}
		}

		public virtual Double Length
		{
			get
			{
				Double	x = _p1.X - _p2.X;
				Double	y = _p1.Y - _p2.Y;
				return(Math.Sqrt((x*x) + (y*y)));
			}
		}

		public String Name { get; set; }

		public Double Slope
		{
			get
			{
				Double	ret = 0;
				Double	y = _p2.Y - _p1.Y;
				Double	x = _p2.X - _p1.X;
				if(y == 0)
				{
					ret = Double.PositiveInfinity;
					_vertical = false;
					_horizontal = true;
				}
				else if(x == 0)
				{
					ret = Double.NegativeInfinity;
					_vertical = true;
					_horizontal = false;
				}
				else
				{
					ret = y / x;
				}
				return ret;
			}
		}

		public Double Intercept
		{
			get
			{
				Double	ret = Double.NaN;
				Double	slope = Slope;
				/** not vertical */
				if(ret != Double.NegativeInfinity)
				{
					ret = _p1.Y - (_p1.X * slope);
				}
				return ret;
			}
		}

		bool	_vertical;
		public bool IsVertical
		{
			get
			{
				if(Slope == Double.PositiveInfinity) {}
				return _vertical;
			}
		}

		bool	_horizontal;
		public bool IsHorizontal
		{
			get
			{
				if(Slope == Double.PositiveInfinity) {}
				return _horizontal;
			}
		}

		public Double Bearing
		{
			get
			{

				Double xDiff = P2.X - P1.X;
				Double yDiff = P2.Y - P1.Y;
				Double b = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);

				/**
				 * get a new line and move it down one unit
				 */
				Line    newLine = this.Clone() as Line;
				newLine.Move(180, 1);

				Line	l = new Line(newLine.P1, new PointD(newLine.P1.X, newLine.P1.Y - 1));
				Double	d = FlatGeo.VectorAngle(l, newLine);
				Angle	angle = new Angle(d);
				if(_p2.X < _p1.X)
				{
					angle.Degrees = 360 - angle.Degrees;
				}
				//Line l = new Line(m_P1, new PointD(m_P1.X + 100, m_P1.Y));
				//Angle angle = new Angle(FlatGeo.VectorAngle(l, this));

				return angle.Degrees;
			}
		}

		public Object Tag { get; set; }

		#endregion

		#region Constructors

		public Line(PointD p1, PointD p2)
		{
			_p1 = p1;
			_p2 = p2;
			Tag = null;
		}

		public Line(GeoPoint p1, GeoPoint p2)
		{
			_p1 = p1.ToPointD();
			_p2 = p2.ToPointD();
			Tag = null;
		}

		public Line(Line line)
			: this(line.P1, line.P2) {}

		public Line(Double x1, Double y1, Double x2, Double y2)
			: this(new PointD(x1, y1), new PointD(x2, y2)) {}

		public Line(PointD origin, Double bearing, Double distance)
			: this(origin, FlatGeo.GetPoint(origin, bearing, distance)) {}

		public Line(Point p1, Point p2)
			: this(new PointD(p1), new PointD(p2)) {}

		public Line()
			: this(new PointD(0, 0), new PointD(0, 0))	{}

		public Line(byte[] serialized)
		{
			using(BinaryReader br = new BinaryReader(new MemoryStream(serialized)))
			{
				byte[] p1 = br.ReadBytes(PointD.ByteArraySize);
				byte[] p2 = br.ReadBytes(PointD.ByteArraySize);
				P1 = new PointD(p1);
				P2 = new PointD(p2);
			}
		}

		#endregion

		#region Public Geometery Metods

		public Double DistanceTo(PointD to)
		{
			Double	ret = Double.PositiveInfinity;

			/** make a single leg of a triangle between the remote point and one point on this line */
			Line	leg = new Line(P1, to);

			/** get the interior angle */
			Double	angle = FlatGeo.Angle(to, leg.P1, leg.P2).Degrees;
			if(angle >= 180)
			{
				angle -= 180;
			}

			if(angle != 0)
			{
				/** find the distance to the closest point on the line */
				ret = Math.Abs(Math.Sin(angle) * leg.Length);
			}
			else
			{
				/** the point is on the line */
				ret = 0;
			}

			return ret;
		}

		/// <summary>
		/// Get a rectangle which contains a path along the given line 'pathWidth' wide
		/// </summary>
		/// <param name="pathWidth"></param>
		/// <returns></returns>
		public RectangleD GetPathRectangle(Double pathWidth, Double extend)
		{
			PointD p1 = ((PointD)P1).GetPointAt(Bearing.AddDegrees(90), pathWidth / 2) as PointD;
			PointD p2 = ((PointD)P1).GetPointAt(Bearing.SubtractDegrees(90), pathWidth / 2) as PointD;
			PointD p3 = ((PointD)P2).GetPointAt(Bearing.SubtractDegrees(90), pathWidth / 2) as PointD;
			PointD p4 = ((PointD)P2).GetPointAt(Bearing.AddDegrees(90), pathWidth / 2) as PointD;

			if(extend > 0)
			{
				p1 = p1.GetPointAt(Bearing.AddDegrees(180), extend) as PointD;
				p2 = p2.GetPointAt(Bearing.AddDegrees(180), extend) as PointD;
				p3 = p3.GetPointAt(Bearing, extend) as PointD;
				p4 = p4.GetPointAt(Bearing, extend) as PointD;
			}

			return new RectangleD(p1, p2, p3, p4);
		}

		/// <summary>
		/// Does either endpoint of the given line fall along a 'pathWidth' path of this line,
		/// or does either endpoint of this line fall along the path of the given line
		/// </summary>
		/// <param name="line"></param>
		/// <param name="pathWidth"></param>
		/// <returns></returns>
		public bool LiesAlongThePathOf(Line line, Double pathWidth, out RectangleD rect1, out RectangleD rect2, Double extend = 0)
		{
			bool result = false;
			rect2 = null;

			// get a rectangle which contains the path of the given line
			rect1 = line.GetPathRectangle(pathWidth, extend);
			if((result = rect1.Contains(P1 as PointD)) == false && (result = rect1.Contains(P2 as PointD)) == false)
			{
				rect2 = GetPathRectangle(pathWidth, extend);
				if((result = rect2.Contains(line.P1 as PointD)) == false && (result = rect2.Contains(line.P2 as PointD)) == false)
				{
					// wil return false;
				}
			}
			return result;
		}

		public static Line ConsolidateLongest(Line l1, Line l2)
		{
			Line line = null;
			PointD p = l1.P1 as PointD;
			List<Double> distances = new List<double>()
			{
				((PointD)l1.P1).DistanceTo(((PointD)l2.P1)),
				((PointD)l1.P1).DistanceTo(((PointD)l2.P2)),
				((PointD)l1.P2).DistanceTo(((PointD)l2.P1)),
				((PointD)l1.P2).DistanceTo(((PointD)l2.P2))
			};
			Double max = distances.Max();
			if(max > l1.Length && max > l2.Length)
			{
				if(max == distances[0])
					line = new Line(l1.P1, l2.P1);
				else if(max == distances[1])
					line = new Line(l1.P1, l2.P2);
				else if(max == distances[2])
					line = new Line(l1.P2, l2.P1);
				else if(max == distances[3])
					line = new Line(l1.P2, l2.P2);
			}
			else
			{
				line = l1.Length > l2.Length ? l1 : l2;
			}
			return line;
		}

		public PointD FurthestPointFrom(PointD pt)
		{
			Double distance;
			PointD closest = ClosestPointTo(pt, out distance);
			PointD furthest = closest.Equals(P1) ? P2 : P1;
			return furthest;
		}

		public PointD ClosestPointTo(PointD pt)
		{
			Double distance;
			return ClosestPointTo(pt, out distance);
		}

		public bool HasXBetween(Double x1, Double x2)
		{
			return P1.X.Between(x1, x2) || P2.X.Between(x1, x2);
		}

		public bool HasYBetween(Double y1, Double y2)
		{
			return P1.Y.Between(y1, y2) || P2.Y.Between(y1, y2);
		}

		public PointD ClosestPointTo(PointD pt, out Double distance)
		{
			PointD	closest;
			Double	dx = P2.X - P1.X;
			Double	dy = P2.Y - P1.Y;

			// Calculate the t that minimizes the distance.
			Double	t = ((pt.X - P1.X) * dx + (pt.Y - P1.Y) * dy) / (dx * dx + dy * dy);

			// See if this represents one of the segment's
			// end points or a point in the middle.
			if (t < 0)
			{
				closest = new PointD(P1.X, P1.Y);
				dx = pt.X - P1.X;
				dy = pt.Y - P1.Y;
			}
			else if (t > 1)
			{
				closest = new PointD(P2.X, P2.Y);
				dx = pt.X - P2.X;
				dy = pt.Y - P2.Y;
			}
			else
			{
				closest = new PointD(P1.X + t * dx, P1.Y + t * dy);
				dx = pt.X - closest.X;
				dy = pt.Y - closest.Y;
			}

			distance = Math.Sqrt(dx * dx + dy * dy);

			return closest;
		}

		public void Move(Double bearing, Double distance)
		{
			_p1.Move(bearing, distance);
			_p2.Move(bearing, distance);
		}

		public void Rotate(PointD centroid, Double angle)
		{
			_p1.Rotate(centroid, angle);
			_p2.Rotate(centroid, angle);
		}

		public bool Intersects(Line other, out PointD intersection)
		{
			intersection = null;
			return FlatGeo.GetIntersection(this, other, out intersection);
		}

		public bool Intersects(Line other)
		{
			PointD intersection;
			return Intersects(other, out intersection);
		}

		public bool Intersects(Circle circle)
		{
			return circle.Intersects(this);
		}

		public bool SharesEndPointWith(Line other, int precision = 0)
		{
			return
			    other.P1.Equals(P1, precision) ||
			    other.P1.Equals(P2, precision) ||
			    other.P2.Equals(P1, precision) ||
			    other.P2.Equals(P2, precision);
		}

		public bool IsEndPoint(PointD point, int precision = 0)
		{
			return _p1.Equals(point, precision) || _p2.Equals(point, precision);
		}

		public void GetABC(out Double A, out Double B, out Double C)
		{
			A = B = C = 0;

			A = P2.Y - P1.Y;
			B = P1.X - P2.X;
			C = A * P1.X + B * P1.Y;
		}

		public bool IsLeftOf(Line other)
		{
			Double minL1 = Math.Min(P1.X, P2.X);
			Double minL2 = Math.Min(other.P1.X, other.P2.X);

			return minL1 < minL2;
		}

		public bool IsRightOf(Line other)
		{
			Double maxL1 = Math.Max(P1.X, P2.X);
			Double maxL2 = Math.Max(other.P1.X, other.P2.X);

			return maxL1 > maxL2;
		}

		public bool IsAbove(Line other)
		{
			Double minL1 = Math.Min(P1.Y, P2.Y);
			Double minL2 = Math.Min(other.P1.Y, other.P2.Y);

			return minL1 < minL2;
		}

		public bool IsBelow(Line other)
		{
			Double maxL1 = Math.Max(P1.Y, P2.Y);
			Double maxL2 = Math.Max(other.P1.Y, other.P2.Y);

			return maxL1 > maxL2;
		}

		public bool SharesEndPointWith(Line other, Double distance)
		{
			bool result =
				((PointD)P1).DistanceTo(other.P1 as PointD) <= distance ||
				((PointD)P1).DistanceTo(other.P2 as PointD) <= distance ||
				((PointD)P2).DistanceTo(other.P1 as PointD) <= distance ||
				((PointD)P2).DistanceTo(other.P2 as PointD) <= distance;
			return result;
		}

		public bool SharesEndpointAndBearing(Line other, Double distance, Double bearingSlack)
		{
			bool result =
				SharesEndPointWith(other, distance) &&
				(Bearing.AngularDifference(other.Bearing) <= bearingSlack || Bearing.AddDegrees(180).AngularDifference(other.Bearing) <= bearingSlack);
			return result;

		}

		#endregion

		#region Utility

		public Line Clone()
		{
			return new Line(_p1.Clone(), _p2.Clone());
		}

		public bool Equals(Line l)
		{
			return (l.P1.X == P1.X && l.P1.Y == P1.Y && l.P2.X == P2.X && l.P2.Y == P2.Y);
		}

		public String ToString(int precision)
		{
			String result = Tag == null
				? String.Format("[{0}] - [{1}]  {2:0.0}°  len: {3:0.000}", _p1.ToString(precision), _p2.ToString(precision), Bearing, Length)
				: String.Format("[{0}]  [{1}] - {2}  {3:0.0}°  len: {4:0.000}", Tag, _p1.ToString(precision), _p2.ToString(precision), Bearing, Length);

			return result;
		}

		public byte[] Serialize()
		{
			if(P1 is PointD == false || P2 is PointD == false)
			{
				throw new InvalidCastException("Points must be POINTD");
			}
			byte[] output = new byte[ByteArraySize];
			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(output)))
			{
				bw.Write(((PointD)P1).Serialize());
				bw.Write(((PointD)P2).Serialize());
			}
			return output;
		}

		public override string ToString()
		{
			return ToString(0);
		}

		#endregion
	}
}
