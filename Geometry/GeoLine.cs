using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Logging;

namespace KanoopCommon.Geometry
{
	public class GeoLineListList : List<GeoLineList>
	{
		#region Constructors

		public GeoLineListList()
			: base() {}

		public GeoLineListList(IEnumerable<GeoLineList> other)
			: base(other) {}

		#endregion


		#region Public Properties

		public int TotalLines
		{
			get
			{
				int count = this.Sum(l => l.Count);
				return count;
			}
		}

		#endregion

		#region Public Access Methods

		public void RemoveEmptyLists()
		{
			this.RemoveAll(l => l.Count == 0);
		}

		#endregion
	}

	public class GeoLineList : List<GeoLine>
	{
		#region Constructors

		public GeoLineList()
			: base() {}

		public GeoLineList(IEnumerable<GeoLine> other)
		{
			foreach(GeoLine line in other)
			{
				Add(new GeoLine(line.P1, line.P2));
			}
		}

		public GeoLineList(GeoLineList other)
			: base(other) {}

		public GeoLineList(GeoPointList points)
			: base(points.ToGeoLineList()) {}

		public GeoLineList(PointDList list)
			: this(new GeoPointList(list)) {}

		#endregion

		#region Public Methods

		public bool ContainsEndPoint(GeoPoint point, int precision = 0)
		{
			bool result = false;
			foreach(GeoLine line in this)
			{
				if(line.IsEndPoint(point, precision))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool ContainsAnyEndPoint(GeoLineList other, int precision = 0)
		{
			bool result = false;
			foreach(GeoLine l1 in this)
			{
				foreach(GeoLine l2 in other)
				{
					if(l1.IsEndPoint(l2.P1, precision) || l1.IsEndPoint(l2.P2, precision))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		public GeoRectangle GetBoundingRectangle()
		{
			GeoPoint north = null;
			GeoPoint south = null;
			GeoPoint east = null;
			GeoPoint west = null;

			foreach(GeoLine line in this)
			{
				if(north == null || line.NorthMost.IsNorthOf(north))
					north = line.NorthMost;
				if(south == null || line.SouthMost.IsSouthOf(south))
					south = line.SouthMost;
				if(east == null || line.EastMost.IsEastOf(east))
					east = line.EastMost;
				if(west == null || line.WestMost.IsWestOf(west))
					west = line.WestMost;
			}

			return new GeoRectangle(	new GeoPointList()
										{
											new GeoPoint(north.Latitude, west.Longitude),
											new GeoPoint(north.Latitude, east.Longitude),
											new GeoPoint(south.Latitude, east.Longitude),
											new GeoPoint(south.Latitude, west.Longitude)
										});
		}

		public GeoPoint ClosestPointTo(GeoPoint point, out GeoLine closestLine, out Double distance)
		{
			Double shortestDistance = Double.MaxValue;
			closestLine = null;
			distance = 0;
			GeoPoint closestPoint = null;

			foreach(GeoLine line in this)
			{
				GeoPoint pointOnLine = line.ClosestPointTo(point, out distance);
				if(distance < shortestDistance)
				{
					shortestDistance = distance;
					closestLine = line;
					closestPoint = pointOnLine;
				}
			}

			distance = shortestDistance;
			return closestPoint;
		}

		public bool Intersects(GeoCircle circle)
		{
			bool intersects = false;
			foreach(GeoLine line in this)
			{
				GeoPoint p1, p2;
				int intersections;
				if(circle.Intersects(line, out p1, out p2, out intersections))
				{
					intersects = true;
					break;
				}
			}
			return intersects;
		}

		public void Move(Double bearing, Double distance)
		{
			foreach(GeoLine line in this)
			{
				line.Move(bearing, distance);
			}
		}

		public bool ContainsLine(GeoLine other)
		{
			GeoLine found = Find(l => l.Equals(other));
			return found != null;
		}

		#endregion

		#region Comparer Classes

		public class ListCountComparer : IComparer<GeoLineList>
		{
			public int Compare(GeoLineList x, GeoLineList y)
			{
				return x.Count.CompareTo(y.Count);
			}
		}

		#endregion

		#region Logging

		public void DumpToLog()
		{
			foreach(GeoLine line in this)
			{
				Log.SysLogText(LogLevel.DEBUG, "{0}", line);
			}
		}

		public void DumpToKML(string location, string title)
		{
			string path = @"c:\tmp\" + title + ".kml";

			if(System.IO.File.Exists(path))
			{
				System.IO.File.Delete(path);
			}

			#region Header
			System.IO.File.AppendAllText(path, "<?xml version='1.0' encoding='UTF-8'?>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<kml xmlns='http://www.opengis.net/kml/2.2' xmlns:gx='http://www.google.com/kml/ext/2.2' xmlns:kml='http://www.opengis.net/kml/2.2' xmlns:atom='http://www.w3.org/2005/Atom'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Document>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, string.Format("<name>{0}</name>", title) + Environment.NewLine);
			#region Color Setup
			#region StyleMaps
			System.IO.File.AppendAllText(path, "<StyleMap id='failed0'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>normal</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed01</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>highlight</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed12</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</StyleMap>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<StyleMap id='failed1'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>normal</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed05</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>highlight</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed15</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</StyleMap>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<StyleMap id='failed2'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>normal</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed06</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>highlight</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed13</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</StyleMap>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<StyleMap id='failed3'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>normal</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed02</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>highlight</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed11</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</StyleMap>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<StyleMap id='failed4'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>normal</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed04</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>highlight</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed10</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</StyleMap>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<StyleMap id='failed5'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>normal</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed00</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>highlight</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed14</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</StyleMap>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<StyleMap id='failed6'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>normal</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed03</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<key>highlight</key>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<styleUrl>#failed16</styleUrl>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Pair>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</StyleMap>" + Environment.NewLine);
			#endregion
			#region Styles
			System.IO.File.AppendAllText(path, "<Style id='failed00'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ff00ffff</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed01'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ff00ff00</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed02'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ffff00ff</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed03'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ff000000</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed04'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ff0000ff</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed05'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ffffff00</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed06'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ffff0000</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed10'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ff0000ff</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed12'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ff00ff00</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed11'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ffff00ff</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed13'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ffff0000</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed14'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ff00ffff</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed15'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ffffff00</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style id='failed16'>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<color>ff000000</color>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<width>2</width>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</LineStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
			#endregion
			#endregion
			System.IO.File.AppendAllText(path, "<Folder>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, string.Format("<name>{0}</name>", location) + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<open>1</open>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<Style>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<ListStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<listItemType>check</listItemType>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<ItemIcon>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<state>open</state>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<href>:/mysavedplaces_open.png</href>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</ItemIcon>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<ItemIcon>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<state>closed</state>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<href>:/mysavedplaces_closed.png</href>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</ItemIcon>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<bgColor>00ffffff</bgColor>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "<maxSnippetLines>2</maxSnippetLines>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</ListStyle>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Style>" + Environment.NewLine);
		#endregion

			int index = 0;
			Random rnd = new Random();
			foreach (GeoLine line in this)
			{
				index++;

				System.IO.File.AppendAllText(path, "<Placemark>" + Environment.NewLine);
				if (line.Name == "")
					System.IO.File.AppendAllText(path, string.Format("<name>Inital Line {0}</name>", index) + Environment.NewLine);
				else
					System.IO.File.AppendAllText(path, string.Format("<name>{0}</name>", line.Name) + Environment.NewLine);

				System.IO.File.AppendAllText(path, string.Format("<styleUrl>#failed{0}</styleUrl>", rnd.Next(0, 6)) + Environment.NewLine);
				System.IO.File.AppendAllText(path, "<LineString>" + Environment.NewLine);
				System.IO.File.AppendAllText(path, "<tessellate>1</tessellate>" + Environment.NewLine);
				System.IO.File.AppendAllText(path, "<coordinates>" + Environment.NewLine);
				System.IO.File.AppendAllText(path, string.Format("{0},{1},0 {2},{3},0", line.P1.X.ToString(), line.P1.Y.ToString(), line.P2.X.ToString(), line.P2.Y.ToString()) + Environment.NewLine);
				System.IO.File.AppendAllText(path, "</coordinates>" + Environment.NewLine);
				System.IO.File.AppendAllText(path, "</LineString>" + Environment.NewLine);
				System.IO.File.AppendAllText(path, "</Placemark>" + Environment.NewLine);
			}

			#region Footer
			System.IO.File.AppendAllText(path, "</Folder>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</Document>" + Environment.NewLine);
			System.IO.File.AppendAllText(path, "</kml>" + Environment.NewLine);
			#endregion
		}

		#endregion
	}

	public class GeoLine
	{
		#region Public Properties

		protected GeoPoint	_P1;
		public GeoPoint P1
		{
			get { return _P1; }
			set { _P1 = value as GeoPoint; }
		}

		protected GeoPoint	_P2;
		public GeoPoint P2
		{
			get { return _P2; }
			set { _P2 = value as GeoPoint; }		/** SP Remove setter if possible */
		}

		public GeoPoint NorthMost { get { return _P1.IsNorthOf(_P2) ? _P1 : _P2 ; } }

		public GeoPoint SouthMost { get { return _P1.IsSouthOf(_P2) ? _P1 : _P2 ; } }

		public GeoPoint WestMost { get { return _P1.IsWestOf(_P2) ? _P1 : _P2 ; } }

		public GeoPoint EastMost { get { return _P1.IsEastOf(_P2) ? _P1 : _P2 ; } }

		public Double Bearing { get { return EarthGeo.GetBearing(_P1, _P2); } }

		public Double Length { get { return EarthGeo.GetDistance(_P1, _P2); } }

		public GeoPoint MidPoint { get { return EarthGeo.GetPoint(_P1, Bearing, Length / 2); } }

		public String Name { get; set; }

		#endregion

		#region Constructors

		public GeoLine()
			: this(new GeoPoint(), new GeoPoint()) {}

		public GeoLine(Line line, CoordinateMap map)
			: this(map.GetGeoPoint(line.P1 as PointD), map.GetGeoPoint(line.P2 as PointD)) {}

		public GeoLine(GeoPoint p1, GeoPoint p2)
		{
			_P1 = p1;
			_P2 = p2;
			Name = String.Empty;
		}

		#endregion

		#region Public Geometry Methods

		public bool IsEndPoint(GeoPoint point, int precision = 0)
		{
			return _P1.Equals(point, precision) || _P2.Equals(point, precision);
		}

		public bool SharesEndPointWith(GeoLine other, int precision = 0)
		{
			return
				other.P1.Equals(P1, precision) ||
				other.P1.Equals(P2, precision) ||
				other.P2.Equals(P1, precision) ||
				other.P2.Equals(P2, precision);
		}

		public bool Intersects(GeoLine other)
		{
			GeoPoint intersection;
			return Intersects(other, out intersection);
		}

		public bool Intersects(GeoLine other, out GeoPoint intersection)
		{
			return Intersects(other, EarthGeo.GeoPrecision, out intersection);
		}

		public bool Intersects(GeoLine other, int precision, out GeoPoint intersection)
		{
			return EarthGeo.GetIntersection(this, other, precision, out intersection);
		}

		public bool Intersects(GeoPolygon polygon, out GeoPoint closestIntersection)
		{
			closestIntersection = null;
			Double closestDistance = Double.MaxValue;
			foreach(GeoLine line in polygon.Lines)
			{
				GeoPoint intersection;
				if(this.Intersects(line, out intersection))
				{
					Double distance = ClosestDistanceFrom(intersection);
					if(distance < closestDistance)
					{
						closestDistance = distance;
						closestIntersection = intersection;
					}
				}
			}
			return closestIntersection != null;
		}

		public bool Intersects(GeoCircle circle)
		{
			return circle.Intersects(this);
		}

		public void Move(Double bearing, Double distance)
		{
			_P1.Move(bearing, distance);
			_P2.Move(bearing, distance);
		}

		public void SetPrecision(int precision)
		{
			_P1.SetPrecision(precision);
			_P2.SetPrecision(precision);
		}

		public Double ClosestDistanceFrom(GeoPoint from)
		{
			GeoPoint point = new GeoPoint(ClosestPointTo(from));
			Double distance = EarthGeo.GetDistance(point, from);
			return distance;
		}

		public GeoPoint ClosestPointTo(GeoPoint pt)
		{
			Double distance;
			return ClosestPointTo(pt, out distance);
		}

		public GeoPoint ClosestPointTo(GeoPoint pt, out Double distance)
		{
			/** SPSPTODO: This needs to use spherical trig */

			PointD closest;
			Double dx = P2.X - P1.X;
			Double dy = P2.Y - P1.Y;

			// Calculate the t that minimizes the distance.
			Double t = ((pt.X - P1.X) * dx + (pt.Y - P1.Y) * dy) / (dx * dx + dy * dy);

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

			GeoPoint ret = new GeoPoint(closest);
			distance = EarthGeo.GetDistance(pt, ret);
			return ret;
		}

		public GeoPoint ClosestEndPointTo(GeoPoint point, out Double distance)
		{
			Double d1 = EarthGeo.GetDistance(_P1, point);
			Double d2 = EarthGeo.GetDistance(_P2, point);
			distance = Math.Min(d1, d2);
			return d1 < d2 ? _P1 : _P2;
		}

		public void ExtendTo(GeoPoint point)
		{
			Double d1 = EarthGeo.GetDistance(_P1, point);
			Double d2 = EarthGeo.GetDistance(_P2, point);

			if(d1 < d2)
			{
				_P1 = point;
			}
			else
			{
				_P2 = point;
			}
		}

		#endregion

		#region Comparer Classes

		public class NameComparer : IComparer<GeoLine>
		{
			public int Compare(GeoLine x, GeoLine y)
			{
				return x.Name.CompareTo(y.Name);
			}
		}


		#endregion

		#region Utility

		public GeoLine Clone()
		{
			return new GeoLine(P1 as GeoPoint, P2 as GeoPoint);
		}

		public static GeoLine Parse(String value)
		{
			GeoLine parsed;
			if(!TryParse(value, out parsed))
			{
				throw new GeometryException("Nonparseable GeoLine");
			}
			return parsed;
		}

		public static bool TryParse(String value, out GeoLine line)
		{
			line = null;
			int index1  = value.IndexOf(GeoPoint.TOSTRING_DELIM);

			String name = String.Empty;
			if(index1 > 0)
			{
				name = value.Substring(0, index1).Trim();
				value = value.Substring(index1).Trim();
			}

			int index = value.IndexOf(" - ");
			if(index > 0)
			{
				String s1 = value.Substring(0, index).Trim();
				String s2 = value.Substring(index + 2).Trim();

				GeoPoint p1, p2;
				if(GeoPoint.TryParse(s1, out p1) && GeoPoint.TryParse(s2, out p2))
				{
					line = new GeoLine(p1, p2);
					line.Name = name;
				}
			}
			return line != null;
		}

		public Line ToLine()
		{
			return new Line(P1.ToPointD(), P2.ToPointD());
		}

		public bool Equals(GeoLine l)
		{
			return (l.P1.X == P1.X && l.P1.Y == P1.Y && l.P2.X == P2.X && l.P2.Y == P2.Y);
		}

		public String ToString(int precision)
		{
			String name = String.Empty;
			if(String.IsNullOrEmpty(Name) == false)
			{
				name = String.Format("{0} - ", Name);
			}
			return String.Format("{0}{1} - {2}", name, _P1.ToString(precision), _P2.ToString(precision));
		}

		public override string ToString()
		{
			return ToString(EarthGeo.GeoPrecision);
		}

		#endregion
	}
}
