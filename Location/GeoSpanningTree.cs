using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Geometry;
using KanoopCommon.Logging;

namespace KanoopCommon.LocationAlgorithms
{
	public class GeoSpanningTree : SpanningTree
	{
		#region Public Properties

		CoordinateMap							_coordinateMap;
		public CoordinateMap CoordinateMap { get { return _coordinateMap; }  }

		#endregion

		#region Constructor(s)
		
		public GeoSpanningTree(GeoLineList geoLines)
			: this(geoLines, MakeCoordinateMap(geoLines)) {}

		public GeoSpanningTree(GeoLineList geoLines, CoordinateMap coordinateMap)
		{
			_coordinateMap = coordinateMap;

			m_LinesToConstructor = GeoSpanningTree.ConvertedGeoLines(geoLines, coordinateMap);

			Initialize();
		}

		#endregion

		#region Primary Funtionality

		public new GeoPath ComputePath()
		{
			if(m_Origin == null || m_Destination == null)
			{
				throw new Exception("Spanning Tree must have origin and destination");
			}

			InitializeVertices();

			while(Cycle())	{}

			return new GeoPath(GetPath());
		}

		public new GeoPointList GetPath()
		{
			/** now work backwards along source paths */
			GeoPointList path = new GeoPointList();
			TreePathVertice vertice = m_Destination;
			TreePathVertice lastInserted;
			do
			{
				if(vertice == null)
				{
					throw new Exception("Can't compute path");
				}
				path.Insert(0, _coordinateMap.GetGeoPoint(vertice.Position));
				lastInserted = vertice;
				vertice = vertice.Source;
			}while(lastInserted != m_Origin);

			return path;
		}

		#endregion

		#region Parent Class Overrides

		public override PointD ConvertPointToPointD(GeoPoint original)
		{
			return (original is GeoPoint) ? _coordinateMap.GetPixelPoint(original as GeoPoint).Round(3) as PointD : original.Round(3) as PointD;
		}

		public override IPoint ConvertPointToNative(IPoint point)
		{
			return (point is PointD) ? _coordinateMap.GetGeoPoint(point as PointD) : point;
		}

		protected override double GetNativeDistance(IPoint p1, IPoint p2)
		{
			return EarthGeo.GetDistance(ConvertPointToNative(p1) as GeoPoint, ConvertPointToNative(p2) as GeoPoint);
		}

		protected override double GetNativeDistance(double distance)
		{
			return _coordinateMap.GetGeoDistance(distance);
		}

		#endregion

		#region Static Utlity

		static LineList ConvertedGeoLines(GeoLineList geoLines, CoordinateMap coordinateMap = null)
		{
			LineList lines = new LineList();

			if(coordinateMap == null)
			{
				coordinateMap = MakeCoordinateMap(geoLines);
				DebugLogText(LogLevel.DEBUG, "Created coordinate map: {0}", coordinateMap);
			}

			int lineCount = 0;
			int pointCount = 0;
			foreach(GeoLine line in geoLines)
			{
				line.SetPrecision(EarthGeo.GeoPrecision);

				/** convert to local coordinate system */
				PointD p1 = coordinateMap.GetPixelPoint(line.P1 as GeoPoint).Round(3) as PointD;
				p1.Name = String.Format("Pt {0}", ++pointCount);

				PointD p2 = coordinateMap.GetPixelPoint(line.P2 as GeoPoint).Round(3) as PointD;
				p2.Name = String.Format("Pt {0}", ++pointCount);

				Line l = new Line(p1, p2);
				l.Name = String.Format("Line {0}", ++lineCount);
				lines.Add(l);

				DebugLogText(LogLevel.DEBUG, "Converted Line {0} {1} to {2}", line, lineCount, l);

			}

			return lines;
		}

		static CoordinateMap MakeCoordinateMap(GeoLineList geoLines)
		{
			GeoRectangle rectangle = geoLines.GetBoundingRectangle();
			return new CoordinateMap(rectangle.NorthWest, EarthGeo.North, 1000);
		}

		#endregion
	}
}
