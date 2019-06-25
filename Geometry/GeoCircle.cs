using KanoopCommon.CommonObjects;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System;

namespace KanoopCommon.Geometry
{
	public class GeoCircle : GeoEllipse
	{
		#region Public Properties

		Double		m_Radius;
		public Double Radius
		{
			get { return m_Radius; }
			set { m_Radius = value; }
		}

		public Double Diameter { get { return Radius * 2; } }

		public override Double Area { get { return Math.PI * (m_Radius * m_Radius); } }

		public Double Latitude
		{
			get { return _center.Latitude; }
			set { _center.Latitude = value; }
		}

		public Double Longitude
		{
			get { return _center.Longitude; }
			set { _center.Longitude = value; }
		}

		public GeoPoint EarthPoint
		{
			get { return _center; }
			set { _center = value; }
		}

		#endregion

		#region Constructors

		public GeoCircle()
			: this(new GeoPoint(), 0) {}

		public GeoCircle(Double latitude, Double longitude, Double radius)
			: this(new GeoPoint(latitude, longitude), radius) {}

		public GeoCircle(GeoCircle other)
			: this(other.Latitude, other.Longitude, other.Radius) {}

		public GeoCircle(PointD center, Double radius)
			: this(new GeoPoint(center), radius) {}

		public GeoCircle(GeoSquare square)
			: this(square.Center, square.GeoLines[0].Length / 2) {}

		public GeoCircle(GeoPoint center, Double radius)
			: base(center, radius)
		{
			_center = center;
			m_Radius = radius;
		}

		#endregion

		#region Conversions

		public GeoPolygon ToGeoPolygon(int sides = 16)
		{
			GeoPointList	points = new GeoPointList();
			Double			angleSize = (360 / (Double)sides);

			for (Double degrees = 0; degrees < 360; degrees += angleSize)
			{
				points.Add(EarthGeo.GetPoint(_center, Radius, degrees));
			}

			GeoPolygon		polygon = new GeoPolygon(points);
			return polygon;
		}

		#endregion

		#region Public Geometry Methods

		public override bool Contains(GeoPoint point)
		{
			Double distance = EarthGeo.GetDistance(_center, point);
			return distance <= Radius;
		}

		public override void Move(GeoPoint center)
		{
			_center = new GeoPoint(center);
		}

		public override void Move(Double bearing, Double distance)
		{
			GeoPoint center = new GeoPoint(Center);
			center.Move(bearing, distance);
			_center = center;
		}

		public override bool Intersects(GeoPolygon polygon)
		{
			bool result = false;

			foreach(GeoLine line in polygon.Lines)
			{
				if(	this.Contains(line.P1 as GeoPoint) ||
					this.Contains(line.P2 as GeoPoint) ||
					this.Intersects(line))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public override bool Intersects(GeoLine line)
		{
			GeoPoint i1, i2;
			int intersections;
			return Intersects(line, out i1, out i2, out intersections);
		}

		public override bool Intersects(GeoLine line, out GeoPoint intersection1, out GeoPoint intersection2, out int intersections)
		{
			intersection1 = new GeoPoint(float.NaN, float.NaN);
			intersection2 = new GeoPoint(float.NaN, float.NaN);

			const Double	increment = .1;

			bool			intersects = false;
			GeoPoint		p = line.P1.Clone() as GeoPoint;

			do
			{
				if(this.Contains(p))
				{
					intersects = true;
					break;
				}

				p = EarthGeo.GetPoint(p, line.Bearing, increment);
			} while(EarthGeo.GetDistance(line.P1 as GeoPoint, p) < line.Length);

			intersections = 0;
			if(intersects)
			{
				Double	degy = EarthGeo.DegreesPerMeterAtLatitude(_center.Latitude);
				Double	degx = EarthGeo.DegreesPerMeterAtLongitude(_center.Longitude);
				Double	radius = Radius * Math.Max(degx, degy);

				Double	dx, dy, A, B, C, det, t;

				dx = line.P2.X - line.P1.X;
				dy = line.P2.Y - line.P1.Y;

				A = dx * dx + dy * dy;
				B = 2 * (dx * (line.P1.X - Center.X) + dy * (line.P1.Y - Center.Y));
				C = (line.P1.X - Center.X) * (line.P1.X - Center.X) + (line.P1.Y - Center.Y) * (line.P1.Y - Center.Y) - radius * radius;

				det = B * B - 4 * A * C;

				if ((A <= 0.00000001) || (det < 0))
				{
					// No real solutions.
					intersection1 = new GeoPoint(float.NaN, float.NaN);
					intersection2 = new GeoPoint(float.NaN, float.NaN);
				}
				else if (det == 0)
				{
					// One solution.
					t = -B / (2 * A);
					intersection1 = new GeoPoint(line.P1.X + t * dx, line.P1.Y + t * dy);
					intersection2 = new GeoPoint(float.NaN, float.NaN);
					intersections = 1;
				}
				else
				{
					// Two solutions.
					t = (float)((-B + Math.Sqrt(det)) / (2 * A));
					intersection1 = new GeoPoint(line.P1.X + t * dx, line.P1.Y + t * dy);
					t = (float)((-B - Math.Sqrt(det)) / (2 * A));
					intersection2 = new GeoPoint(line.P1.X + t * dx, line.P1.Y + t * dy);
					intersections = 2;
				}
			}
			return intersects;
		}

		public override void SetPrecision(int precision)
		{
			_center.Latitude = Math.Round(_center.Latitude, precision);
			_center.Longitude = Math.Round(_center.Longitude, precision);
		}

		public override GeoRectangle GetMinimumBoundingRectangle()
		{
			GeoPoint north = _center.GetPointAt(EarthGeo.North, Radius) as GeoPoint;
			GeoPoint south = _center.GetPointAt(EarthGeo.South, Radius) as GeoPoint;
			return new GeoRectangle(new GeoPointList()
			{
				north.GetPointAt(EarthGeo.West, Radius) as GeoPoint,
				north.GetPointAt(EarthGeo.East, Radius) as GeoPoint,
				south.GetPointAt(EarthGeo.East, Radius) as GeoPoint,
				south.GetPointAt(EarthGeo.West, Radius) as GeoPoint
			});
		}

		#endregion

		#region Utility

		public static GeoCircle CreateFromRegionPolygon(GeoPolygon polygon)
		{
			GeoPoint		centroid = polygon.Centroid;
			List<Double>	distances = new List<Double>();

			foreach(GeoPoint point in polygon.Points)
			{
				distances.Add(EarthGeo.GetDistance(centroid, point));
			}

			return new GeoCircle(centroid, distances.Average());
		}

		public static bool TryParse(String str, out GeoCircle circle)
		{
			circle = null;

			String[]	parts = str.Split('R');
			GeoPoint	p;
			Double		r;
			if(parts.Length == 2 && GeoPoint.TryParse(parts[0].Trim(), out p) && Double.TryParse(parts[1].Trim(), out r))
			{
				circle = new GeoCircle(p, r);
			}

			return circle != null;
		}

		public override object Clone()
		{
			return new GeoCircle(Center, Radius);
		}

		public override string ToString()
		{
			return String.Format("{0} R {1}", Center, Radius);
		}

		#endregion
	}
}
