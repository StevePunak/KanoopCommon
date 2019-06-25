using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Geometry
{
	public class EarthGeo
	{
		public const Double EarthRadius = 6372797.560856;
		public const int	GeoPrecision = 8;
		public const int	LengthPrecision = 3;		/** millimeter */

		public const Double North = 0;
		public const Double NorthEast = 45;
		public const Double East = 90;
		public const Double SouthEast = 135;
		public const Double South = 180;
		public const Double SouthWest = 225;
		public const Double West = 270;
		public const Double NorthWest = 315;

		static public double GetDistance(GeoPoint pt1, GeoPoint pt2)
		{
			// get the arc between the two earth points in radians
			double deltaLat = FlatGeo.Radians(pt2.Y - pt1.Y);
			double deltaLong = FlatGeo.Radians(pt2.X - pt1.X);

			double latitudeHelix = Math.Pow(Math.Sin(deltaLat * 0.5), 2);
			double longitudeHelix = Math.Pow(Math.Sin(deltaLong * 0.5), 2);

			double tmp = Math.Cos(FlatGeo.Radians(pt1.Y)) * Math.Cos(FlatGeo.Radians(pt2.Y));
			double rad = 2.0 * Math.Asin(Math.Sqrt(latitudeHelix + tmp * longitudeHelix));

			// Multiply the radians by the earth radius in meters for a distance measurement in meters
			return (EarthRadius * rad);
		}

		public static double GetBearing(GeoPoint pt1, GeoPoint pt2)
		{
			double lat1 = FlatGeo.Radians(pt1.Y);
			double lon1 = FlatGeo.Radians(pt1.X);
			double lat2 = FlatGeo.Radians(pt2.Y);
			double lon2 = FlatGeo.Radians(pt2.X);

			double y = Math.Sin(lon2 - lon1) * Math.Cos(lat2);
			double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1);
			double bearing = Math.Atan2(y, x); 	// bearing in radians

			double ret = (FlatGeo.Degrees(bearing) + 360) % 360;
			return ret;
		}

		public static Double Degrees(Double angle)
		{
			return angle * (180 / Math.PI);
		}

		public static Double Radians(Double angle)
		{
			return Math.PI * angle / 180;
		}

		[Obsolete("Use GetPoint - BEWARE Parameter Order Is Changed")]
		public static GeoPoint GetDestination(GeoPoint origin, Double distanceInMeters, Double bearing)
		{
			bearing = FlatGeo.Radians(bearing % 360);
			double ddr = distanceInMeters / EarthGeo.EarthRadius;

			double lat = FlatGeo.Radians(origin.Y);
			double lon = FlatGeo.Radians(origin.X);


			double lat2 = Math.Asin(Math.Sin(lat) * Math.Cos(ddr) + Math.Cos(lat) * Math.Sin(ddr) * Math.Cos(bearing));
			double lon2 = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(ddr) * Math.Cos(lat), Math.Cos(ddr) - Math.Sin(lat) * Math.Sin(lat2));
			lon2 = ((lon2 + Math.PI) % (2 * Math.PI)) - Math.PI;

			return new GeoPoint(FlatGeo.Degrees(lat2), FlatGeo.Degrees(lon2));
		}

		public static GeoPoint GetPoint(GeoPoint origin, Double bearing, Double distanceInMeters)
		{
			bearing = FlatGeo.Radians(bearing % 360);
			double ddr = distanceInMeters / EarthGeo.EarthRadius;

			double lat = FlatGeo.Radians(origin.Y);
			double lon = FlatGeo.Radians(origin.X);


			double lat2 = Math.Asin(Math.Sin(lat) * Math.Cos(ddr) + Math.Cos(lat) * Math.Sin(ddr) * Math.Cos(bearing));
			double lon2 = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(ddr) * Math.Cos(lat), Math.Cos(ddr) - Math.Sin(lat) * Math.Sin(lat2));
			lon2 = ((lon2 + Math.PI) % (2 * Math.PI)) - Math.PI;

			return new GeoPoint(FlatGeo.Degrees(lat2), FlatGeo.Degrees(lon2));
		}

		public static bool GetIntersection(GeoLine l1, GeoLine l2, out GeoPoint retPoint)
		{
			return GetIntersection(l1, l2, EarthGeo.GeoPrecision, out retPoint);
		}

		public static bool GetIntersection(GeoLine l1, GeoLine l2, int precision, out GeoPoint intersection)
		{
			intersection = new GeoPoint();
			bool result = false;

			double A1, B1, C1;
			FlatGeo.GetLineABC(l1.ToLine(), out A1, out B1, out C1);

			double A2, B2, C2;
			FlatGeo.GetLineABC(l2.ToLine(), out A2, out B2, out C2);

			double det = A1*B2 - A2*B1;
			if(det != 0)
			{
				intersection = new GeoPoint((A1*C2 - A2*C1) / det, (B2*C1 - B1*C2) / det);

bool result1 = intersection.IsWestOfOrEqualTo(EastMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision);
bool result2 = intersection.IsEastOfOrEqualTo(WestMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision);
bool result3 = intersection.IsWestOfOrEqualTo(EastMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision);
bool result4 = intersection.IsEastOfOrEqualTo(WestMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision);
bool result5 = intersection.IsSouthOfOrEqualTo(NorthMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision);
bool result6 = intersection.IsNorthOfOrEqualTo(SouthMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision);
bool result7 = intersection.IsSouthOfOrEqualTo(NorthMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision);
bool result8 = intersection.IsNorthOfOrEqualTo(SouthMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision);

				if(	intersection.IsWestOfOrEqualTo(EastMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision) && intersection.IsEastOfOrEqualTo(WestMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision) &&
					intersection.IsWestOfOrEqualTo(EastMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision) && intersection.IsEastOfOrEqualTo(WestMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision) &&
					intersection.IsSouthOfOrEqualTo(NorthMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision) && intersection.IsNorthOfOrEqualTo(SouthMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision) &&
					intersection.IsSouthOfOrEqualTo(NorthMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision) && intersection.IsNorthOfOrEqualTo(SouthMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision))
				{
					result = true;
				}
					
			}
			// else, its parallel
			return result;
		}

		public static bool GetIntersection2(GeoLine l1, GeoLine l2, int precision, out GeoPoint intersection) 
		{
			bool result = false;

			result = GetIntersection2(l1.P1 as GeoPoint, l1.Bearing, l2.P1 as GeoPoint, l2.Bearing, out intersection);

//bool result1 = intersection.IsWestOfOrEqualTo(EastMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision);
//bool result2 = intersection.IsEastOfOrEqualTo(WestMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision);
//bool result3 = intersection.IsWestOfOrEqualTo(EastMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision);
//bool result4 = intersection.IsEastOfOrEqualTo(WestMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision);
//bool result5 = intersection.IsSouthOfOrEqualTo(NorthMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision);
//bool result6 = intersection.IsNorthOfOrEqualTo(SouthMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision);
//bool result7 = intersection.IsSouthOfOrEqualTo(NorthMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision);
//bool result8 = intersection.IsNorthOfOrEqualTo(SouthMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision);

				if(	intersection.IsWestOfOrEqualTo(EastMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision) && intersection.IsEastOfOrEqualTo(WestMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision) &&
					intersection.IsWestOfOrEqualTo(EastMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision) && intersection.IsEastOfOrEqualTo(WestMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision) &&
					intersection.IsSouthOfOrEqualTo(NorthMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision) && intersection.IsNorthOfOrEqualTo(SouthMost(l1.P1 as GeoPoint, l1.P2 as GeoPoint), precision) &&
					intersection.IsSouthOfOrEqualTo(NorthMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision) && intersection.IsNorthOfOrEqualTo(SouthMost(l2.P1 as GeoPoint, l2.P2 as GeoPoint), precision))
				{
					result = true;
				}

			return result;
		}

		/**
		* Returns the point of intersection of two paths defined by point and bearing.
		*
		* @param   {LatLon} p1 - First point.
		* @param   {number} brng1 - Initial bearing from first point.
		* @param   {LatLon} p2 - Second point.
		* @param   {number} brng2 - Initial bearing from second point.
		* @returns {LatLon} Destination point (null if no unique intersection defined).
		*
		* @example
		*     var p1 = LatLon(51.8853, 0.2545), brng1 = 108.547;
		*     var p2 = LatLon(49.0034, 2.5735), brng2 =  32.435;
		*     var pInt = LatLon.intersection(p1, brng1, p2, brng2); // pInt.toString(): 50.9076°N, 004.5084°E
		*/
		public static bool GetIntersection2(GeoPoint p1, Double brng1, GeoPoint p2, Double brng2, out GeoPoint intersection) 
		{
			intersection = null;

			// see http://williams.best.vwh.net/avform.htm#Intersection
			Double φ1 = Radians(p1.Latitude);
			Double λ1 = Radians(p1.Longitude);
			Double φ2 = Radians(p2.Latitude);
			Double λ2 = Radians(p2.Longitude);
			Double θ13 = Radians(brng1);
			Double θ23 = Radians(brng2);
			Double Δφ = φ2-φ1, Δλ = λ2-λ1;

			Double δ12 = 2*Math.Asin(Math.Sqrt(Math.Sin(Δφ/2)*Math.Sin(Δφ/2) +
				Math.Cos(φ1)*Math.Cos(φ2)*Math.Sin(Δλ/2)*Math.Sin(Δλ/2) ) );
			if(δ12 != 0)	// if zero, no intersection
			{
				// initial/final bearings between points
				Double θ1 = Math.Acos( ( Math.Sin(φ2) - Math.Sin(φ1)*Math.Cos(δ12) ) /
									( Math.Sin(δ12)*Math.Cos(φ1) ) );
				if (Double.IsNaN(θ1))
				{
					θ1 = 0; // protect against rounding
				}

				Double θ2 = Math.Acos( ( Math.Sin(φ1) - Math.Sin(φ2)*Math.Cos(δ12) ) /
									( Math.Sin(δ12)*Math.Cos(φ2) ) );

				Double θ12, θ21;
				if (Math.Sin(λ2-λ1) > 0) 
				{
					θ12 = θ1;
					θ21 = 2*Math.PI - θ2;
				} 
				else 
				{
					θ12 = 2*Math.PI - θ1;
					θ21 = θ2;
				}

				Double α1 = (θ13 - θ12 + Math.PI) % (2*Math.PI) - Math.PI; // angle 2-1-3
				Double α2 = (θ21 - θ23 + Math.PI) % (2*Math.PI) - Math.PI; // angle 1-2-3

				if (Math.Sin(α1)==0 && Math.Sin(α2)==0)
				{
					// infinite intersections
				}
				else if (Math.Sin(α1)*Math.Sin(α2) < 0)
				{
					// ambiguous intersection
				}
				else
				{

					//α1 = Math.abs(α1);
					//α2 = Math.abs(α2);
					// ... Ed Williams takes abs of α1/α2, but seems to break calculation?

					Double α3 = Math.Acos( -Math.Cos(α1)*Math.Cos(α2) +
										 Math.Sin(α1)*Math.Sin(α2)*Math.Cos(δ12) );
					Double δ13 = Math.Atan2( Math.Sin(δ12)*Math.Sin(α1)*Math.Sin(α2),
										  Math.Cos(α2)+Math.Cos(α1)*Math.Cos(α3) );
					Double φ3 = Math.Asin( Math.Sin(φ1)*Math.Cos(δ13) +
										Math.Cos(φ1)*Math.Sin(δ13)*Math.Cos(θ13) );
					Double Δλ13 = Math.Atan2( Math.Sin(θ13)*Math.Sin(δ13)*Math.Cos(φ1),
										   Math.Cos(δ13)-Math.Sin(φ1)*Math.Sin(φ3) );
					Double λ3 = λ1 + Δλ13;
					λ3 = (λ3+3*Math.PI) % (2*Math.PI) - Math.PI; // normalise to -180..+180º
					intersection = new GeoPoint(Degrees(φ3), Degrees(λ3));
				}
			
			}
			return intersection != null;
		}

		public static GeoPoint NorthMost(GeoPoint p1, GeoPoint p2)
		{
			return p1.IsNorthOf(p2) ? p1 : p2;
		}

		public static GeoPoint NorthMost(GeoPointList list)
		{
			GeoPoint ret = null;
			foreach(GeoPoint point in list)
			{
				if(ret == null || point.IsNorthOf(ret))
				{
					ret = point;
				}
			}

			return ret;
		}

		public static GeoPointList NorthToSouth(GeoPointList list)
		{
			GeoPointList fromList = new GeoPointList(list);
			GeoPointList retList = new GeoPointList();
			
			while(fromList.Count > 0)
			{
				retList.Add(NorthMost(fromList));
				fromList.Remove(retList.Last());
			}
			return retList;
		}

		public static GeoPoint SouthMost(GeoPoint p1, GeoPoint p2)
		{
			return p1.IsSouthOf(p2) ? p1 : p2;
		}

		public static GeoPoint SouthMost(GeoPointList list)
		{
			GeoPoint ret = null;
			foreach(GeoPoint point in list)
			{
				if(ret == null || point.IsSouthOf(ret))
				{
					ret = point;
				}
			}

			return ret;
		}

		public static GeoPointList SouthToNorth(GeoPointList list)
		{
			GeoPointList fromList = new GeoPointList(list);
			GeoPointList retList = new GeoPointList();
			
			while(fromList.Count > 0)
			{
				retList.Add(SouthMost(fromList));
				fromList.Remove(retList.Last());
			}
			return retList;
		}

		public static GeoPoint EastMost(GeoPoint p1, GeoPoint p2)
		{
			return p1.IsEastOf(p2) ? p1 : p2;
		}

		public static GeoPoint EastMost(GeoPointList list)
		{
			GeoPoint ret = null;
			foreach(GeoPoint point in list)
			{
				if(ret == null || point.IsEastOf(ret))
				{
					ret = point;
				}
			}

			return ret;
		}

		public static GeoPointList EastToWest(GeoPointList list)
		{
			GeoPointList fromList = new GeoPointList(list);
			GeoPointList retList = new GeoPointList();
			
			while(fromList.Count > 0)
			{
				retList.Add(EastMost(fromList));
				fromList.Remove(retList.Last());
			}
			return retList;
		}

		public static GeoPoint WestMost(GeoPoint p1, GeoPoint p2)
		{
			return p1.IsWestOf(p2) ? p1 : p2;
		}

		public static GeoPoint WestMost(GeoPointList list)
		{
			GeoPoint ret = null;
			foreach(GeoPoint point in list)
			{
				if(ret == null || point.IsWestOf(ret))
				{
					ret = point;
				}
			}

			return ret;
		}

		public static GeoPointList WestToEast(GeoPointList list)
		{
			GeoPointList fromList = new GeoPointList(list);
			GeoPointList retList = new GeoPointList();
			
			while(fromList.Count > 0)
			{
				retList.Add(WestMost(fromList));
				fromList.Remove(retList.Last());
			}
			return retList;
		}

		public static Double DegreesPerMeterAtLatitude(Double latitude)
		{
			GeoPoint origin = new GeoPoint(latitude, 0);
			GeoPoint destination = GetPoint(origin, EarthGeo.East, 1);
			return Math.Abs(destination.Longitude - origin.Longitude);
		}

		public static Double DegreesPerMeterAtLongitude(Double longitude)
		{
			GeoPoint origin = new GeoPoint(0, longitude);
			GeoPoint destination = GetPoint(origin, EarthGeo.North, 1);
			return Math.Abs(destination.Latitude - origin.Latitude);
		}

	}
}
