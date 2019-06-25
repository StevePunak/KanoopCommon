using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using KanoopCommon.Performance;
using KanoopCommon.Conversions;

namespace KanoopCommon.Geometry
{
	public class CoordinateMap 
	{
		#region Public Properties

		double m_BearingDelta;
		public double BearingDelta
		{
			get { return m_BearingDelta; }
			set {m_BearingDelta = value; }
		}

		double _pixelsPerMeter;
		public double PixelsPerMeter
		{
			get { return _pixelsPerMeter; }
			set { _pixelsPerMeter = value; }
		}

		public double MetersPerPixel
		{
			get { return 1 / PixelsPerMeter; }
		}

		PointD _pixelReferencePoint;
		public PointD PixPoint
		{
			get { return _pixelReferencePoint; }
			set { _pixelReferencePoint = value; }
		}

		GeoPoint _geoReferencePoint;
		public GeoPoint GeoPoint
		{
			get { return _geoReferencePoint; }
			set { _geoReferencePoint = value; }
		}

		#endregion

		#region Constructors

		public CoordinateMap()
			: this(PointD.Empty, GeoPoint.Empty, 0, 0) {}

		public CoordinateMap(GeoPoint geoReferencePoint, Double bearingDelta, Double pixelsPerMeter)
			: this(PointD.Empty, geoReferencePoint, bearingDelta, pixelsPerMeter) {}

		public CoordinateMap(PointD pixelReferencePoint, GeoPoint geoReferencePoint, Double bearingDelta, Double pixelsPerMeter)
		{
			_pixelReferencePoint = pixelReferencePoint;
			_geoReferencePoint = geoReferencePoint;
			m_BearingDelta = bearingDelta;
			_pixelsPerMeter = pixelsPerMeter;
		}

		public CoordinateMap(PointDList pixPointList, GeoPointList geoPointList)
		{
			Initialize(pixPointList, geoPointList);
		}

		void Initialize(PointDList pixPointList, GeoPointList geoPointList)
		{
			// Get and store the difference in pixel orientation to north (Rotation from north)
			double geoBearing = EarthGeo.GetBearing(geoPointList[0], geoPointList[1]);
			double pixBearing = GetPixelBearing(pixPointList[0], pixPointList[1]);
			this.BearingDelta = geoBearing - pixBearing;


			// Get and store the pixel to meter ratio (Scale to meters)
			double geoDistance = EarthGeo.GetDistance(geoPointList[0], geoPointList[1]);
			double pixDistance = new Line(pixPointList[0],pixPointList[1]).Length;
			_pixelsPerMeter = pixDistance / geoDistance;

			// Store a pixel cross reference point, from which all other conversions can happen
			this.PixPoint = new PointD(pixPointList[0].X, pixPointList[0].Y);
			this.GeoPoint = new GeoPoint(geoPointList[0].Y, geoPointList[0].X);
		}

		#endregion

		#region Public Conversion Methods

		public double GetPixelBearing(PointD pt1, PointD pt2)
		{
			double retVal = FlatGeo.Degrees(Math.Atan2(pt2.Y - pt1.Y, pt2.X - pt1.X)) + 90;
			if (retVal < 0)
				retVal = 360 + retVal;
			return retVal;
		}

		public PointD GetPixelPoint(GeoPoint geoPoint)
		{
			// get a bearing to the point from our known point
			double geoBearing = EarthGeo.GetBearing(this.GeoPoint, geoPoint);
			double geoDistance = EarthGeo.GetDistance(this.GeoPoint, geoPoint);

			double pixBearing = FlatGeo.Radians(((geoBearing + 360) - this.BearingDelta) % 360);
			double pixDistance = geoDistance * this.PixelsPerMeter;

			PointD ret = new PointD(this.PixPoint.X + (Math.Sin(pixBearing) * pixDistance),this.PixPoint.Y - (Math.Cos(pixBearing) * pixDistance));
			return ret;

		}

		public GeoPointList GetGeoPointList(PointDList pixPointList)
		{
			GeoPointList retVal = new GeoPointList();
			foreach (PointD pt in pixPointList)
			{
				retVal.Add(GetGeoPoint(pt));
			}
			return retVal;
		}

		public GeoPoint GetGeoPoint(int x, int y)
		{
			return GetGeoPoint(new PointD(x, y));
		}

		public GeoPoint GetGeoPoint(Point pixPoint)
		{
			return GetGeoPoint(new PointD(pixPoint));
		}

		public GeoPoint GetGeoPoint(PointD pixPoint)
		{
			// get a bearing to the point from our known point
			if ((pixPoint.X == this.PixPoint.X) && (pixPoint.Y == this.PixPoint.Y))
				return this.GeoPoint;

			/** draw a line to get our pixel length */
			Line l1 = new Line(pixPoint, this.PixPoint);

			PointD vectorPoint = new PointD(_pixelReferencePoint.X + 100, _pixelReferencePoint.Y);
			double pixBearing = FlatGeo.Angle(pixPoint, _pixelReferencePoint, vectorPoint).Degrees + 90;
			double pixDistance = l1.Length;

			double geoBearing = FlatGeo.Radians((pixBearing + this.BearingDelta) % 360);
			double geoDistance = pixDistance / this.PixelsPerMeter;

			double lat = FlatGeo.Radians(this.GeoPoint.Y);
			double lon = FlatGeo.Radians(this.GeoPoint.X);

			double ddr = geoDistance / EarthGeo.EarthRadius;

			double lat2 = Math.Asin(Math.Sin(lat) * Math.Cos(ddr) + Math.Cos(lat) * Math.Sin(ddr) * Math.Cos(geoBearing));
			double lon2 = lon + Math.Atan2(Math.Sin(geoBearing) * Math.Sin(ddr) * Math.Cos(lat), Math.Cos(ddr) - Math.Sin(lat) * Math.Sin(lat2));
			lon2 = ((lon2 + Math.PI) % (2 * Math.PI)) - Math.PI;

			lat2 = FlatGeo.Degrees(lat2);
			lon2 = FlatGeo.Degrees(lon2);

			return new GeoPoint(lat2, lon2);
		}

		public Double GetPixelDistance(Double geoDistance)
		{
			return geoDistance * PixelsPerMeter;
		}

		public Double GetGeoDistance(Double pixelDistance)
		{
			return pixelDistance /  PixelsPerMeter;
		}

		#endregion

		#region Public Move, Scale, Rotate etc

		/// <summary>
		/// Rotate the coordinate map by the given number of degrees
		/// </summary>
		/// <param name="degrees"></param>
		public void Rotate(Double degrees)
		{
			/**
			 * create a new point some ways off from our reference point
			 */
			PointD newPoint = _pixelReferencePoint.Clone() as PointD;
			newPoint.X += 100;

			/** 
			 * save the lat and long of the new point
			 */
			GeoPoint geoPoint = GetGeoPoint(newPoint);

			/**
			 * rotate the new point about our reference point
			 */
			newPoint.Rotate(_pixelReferencePoint, degrees);

			GeoPointList geoPoints = new GeoPointList() { _geoReferencePoint, geoPoint };
			PointDList pixPoints = new PointDList() { _pixelReferencePoint, newPoint };

			/**
			 * Reinitialize ourself with the new rotation
			 */
			Initialize(pixPoints, geoPoints);
		}

		/// <summary>
		/// Scale the coordinate map by the given amount
		/// </summary>
		/// <param name="scale"></param>
		public void Scale(Double scale)
		{
			/** find the new pixel point */
			_pixelReferencePoint.Scale(scale);

			/** scale the pixels per meter */
			_pixelsPerMeter *= scale;
		}

		/// <summary>
		/// Move the coordinate map by the given number of x and y pixels
		/// </summary>
		/// <param name="degrees"></param>
		public void Move(PointD xyAdjustment)
		{
			this.PixPoint.X += xyAdjustment.X;
			this.PixPoint.Y += xyAdjustment.Y;
		}

		#endregion

		#region Utility

		public CoordinateMap Clone()
		{
			CoordinateMap ret = new CoordinateMap();
			ret.m_BearingDelta = m_BearingDelta;
			ret._pixelsPerMeter = _pixelsPerMeter;
			ret._pixelReferencePoint = _pixelReferencePoint.Clone() as PointD;
			ret._geoReferencePoint = _geoReferencePoint.Clone() as GeoPoint;
			return ret;
		}

		public override string ToString()
		{
			return String.Format("CM: {0:0.00}ppm  PR: {1}", PixelsPerMeter, PixPoint);
		}

		#endregion

	}

}
