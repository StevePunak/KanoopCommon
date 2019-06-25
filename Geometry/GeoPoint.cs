using KanoopCommon.CommonObjects;
using KanoopCommon.Conversions;
using KanoopCommon.Database;
using KanoopCommon.Extensions;
using KanoopCommon.Types;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System;
using KanoopCommon.Crypto;
using System.Diagnostics;

namespace KanoopCommon.Geometry
{
	public class GeoPointList : List<GeoPoint>
	{
		#region Constructors

		public GeoPointList()
			: base() {}

		public GeoPointList(IEnumerable<GeoPoint> list)
		{
			foreach (GeoPoint pt in list)
			{
				GeoPoint point = new GeoPoint(pt);
				this.Add(new GeoPoint(pt));
			}
		}

		public GeoPointList(IEnumerable<PointD> list)
		{
			foreach(PointD pt in list)
			{
				GeoPoint point = new GeoPoint(pt);
				this.Add(new GeoPoint(pt));
			}
		}

		#endregion

		#region Relation To Ordinal Directions

		public GeoPoint NorthMost { get { return EarthGeo.NorthMost(this); } }

		public GeoPoint SouthMost { get { return EarthGeo.SouthMost(this); } }

		public GeoPoint EastMost { get { return EarthGeo.EastMost(this); } }

		public GeoPoint WestMost { get { return EarthGeo.WestMost(this); } }

		public GeoPoint NorthWest
		{
			get
			{
				GeoPointList	north = EarthGeo.NorthToSouth(this);
				GeoPointList	west = EarthGeo.WestToEast(this);
				return north[0].Equals(west[0]) || north[0].Equals(west[1])
				       ? north[0] : north[1];
			}
		}

		public GeoPoint NorthEast
		{
			get
			{
				GeoPointList	north = EarthGeo.NorthToSouth(this);
				GeoPointList	east = EarthGeo.EastToWest(this);
				return north[0].Equals(east[0]) || north[0].Equals(east[1])
				       ? north[0] : north[1];
			}
		}

		public GeoPoint SouthWest
		{
			get
			{
				GeoPointList	south = EarthGeo.SouthToNorth(this);
				GeoPointList	west = EarthGeo.WestToEast(this);
				return south[0].Equals(west[0]) || south[0].Equals(west[1])
				       ? south[0] : south[1];
			}
		}

		public GeoPoint SouthEast
		{
			get
			{
				GeoPointList	south = EarthGeo.SouthToNorth(this);
				GeoPointList	east = EarthGeo.EastToWest(this);
				return south[0].Equals(east[0]) || south[0].Equals(east[1])
				       ? south[0] : south[1];
			}
		}

		#endregion

		#region Utility

		public GeoLineList ToGeoLineList()
		{
			GeoLineList list = new GeoLineList();

			for(int x = 0; x < Count - 1; x++)
			{
				list.Add(new GeoLine(this[x], this[x + 1]));
			}
			return list;
		}

		public PointDList ToPointDList()
		{
			PointDList list = new PointDList();

			foreach(GeoPoint point in this)
			{
				list.Add(new PointD(point));
			}
			return list;
		}

		public override string ToString()
		{
			return String.Format("GeoPointList ({0} points)", Count);
		}

		#endregion

		#region Obsolete Methods
#if zero
		[Obsolete("Obsoleted for polygon")]
		public Double Area
		{
			get
			{
				// Convert to Pointlist
				PointDList		pixList = new PointDList();
				pixList.Add(new PointD(0,0));
				pixList.Add(new PointD(100,0));

				GeoPointList	geoList = new GeoPointList();
				geoList.Add(this[0]);
				geoList.Add(this[1]);

				CoordinateMap	map = new CoordinateMap(pixList, geoList);
				pixList.Clear();

				foreach (GeoPoint pt in this)
				{
					pixList.Add(map.GetPixelPoint(pt));
				}
				return pixList.Area / Math.Pow(map.PixelsPerMeter,2);
			}
		}

		[Obsolete("Obsoleted for polygon")]
		public GeoPoint Centroid
		{
			get
			{
				// Convert to Pointlist
				PointDList		pixList = new PointDList();
				pixList.Add(new PointD(0, 0));
				pixList.Add(new PointD(100, 0));

				GeoPointList	geoList = new GeoPointList();
				geoList.Add(this[0]);
				geoList.Add(this[1]);

				CoordinateMap	map = new CoordinateMap(pixList, geoList);
				pixList.Clear();

				foreach (GeoPoint pt in this)
				{
					pixList.Add(map.GetPixelPoint(pt));
				}

				Polygon			polygon = new Polygon(pixList);
				return map.GetGeoPoint(polygon.Centroid);
			}
		}
#endif
		#endregion
	}

	[Serializable]	// Needed for Web Client
	public class GeoPoint
	{
		#region Constants

		public const String TOSTRING_DELIM = "Lat/Long: ";

		#endregion

		#region Public Properties

		Double				m_Latitude;
		[ColumnName("Y_POS", "LATITUDE")]
		public Double Latitude
		{
			get { return m_Latitude; }
			set { m_Latitude = value; }
		}

		Double				m_Longitude;
		[ColumnName("X_POS","LONGITUDE")]
		public Double Longitude
		{
			get { return m_Longitude; }
			set { m_Longitude = value; }
		}

		public Double X { get { return m_Longitude; } }

		public Double Y { get { return m_Latitude; } }

		public String Name { get; set; }

		public String HashName { get { return String.Format("{0:0.000000}, {1:0.000000}", m_Longitude, m_Latitude); } }

		CardinalDirection	m_LatitudeCD;
		public CardinalDirection LatitudeCD
		{
			get { return m_LatitudeCD; }
			set { m_LatitudeCD = value; }
		}

		CardinalDirection	m_LongitudeCD;
		public CardinalDirection LongitudeCD
		{
			get { return m_LongitudeCD; }
			set { m_LongitudeCD = value; }
		}

		public bool IsEmpty { get { return this.Equals(GeoPoint.Empty); } }

		int m_Precision;
		public int Precision 
		{ 
			get { return m_Precision; }
			set
			{
				m_Precision = value;
				m_Latitude = Math.Round(m_Latitude, value);
				m_Longitude = Math.Round(m_Longitude, value);
			}
		}

		#endregion

		#region Constructors

		public GeoPoint()
			: this(0, 0) {}

		public GeoPoint(Double latitude, CardinalDirection latitudeDirection, Double longitude, CardinalDirection longitudeDirection)
		{
			m_Latitude = latitude;
			m_Longitude = longitude;
			m_LatitudeCD = latitudeDirection;
			m_LongitudeCD = longitudeDirection;
			m_Precision = EarthGeo.GeoPrecision;
		}

		public GeoPoint(Double latitude, Double longitude)
			: this(latitude, CardinalDirection.North, longitude, CardinalDirection.West) {}

		public GeoPoint(Double latitude, Double longitude, int precision)
			: this(new GeoPoint(latitude, longitude), precision) {}

		public GeoPoint(PointD point)
			: this(point.Y, point.X) {}

		public GeoPoint(GeoPoint point)
			: this(point.Latitude, point.Longitude) {}

		public GeoPoint(GeoPoint point, int precision)
			: this(point.Latitude, point.Longitude) 
		{
			Precision = precision;
		}

		#endregion

		#region Cardinal Direction Comparisons

		public bool IsNorthOfOrEqualTo(GeoPoint other, int precision = EarthGeo.GeoPrecision)
		{
			return IsNorthOf(other) || (Latitude.EqualsAtPrecision(other.Latitude, precision) && LatitudeCD == other.LatitudeCD);
		}

		public bool IsNorthOf(GeoPoint other)
		{
			bool result = false;
			if(this.LatitudeCD == CardinalDirection.North && other.LatitudeCD == CardinalDirection.North)
				result = this.Latitude > other.Latitude;
			else if(this.LatitudeCD == CardinalDirection.South && other.LatitudeCD == CardinalDirection.South)
				result = this.Latitude < other.Latitude;
			else
				result = this.LatitudeCD == CardinalDirection.North;
			return result;
		}

		public bool IsSouthOfOrEqualTo(GeoPoint other, int precision = EarthGeo.GeoPrecision)
		{
			return IsSouthOf(other) || (Latitude.EqualsAtPrecision(other.Latitude, precision) && LatitudeCD == other.LatitudeCD);
		}

		public bool IsSouthOf(GeoPoint other)
		{
			return !IsNorthOf(other);
		}

		public bool IsWestOfOrEqualTo(GeoPoint other, int precision = EarthGeo.GeoPrecision)
		{
			return IsWestOf(other) || Longitude.EqualsAtPrecision(other.Longitude, precision);
		}

		public bool IsWestOf(GeoPoint other)
		{
			bool result = false;
			if(this.LongitudeCD == CardinalDirection.West && other.LongitudeCD == CardinalDirection.West)
				result = this.Longitude < other.Longitude;
			else if(this.LongitudeCD == CardinalDirection.East && other.LongitudeCD == CardinalDirection.East)
				result = this.Longitude > other.Longitude;
			else
				result = this.LongitudeCD == CardinalDirection.West;
			return result;
		}

		public bool IsEastOfOrEqualTo(GeoPoint other, int precision = EarthGeo.GeoPrecision)
		{
			return IsEastOf(other) || Longitude.EqualsAtPrecision(other.Longitude, precision);
		}

		public bool IsEastOf(GeoPoint other)
		{
			return !IsWestOf(other);
		}

		#endregion

		#region Public Geometry Methods

		public void SetPrecision(int precision)
		{
			Longitude = Math.Round(Longitude, precision);
			Latitude = Math.Round(Latitude, precision);
		}

		public void Move(Double bearing, Double distance)
		{
			GeoPoint np = EarthGeo.GetPoint(this, bearing, distance);
			Latitude = np.Latitude;
			Longitude = np.Longitude;
		}

		public void Move(GeoPoint where)
		{
			m_Latitude = where.Y;
			m_Longitude = where.X;
		}

		public GeoPoint Round(Int32 places)
		{
			return new GeoPoint(Math.Round(m_Latitude, places), Math.Round(m_Longitude, places));
		}

		public GeoPoint GetPointAt(Double bearing, Double distance)
		{
			return EarthGeo.GetPoint(this, bearing, distance);
		}

		#endregion

		#region Utility

		public GeoPoint Clone()
		{
			return new GeoPoint(Latitude, Longitude);
		}

		public static GeoPoint Parse(String value)
		{
			GeoPoint point;
			if(!TryParse(value, out point))
			{
				throw new CommonException("Can't parse geopoint from given string value");
			}
			return point;
		}

		public static bool TryParse(String value, out GeoPoint point)
		{
			point = null;

			int		index = value.IndexOf(TOSTRING_DELIM);
			String	name = String.Empty;
			if(index > 0)
			{
				name = value.Substring(0, index).Trim();
				value = value.Substring(index).Trim();
			}

			if(value.Length > TOSTRING_DELIM.Length && value.Substring(0, TOSTRING_DELIM.Length).Equals(TOSTRING_DELIM))
			{
				/** parse from our own ToString */
				String[]	parts = value.Substring(TOSTRING_DELIM.Length).Split(',');
				Double		latitude, longitude;
				if(parts.Length == 2 && Double.TryParse(parts[0], out latitude) && Double.TryParse(parts[1], out longitude))
				{
					point = new GeoPoint(latitude, longitude);
					point.Name = name;
				}
			}
			else
			{
				/** parse from just lat/long */
				String[]	parts = value.Split(',');
				Double		latitude, longitude;
				if(parts.Length == 2 && Double.TryParse(parts[0], out latitude) && Double.TryParse(parts[1], out longitude))
				{
					point = new GeoPoint(latitude, longitude);
					point.Name = name;
				}
			}
			return point != null;
		}

		public static GeoPoint Empty { get { return new GeoPoint(); } }

		public string ToSQLString()
		{
			return "GeomFromText('POINT(" + X + " " + Y + ")')";
		}

		public Unescaped ToUnescapedSQLString()
		{
			return ToUnescapedSQLString(this);
		}

		public static Unescaped ToUnescapedSQLString(GeoPoint point)
		{
			Unescaped retVal = Unescaped.String("NULL");
			if (point != null)
			{
				retVal = Unescaped.String(point.ToSQLString());
			}
			return retVal;
		}

		public GeoPoint ToGeoPoint()
		{
			return new GeoPoint(this);
		}

		public Point ToPoint()
		{
			return new Point((int)this.X, (int)this.Y);
		}

		public PointD ToPointD()
		{
			return new PointD(this);
		}

		//public int CompareTo(object other)
		//{
		//	return PointCommon.ComparePoints(this, other, Precision);
		//}

		public bool Equals(GeoPoint other)
		{
			return Equals(other, 0);
		}

		public bool Equals(GeoPoint other, int precision)
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
			return base.GetHashCode();
		}

		public override bool Equals(object other)
		{
			return PointCommon.Equals(this, other);
		}

		public override string ToString()
		{
			return ToString(EarthGeo.GeoPrecision);
		}

		public string ToString(int precision)
		{
			String format = String.Format("{{0}}{{1:F{0}}}, {{2:F{1}}}", precision, precision);
			String ret = String.Format(format, TOSTRING_DELIM, Latitude, Longitude);
			return ret;
		}

		#endregion


	}
}
