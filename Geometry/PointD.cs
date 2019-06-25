using KanoopCommon.CommonObjects;
using KanoopCommon.Conversions;
using KanoopCommon.Database;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System;
using System.IO;

namespace KanoopCommon.Geometry
{

    public class PointDListList : List<PointDList> { };

	[Serializable]
	public class PointDList : List<PointD>
	{
		#region Constructors

		public PointDList()
			: base() {}

		public PointDList(IEnumerable<PointD> points)
			: this()
		{
			AddRange(points);
		}

		#endregion

		#region Conversions

		public Point[] ToPointArray()
		{
			List<Point> points = new List<Point>();
			foreach(PointD point in this)
			{
				points.Add(point.ToPoint());
			}
			return points.ToArray();
		}

		public PointF[] ToPointFArray()
		{
			List<PointF> points = new List<PointF>();
			foreach(PointD point in this)
			{
				points.Add(point.ToPointF());
			}
			return points.ToArray();
		}

		public LineList ToLineList()
		{
			LineList list = new LineList();

			for(int x = 0; x < Count - 1; x++)
			{
				list.Add(new Line(this[x], this[x + 1]));
			}
			return list;
		}

		public static Unescaped ToUnescapedSQLString(PointDList pnts)
		{
			Unescaped retVal = Unescaped.String("NULL");
			if (pnts != null & pnts.Count > 0)
			{
				retVal = Unescaped.String(pnts.ToSQLString());
			}
			return retVal;
		}

		public Unescaped ToSQLString()
		{
			String	strReturn = "PolyFromText('POLYGON((";
			string	strComma = "";

			PointD	ptFirst = this[0];
			strReturn += ptFirst.X + " " + ptFirst.Y;

			strComma = ",";

			for(int i = 1; i < this.Count; i++)
			{
				strReturn += strComma + this[i].X + " " + this[i].Y;
				strComma = (ptFirst == this[i]) ? "" : ", ";
			}
			if (this.Count > 0)
			{
				int intLastIndex = this.Count - 1;
				if ((this[0].X != this[intLastIndex].X) ||
				    (this[0].Y != this[intLastIndex].Y))
				{
					strReturn += strComma + this[0].X + " " + this[0].Y;
				}
			}
			strReturn += "))')";
			return Unescaped.String(strReturn);
		}

		#endregion

		#region Obsolete Methods

		[Obsolete("Replaced by Polygon")]
		public void ClosePolygon()
		{
			// Close off polygon
			if (this.Count > 0)
			{
				int intLastIndex = this.Count - 1;
				if( (this[0].X != this[intLastIndex].X) &&
				    (this[0].Y != this[intLastIndex].Y))
				{
					this.Add(this[0]);
				}
			}
		}

		[Obsolete("Replaced by Polygon")]
		public Double Area
		{
			get
			{
				int		i = 0;
				int		j = this.Count - 1;
				Double	area = 0;

				for (i = 0; i < this.Count; j=i++)
				{
					area += this[i].X * this[j].Y;
					area -= this[i].Y * this[j].X;
				}
				area /= 2.0;

				return (area);
			}
		}

		public PointD Centroid
		{
			get
			{
				PointD ret = null;
				if(Count > 0)
				{
					List<Double> xvalues = new List<Double>();
					List<Double> yvalues = new List<Double>();
					foreach(PointD point in this)
					{
						xvalues.Add(point.X);
						yvalues.Add(point.Y);
					}
					ret = new PointD(xvalues.Average(), yvalues.Average());
				}
				else
				{
					ret = new PointD();
				}
				return ret;
			}
		}

		#endregion

		#region Utility

		public PointDList Clone()
		{
			PointDList list = new PointDList();
			this.ForEach(p => list.Add(p.Clone() as PointD));
			return list;
		}

		#endregion
	}

	[Serializable]	// Needed for Web Client
	public class PointD
	{
		#region Constants

		public const int ByteArraySize = sizeof(Double) + sizeof(Double);

		#endregion

		#region Public Properties

		Double  _X;
		[ColumnName("X_POS","LONGITUDE")]
		public Double X
		{
			get { return _X; }
			set { _X = value; }
		}

		Double	_Y;
		[ColumnName("Y_POS", "LATITUDE")]
		public Double Y
		{
			get { return _Y; }
			set { _Y = value; }
		}

		int		_precision;
		public int Precision
		{
			get { return _precision; }
			set
			{
				_precision = value;
				_X = Math.Round(_X, value);
				_X = Math.Round(_X, value);
			}
		}

		public String Name { get; set; }

		public String HashName { get { return String.Format("{0:0.000000}, {1:0.000000}", _X, _Y); } }

		#endregion

		#region Constructors

		public PointD()
			: this(0, 0)	{}

		public PointD(Point p)
			: this(p.X, p.Y) {}

		public PointD(int x, int y)
			: this((Double)x, (Double)y) {}

		public PointD(PointD p)
			: this(p.X, p.Y) {}

		public PointD(GeoPoint p)
			: this(p.X, p.Y) { }

		public PointD(Double x, Double y)
		{
			_X = x;
			_Y = y;
			Name = String.Empty;
		}

		public PointD(byte[] serialized)
		{
			using(BinaryReader br = new BinaryReader(new MemoryStream(serialized)))
			{
				_X = br.ReadDouble();
				_Y = br.ReadDouble();
			}
		}

		public static PointD UpperLeftFromCenter(PointD center, Size size)
		{
			PointD point = new PointD(center.X - size.Width / 2, center.Y - size.Height / 2);
			return point;
		}

		/// <summary>
		/// Given a rectangle, return the upper left which will cause it to be centered
		/// </summary>
		/// <param name="rect"></param>
		/// <returns></returns>
		public static PointD FindCenterUpperLeft(Rectangle rect)
		{
			return new PointD(rect.Left - (rect.Width / 2), rect.Top - (rect.Height / 2));
		}

		public static PointD FindCenterUpperLeft(PointD from, SizeF size)
		{
			return new PointD(from.X - (size.Width / 2), from.Y - (size.Height / 2));
		}

		#endregion

		#region Public Geometry Methods

		public static PointD RelativeTo(PointD point, Double bearing, Double distance)
		{
			PointD ret = point.Clone() as PointD;
			ret.Move(bearing, distance);
			return ret;
		}

		public PointD Round(Int32 places)
		{
			return new PointD(Math.Round(X, places), Math.Round(Y, places));
		}

		public void Move(Double bearing, Double distance)
		{
			PointD np = FlatGeo.GetPoint(this, bearing, distance);
			_X = np.X;
			_Y = np.Y;
		}

		public void Move(PointD where)
		{
			_Y = where.Y;
			_X = where.X;
		}

		public void Rotate(PointD centroid, Double angle)
		{
			_X += (0 - centroid.X);
			_Y += (0 - centroid.Y);

			PointD np = new PointD();
			np.X = ((_X * Math.Cos(angle * (Math.PI / 180))) - (_Y * Math.Sin(angle * (Math.PI / 180)))) + centroid.X;
			np.Y = (Math.Sin(angle * (Math.PI / 180)) * _X + Math.Cos(angle * (Math.PI / 180)) * _Y) + centroid.Y;

			_X = np.X;
			_Y = np.Y;
		}

		public void Scale(Double scale)
		{
			_X *= scale;
			_Y *= scale;
		}

		public PointD GetPointAt(BearingAndRange vector)
		{
			return FlatGeo.GetPoint(this, vector.Bearing, vector.Range);
		}

		public PointD GetPointAt(Double bearing, Double distance)
		{
			return FlatGeo.GetPoint(this, bearing, distance);
		}

		public static PointD Scale(PointD point, Double scale)
		{
			return new PointD(point.X * scale, point.Y * scale);
		}

		public bool IsLeftOf(PointD other)
		{
			return _X < other._X;
		}

		public bool IsRightOf(PointD other)
		{
			return _X > other._X;
		}

		public bool IsAbove(PointD other)
		{
			return _Y < other._Y;
		}

		public bool IsLowerThan(PointD other)
		{
			return _Y > other._Y;
		}

		#endregion

		#region Utility

		public PointD Offset(Double x, Double y)
		{
			return new PointD(X + x, Y + y);
		}

		public static PointD Empty
		{
			get { return new PointD(0, 0); }
		}

		public Unescaped ToUnescapedSQLString()
		{
			return ToUnescapedSQLString(this);
		}

		public static Unescaped ToUnescapedSQLString(PointD pnt)
		{
			Unescaped retVal = Unescaped.String("NULL");
			if (pnt != null)
			{
				retVal = Unescaped.String(pnt.ToSQLString());
			}
			return retVal;
		}

		public Unescaped ToSQLString()
		{
			//return String.Format("GeomFromText('POINT({0:0.000000} {1:0.000000})')", m_X, m_Y);
			return Unescaped.String(String.Format("PointFromText('POINT({0} {1})')", _X, _Y));
		}

		public Double BearingTo(PointD other)
		{
			Line line = new Line(this, other);
			return line.Bearing;
		}

		public BearingAndRange BearingAndRangeTo(PointD other)
		{
			Line line = new Line(this, other);
			return new BearingAndRange(line.Bearing, line.Length);
		}

		public Double DistanceTo(PointD other)
		{
			Line line = new Line(this, other);
			return line.Length;
		}

		public Point ToPoint()
		{
			return new Point((int)Math.Round(X), (int)Math.Round(Y));
		}

		public PointF ToPointF()
		{
			return new PointF((float)X, (float)Y);
		}

		public GeoPoint ToGeoPoint()
		{
			return new GeoPoint(this);
		}

		public PointD ToPointD()
		{
			return new PointD(this);
		}

		public static bool TryParse(String str, out PointD point)
		{
			point = null;

			String[]	parts = str.Split(',');
			Double		x, y;
			if(parts.Length == 2 && Parser.TryParse(parts[0], out x) && Parser.TryParse(parts[1], out y))
			{
				point = new PointD(x, y);
			}

			return point != null;
		}

		public bool Equals(PointD other)
		{
			return Equals(other, 0);
		}

		public bool Equals(PointD other, int precision = 0)
		{
			bool result = false;
			if(precision == 0)
				result = X == other.X && Y == other.Y;
			else
				result = Math.Round(X, precision) == Math.Round(other.X, precision) && Math.Round(Y, precision) == Math.Round(other.Y, precision);
			return result;
		}

		public override int GetHashCode()
		{
			return PointCommon.GetHashCode(this);
		}

		public override bool Equals(object other)
		{
			return PointCommon.Equals(this, other);
		}

		public int CompareTo(PointD other)
		{
			return PointCommon.ComparePoints(this, other, Precision);
		}

		public PointD Clone()
		{
			return new PointD(_X, _Y);
		}

		public byte[] Serialize()
		{
			byte[] output = new byte[ByteArraySize];
			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(output)))
			{
				bw.Write(X);
				bw.Write(Y);
			}
			return output;
		}

		public String ToString(int precision)
		{
			String	format = String.Format("F{0}", precision);
			String	name = String.IsNullOrEmpty(Name) ? String.Empty : String.Format(" {0}", Name);
			return String.Format("{0}, {1}{2}", _X.ToString(format), _Y.ToString(format), name);
		}

		public override string ToString()
		{
			return ToString(2);
		}

		#endregion
	}
}
