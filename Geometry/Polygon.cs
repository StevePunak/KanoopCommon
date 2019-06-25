using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System;
using KanoopCommon.Conversions;

namespace KanoopCommon.Geometry
{
	public class Polygon
	{
		#region Public Properties

		protected LineList _lines;
		public List<Line> Lines
		{
			get
			{
				_lines = new LineList();
				if(_points.Count > 0)
				{
					PointD previous = null;

					foreach(PointD point in _points)
					{
						if(previous != null)
						{
							_lines.Add(new Line(previous, point));
						}
						previous = point;
					}
					_lines.Add(new Line(previous, _points[0]));
				}
				return new List<Line>(_lines);
			}
		}

		protected PointDList _points;
		public List<PointD> Points { get { return new List<PointD>(_points); } }

		public PointDList ToPointDList()
		{
			PointDList points = new PointDList(_points);
			return points;
		}

		public Double Area
		{
			get
			{
				int		i = 0;
				int		j = _points.Count - 1;
				double	area = 0;

				for (i = 0; i < _points.Count; j=i++)
				{
					area += _points[i].X * _points[j].Y;
					area -= _points[i].Y * _points[j].X;
				}
				area /= 2.0;

				return (area);
			}
		}

		#endregion

		#region Constructors

		public Polygon()
			: this(new LineList()) {}

		public Polygon(LineList lines)
		{
			CreateFromLines(lines);
		}

		public Polygon(List<PointD> points)
			: this(new PointDList(points)) {}

		public Polygon(PointDList points)
		{
			_points = points;
		}

		#endregion

		#region Public Geometry Methods

		public void AddVertice(PointD point)
		{
			_points.Add(point);
		}

		public void Move(double bearing, double distance)
		{
			foreach(PointD point in _points)
			{
				point.Move(bearing, distance);
			}
		}

		public void Rotate(PointD centroid, double angle)
		{
			LineList lines = _lines.Clone();

			foreach(Line line in lines)
			{
				line.Rotate(centroid, angle);
			}

			CreateFromLines(lines);
		}

		public PointD CenterPoint	// DEPRECATE FOR CENTROID
		{
			get
			{
				PointD retPoint = new PointD();
				if(Lines.Count > 3)
				{
					Line	l1, l2;
					Line	line;

					line = _lines[0];
					l1 = new Line(line.P1, new PointD());

					line = _lines[1];
					l2 = new Line(line.P1, new PointD());

					line = _lines[2];
					l1 = new Line(l1.P1, line.P1);

					line = _lines[3];
					l2 = new Line(l2.P1, line.P1);

					FlatGeo.GetIntersection(l1, l2, out retPoint);
				}
				else if(Lines.Count == 3)
				{
					Line	l1 = new Line(Lines[0].P1, Lines[1].MidPoint);
					Line	l2 = new Line(Lines[1].P1, Lines[2].MidPoint);
					FlatGeo.GetIntersection(l1, l2, out retPoint);
				}
				return retPoint;
			}
		}

		public Double MinimumXY
		{
			get
			{
				Double min = Double.MaxValue;

				foreach(PointD point in Points)
				{
					if(point.X < min)
						min = point.X;
					if(point.Y < min)
						min = point.Y;
				}
				return min;
			}
		}

		public PointD Centroid
		{
			get
			{
				PointD	centroid = new PointD();
				double	signedArea = 0.0;
				double	x0 = 0.0;	// Current vertex X
				double	y0 = 0.0;	// Current vertex Y
				double	x1 = 0.0;	// Next vertex X
				double	y1 = 0.0;	// Next vertex Y
				double	a = 0.0;// Partial signed area

				// For all vertices except last
				int		i = 0;

				for (i = 0; i < Points.Count - 1; ++i)
				{
					x0 = Points[i].X;
					y0 = Points[i].Y;
					x1 = Points[i + 1].X;
					y1 = Points[i + 1].Y;
					a = x0 * y1 - x1 * y0;
					signedArea += a;
					centroid.X += (x0 + x1) * a;
					centroid.Y += (y0 + y1) * a;
				}

				// Do last vertex
				x0 = Points[i].X;
				y0 = Points[i].Y;
				x1 = Points[0].X;
				y1 = Points[0].Y;
				a = x0 * y1 - x1 * y0;
				signedArea += a;
				centroid.X += (x0 + x1) * a;
				centroid.Y += (y0 + y1) * a;

				signedArea *= 0.5;
				centroid.X /= (6 * signedArea);
				centroid.Y /= (6 * signedArea);

				return centroid;
			}
		}

		protected LineList GetLinesAdjacentTo(PointD p)
		{
			LineList ret = new LineList() { new Line(), new Line() };

			foreach(Line l in Lines)
			{
				if(l.P1.Equals(p))
				{
					ret[1] = l.Clone() as Line;
				}

				if(l.P2.Equals(p))
				{
					ret[0] = new Line(l.P2, l.P1);
				}
			}
			return ret;
		}

		public virtual bool Contains(PointD point)
		{
			PointD		p1, p2;

			bool		inside = false;

			PointDList	poly = _points;

			PointD		oldPoint = poly[Points.Count - 1];

			for (int i = 0; i < poly.Count; i++)
			{
				PointD newPoint = poly[i];

				if (newPoint.X > oldPoint.X)
				{
					p1 = oldPoint;

					p2 = newPoint;
				}

				else
				{
					p1 = newPoint;

					p2 = oldPoint;
				}

				if ((newPoint.X < point.X) == (point.X <= oldPoint.X)
				    && (point.Y - (long)p1.Y) * (p2.X - p1.X)
				    < (p2.Y - (long)p1.Y) * (point.X - p1.X))
				{
					inside = !inside;
				}

				oldPoint = newPoint;
			}

			return inside;
		}

		public RectangleD GetBoundingRectangle()
		{
			float	leftMost = float.MaxValue;
			float	rightMost = float.MinValue;
			float	topMost = float.MaxValue;
			float	bottomMost = float.MinValue;

			foreach(Line line in Lines)
			{
				foreach(PointD point in line.Points)
				{
					if(point.X < leftMost)
						leftMost = (float)point.X;
					if(point.X > rightMost)
						rightMost = (float)point.X;
					if(point.Y < topMost)
						topMost = (float)point.Y;
					if(point.Y > bottomMost)
						bottomMost = (float)point.Y;
				}
			}
			return new RectangleD(leftMost, topMost, rightMost - leftMost, bottomMost - topMost);
		}

		void CreateFromLines(LineList lines)
		{
			_points = new PointDList();

			foreach(Line line in lines)
			{
				_points.Add(line.P1 as PointD);
			}
		}

		#endregion

		#region Utility

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			foreach(Line line in Lines)
			{
				sb.AppendFormat("(x={0},y={1}), ", (int)line.P1.X, (int)line.P1.Y);
			}
			return sb.ToString().TrimEnd(new char[] {',', ' '});
		}

		public bool Equals(Polygon other)
		{
			bool result = false;
			if(Lines.Count == other.Lines.Count)
			{
				result = true;

				for(int x = 0; x < Lines.Count; x++)
				{
					if(Lines[x].Equals(other.Lines[x]) == false)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

        public void Offset(Offset offset)
        {
            for (int pointNumber = 0; pointNumber < _points.Count; pointNumber++)
            {
                _points[pointNumber].X += offset.DeltaX;
                _points[pointNumber].Y += offset.DeltaY;
            }
        }

        public void Offset(PointD offset)
        {
            for (int pointNumber = 0; pointNumber < _points.Count; pointNumber++)
            {
                _points[pointNumber].X += offset.X;
                _points[pointNumber].Y += offset.Y;
            }
        }
		#endregion
	}
}

