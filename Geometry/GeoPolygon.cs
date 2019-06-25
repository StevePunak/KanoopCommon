using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using KanoopCommon.Logging;
using KanoopCommon.Database;
using System.ComponentModel;
using KanoopCommon.Conversions;

namespace KanoopCommon.Geometry
{
	public class GeoPolygonList : List<GeoPolygon> {}

	public class GeoPolygon : GeoShape
	{
		#region Constants

		const int DEFAULT_RAY_LENGTH = 10000;

		#endregion

		#region Public Properties

		protected GeoPointList _points;
		public GeoPointList GeoPoints { get { return _points; } }

		public List<GeoPoint> Points { get { return new List<GeoPoint>(_points); } }
	
		public List<GeoLine> Lines { get { return new List<GeoLine>(GeoLines); } }

		public GeoLineList GeoLines
		{ 
			get 
			{
				GeoLineList lines = new GeoLineList();
				if(GeoPoints.Count > 0)
				{
					GeoPoint previous = null;
					foreach(GeoPoint point in GeoPoints)
					{
						if(previous != null)
						{
							lines.Add(new GeoLine(previous, point));
						}
						previous = point;
					}
					lines.Add(new GeoLine(previous, GeoPoints[0] as GeoPoint));
				}
				return lines; 
			} 
		}

		public GeoPoint NorthMost
		{
			get
			{
				GeoPoint retpoint = Points[0] as GeoPoint;
				foreach(GeoPoint point in Points)
				{
					if(point.IsNorthOf(retpoint))
						retpoint = point;
				}
				return retpoint;
			}
		}

		public GeoPoint SouthMost
		{
			get
			{
				GeoPoint retpoint = Points[0] as GeoPoint;
				foreach(GeoPoint point in Points)
				{
					if(point.IsSouthOf(retpoint))
						retpoint = point;
				}
				return retpoint;
			}
		}

		public GeoPoint WestMost
		{
			get
			{
				GeoPoint retpoint = Points[0] as GeoPoint;
				foreach(GeoPoint point in Points)
				{
					if(point.IsWestOf(retpoint))
						retpoint = point;
				}
				return retpoint;
			}
		}

		public GeoPoint EastMost
		{
			get
			{
				GeoPoint retpoint = Points[0] as GeoPoint;
				foreach(GeoPoint point in Points)
				{
					if(point.IsEastOf(retpoint))
						retpoint = point;
				}
				return retpoint;
			}
		}

		public GeoPoint RandomPoint
		{
			/** code this later */
			get { return Centroid; }
		}

		public GeoPoint Centroid 
		{ 
			get 
			{
				/** SPSPTODO - Get this into EarthGeo */

				/** map onto flat space */
				CoordinateMap map;
				Polygon polygon;
				GetMappedPolygon(out polygon, out map);

				/** get offset for a positive graph */
				Double offset = polygon.MinimumXY;

				/** move our polygon into positive territory and get centroid */
				polygon.Move(EarthGeo.SouthEast, offset);
				PointD centroid = polygon.Centroid;

				/** now re-offset it and return mapped result */
				centroid.Move(EarthGeo.NorthWest, offset);
				return map.GetGeoPoint(centroid);
			} 
		}

		public GeoPoint GeoCenter { get { return Centroid; } }		// for interface

		public Double Area
		{
			get
			{
				Double latAnchor = GeoPoints[0].Latitude;
				Double lonAnchor = GeoPoints[0].Longitude;

				/** convert to cartesian coordinates */
				PointDList cartesianPoints = new PointDList();
				foreach(GeoPoint point in GeoPoints)
				{
					Double cartX = ( (point.Longitude - lonAnchor) * ( EarthGeo.EarthRadius * Math.PI / 180 ) ) * Math.Cos(latAnchor * Math.PI / 180 );
					Double cartY = (point.Latitude - latAnchor) * FlatGeo.Radians(EarthGeo.EarthRadius);

					cartesianPoints.Add(new PointD(cartX, cartY));
				}

				Polygon polygon = new Polygon(cartesianPoints);
				return polygon.Area;
			}
		}

		public Double PerimiterLength
		{
			get
			{
				Double length = 0;
				Lines.ForEach(line => length += line.Length);
				return length;
			}
		}

		#endregion

		#region Constructors

		public GeoPolygon(GeoPolygon other)
			: this(new GeoPointList(other.GeoPoints)) {}

		public GeoPolygon(PointDList points)
			: this(new GeoPointList(points)) {}

		public GeoPolygon(GeoPointList points)
		{
			_points = points;
		}

		#endregion

		#region Public Geometery Methods

		public bool ContainsAnyPoint(GeoPointList points, Double rayLength = DEFAULT_RAY_LENGTH)
		{
			bool result = false;
			foreach(GeoPoint point in points)
			{
				if(Contains(point, rayLength))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool Contains(GeoPoint point)
		{
			return Contains(point, DEFAULT_RAY_LENGTH);
		}

		public bool Contains(GeoPoint point, Double rayLength = DEFAULT_RAY_LENGTH)
		{
			/** Use Ray-Casting Algorithm to determine if point lies within polygon */

			Double rayBearing = EarthGeo.North;
			float insides = 0;
			float outsides = 0;

			int minimumRays = 4;

			bool majority = false;

			/**
			 * We will cast at least two rays and keep casting till we get a majority of 
			 * one or the other, or we go in a circle.
			 */
			do
			{
				/** 
				 * Step One: 
				 *   Create a ray from our point extending outwards
				 */
				GeoPoint endPoint = EarthGeo.GetPoint(point, rayBearing, rayLength);
				GeoLine ray = new GeoLine(point, endPoint);

				/**
				 * Step 2
				 *   Draw line from our point in any direction (we will use North)
				 *   and count the intersections
				 */
				Double intersections = 0;
				foreach(GeoLine line in Lines)
				{
					if(ray.Intersects(line))
						intersections++;
				}

				/** if the intersections are even, the point is outside... if odd, it's inside */
				bool inside = (intersections % 2) != 0;

				if(inside)
					insides++;
				else
					outsides++;

				rayBearing = Angle.Add(rayBearing, 95);

				if(insides + outsides >= minimumRays)
				{
					majority = insides > outsides * 2 || outsides > insides * 2;
				}
				
			}while(majority == false && !(insides + outsides >= 360 / 5) );

			return insides > outsides;
		}

		public bool Contains(GeoCircle circle, Double rayLength = DEFAULT_RAY_LENGTH)
		{
			/** see if this polygon entirely contains the given circle */
			return this.Contains(circle.Center as GeoPoint) == true && circle.Intersects(this) == false;
		}

		public override void Move(double bearing, double distance)
		{
			foreach(GeoPoint point in _points)
			{
				point.Move(bearing, distance);
			}
		}
		
		public GeoPoint NearestPointOnEdgeFrom(GeoPoint point)
		{
			Double closest = Double.MaxValue;
			GeoPoint closetPoint = new GeoPoint();
			foreach(GeoLine line in this.Lines)
			{
				Double distance;
				GeoPoint p = line.ClosestPointTo(point, out distance);
				if(distance < closest)
				{
					closest = distance;
					closetPoint = p;
				}
			}
			return closetPoint;
		}

		public GeoLine NearestEdgeFrom(GeoPoint point)
		{
			Double closest = Double.MaxValue;
			GeoLine closetLine = new GeoLine();
			foreach(GeoLine line in this.Lines)
			{
				Double distance;
				GeoPoint p = line.ClosestPointTo(point, out distance);
				if(distance < closest)
				{
					closest = distance;
					closetLine = line;
				}
			}
			return closetLine;
		}

		public Double DistanceToEdgeFrom(GeoPoint point)
		{
			Double closest = Double.MaxValue;
			foreach(GeoLine line in this.Lines)
			{
				Double distance = line.ClosestDistanceFrom(point);
				if(distance < closest)
				{
					closest = distance;
				}
			}
			return closest;
		}

		public GeoRectangle GetBoundingRectangle()
		{
			return new GeoRectangle(	new GeoPointList()
										{
											new GeoPoint(NorthMost.Latitude, WestMost.Longitude),
											new GeoPoint(NorthMost.Latitude, EastMost.Longitude),
											new GeoPoint(SouthMost.Latitude, EastMost.Longitude),
											new GeoPoint(SouthMost.Latitude, WestMost.Longitude)
										});
		}

		public Double GetMiniumumPossibleDistance(GeoCircle postion)
		{
			Double distance = Double.MaxValue;

			if(postion.Intersects(this))
			{
				distance = 0;
			}
			else
			{
				distance = DistanceToEdgeFrom(postion.Center as GeoPoint);
				distance = Math.Max(0, distance - postion.Radius);
			}

			return distance;
		}

		public void GetMappedPolygon(out Polygon polygon, out CoordinateMap coordinateMap)
		{
			polygon = new Polygon();
			coordinateMap = new CoordinateMap(Points[0] as GeoPoint, 0, 100);
			foreach(GeoPoint point in Points)
			{
				polygon.AddVertice(coordinateMap.GetPixelPoint(point));
				//polygon.Points.Add(coordinateMap.GetPixelPoint(point));
			}
		}

		public void SetPrecision(int precision)
		{
			foreach(GeoPoint point in Points)
			{
				point.SetPrecision(precision);
			}
		}

		public static LineList GeoPointsToLineList(GeoPointList points)
		{
			LineList lines = new LineList();

			GeoPoint lastPoint = null;
			foreach(GeoPoint point in points)
			{
				if(lastPoint != null)
				{
					lines.Add(new Line(lastPoint, point));
				}
				lastPoint = point;
			}

			return lines;
		}

		public static bool ContainedByAnyPolygon(GeoPoint point, IEnumerable<GeoPolygon> polygons)
		{
			bool result = false;
			foreach(GeoPolygon polygon in polygons)
			{
				if(polygon.Contains(point))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public static Double GetMiniumumPossibleDistanceFromAnyPolygon(GeoCircle postion, IEnumerable<GeoPolygon> polygons)
		{
			Double distance = Double.MaxValue;

			foreach(GeoPolygon polygon in polygons)
			{
				if(postion.Intersects(polygon) || polygon.Contains(postion.Center as GeoPoint))
				{
					distance = 0;
				}
				else
				{
					Double thisDistance = polygon.DistanceToEdgeFrom(postion.Center as GeoPoint);
					thisDistance = Math.Max(0, thisDistance - postion.Radius);
					distance = Math.Min(distance, thisDistance);
				}

				if(distance == 0)
					break;
			}
			return distance;
		}

		public GeoRectangle GetMinimumBoundingRectangle()
		{
			GeoPoint p1 = new GeoPoint(NorthMost.Latitude, WestMost.Longitude);
			GeoPoint p2 = new GeoPoint(NorthMost.Latitude, EastMost.Longitude);
			GeoPoint p3 = new GeoPoint(SouthMost.Latitude, EastMost.Longitude);
			GeoPoint p4 = new GeoPoint(SouthMost.Latitude, WestMost.Longitude);
			return new GeoRectangle(p1, p2, p3, p4);
		}

		#endregion

		#region Private Helper Methods

		#endregion

		#region Utility

		public PointDList ToPointDList()
		{
			PointDList list = new PointDList();
			foreach(GeoPoint point in GeoPoints)
			{
				list.Add(point.ToPointD());
			}
			return list;
		}

		public bool Equals(GeoPolygon other)
		{
			bool result = false;
			if(Lines.Count == other.Lines.Count)
			{
				result = true;
				for(int x = 0;x < Lines.Count;x++)
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

		public override object Clone()
		{
			return new GeoPolygon(GeoPoints);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach(GeoPoint point in Points)
			{
				sb.AppendFormat("({0}), ", point);
			}
			return sb.ToString().TrimEnd(new char[] {',', ' '});
		}

		public Unescaped ToUnescapedSQLString()
		{
			return this.GeoPoints.ToPointDList().ToSQLString();
		}

		#endregion

	}
}
