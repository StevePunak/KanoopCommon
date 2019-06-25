using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Geometry
{
	public class Ellipse
	{
		#region Public Properties

		public Double Area { get; private set; }

		public Double Eccentricity { get; private set; }

		public PointD[] Foci { get; private set; }

		public Line MajorAxis { get; private set; }

		public Double MajorRadius { get; private set; }

		public Line MinorAxis { get; private set; }

		public Double MinorRadius { get; private set; }

		public PointD Center 
		{ 
			get { return _center; } 
			private set
			{
				_center = value;
			}
		}

		#endregion

		#region Private Members

		/** These member variables actually define the ellipse */
		PointD _center;
		Double _majorAxisLength;
		Double _minorAxisLength;
		Double _majorAxisBearing;

		#endregion

		#region Constructors

		private Ellipse(PointD center, Double length1, Double length2, Double majorAxisBearing)
		{
			_center = center;

			if(MajorAxis == null && MinorAxis == null)
			{
				_majorAxisLength = length1 > length2 ? length1 : length2;
				_minorAxisLength = length1 < length2 ? length1 : length2;
				_majorAxisBearing = majorAxisBearing;

				RecalculateAxes();
			}
			else
			{
				_majorAxisLength = MajorAxis.Length;
				_minorAxisLength = MinorAxis.Length;
				
				_majorAxisBearing = majorAxisBearing;
			}

			RecalculateFociAndArea();
		}

		public Ellipse(Line majorAxis, Line minorAxis)
			: this(majorAxis.MidPoint, majorAxis.Length, minorAxis.Length, majorAxis.Bearing) 
		{
			MajorAxis = majorAxis;
			MinorAxis = minorAxis;
		}

		public Ellipse(RectangleD rectangle)
			: this(	rectangle.Lines[0].Length > rectangle.Lines[1].Length						// major
						? new Line(rectangle.Lines[1].MidPoint, rectangle.Lines[3].MidPoint)
						: new Line(rectangle.Lines[0].MidPoint, rectangle.Lines[2].MidPoint),
					rectangle.Lines[0].Length < rectangle.Lines[1].Length						// minor
						? new Line(rectangle.Lines[1].MidPoint, rectangle.Lines[3].MidPoint)
						: new Line(rectangle.Lines[0].MidPoint, rectangle.Lines[2].MidPoint) ) {}

		#endregion

		#region Public Geometry Methods

		public void Move(PointD where)
		{
			_center = new PointD(where);

			RecalculateAll();
		}

		#endregion

		#region Private Methods

		void RecalculateAll()
		{
			RecalculateAxes();
			RecalculateFociAndArea();
		}

		void RecalculateAxes()
		{
			MajorAxis = new Line(	
				FlatGeo.GetPoint(_center, Angle.Reverse(_majorAxisBearing), _majorAxisLength / 2),
				FlatGeo.GetPoint(_center, _majorAxisBearing, _majorAxisLength / 2));

			Double minorAxisBearing = Angle.Add(_majorAxisBearing, 90);
			MinorAxis = new Line(
				FlatGeo.GetPoint(_center, Angle.Reverse(minorAxisBearing), _minorAxisLength / 2),
				FlatGeo.GetPoint(_center, minorAxisBearing, _minorAxisLength / 2));
		}

		void RecalculateFociAndArea()
		{
			//      __________
			//    \/ r1² * r2²
			Double fociDistance = Math.Sqrt(MajorRadius * MajorRadius - MinorRadius * MinorRadius);	
			Foci = new PointD[]
			{
				FlatGeo.GetPoint(Center, Angle.Reverse(MajorAxis.Bearing), fociDistance),
				FlatGeo.GetPoint(Center, MajorAxis.Bearing, fociDistance)
			};

			Eccentricity = new Line(Foci[0], Center).Length / new Line(Foci[0], MinorAxis.P1).Length;

			Area = Math.PI * MajorRadius * MinorRadius;
		}

		static Line LineFromCenterAndBearing(PointD centerPoint, Double bearing, Double distance)
		{
			PointD p1 = FlatGeo.GetPoint(centerPoint, Angle.Reverse(bearing), distance / 2);
			PointD p2 = FlatGeo.GetPoint(centerPoint, bearing, distance / 2);
			return new Line(p1, p2);
		}


		#endregion

		#region Utility

		public override string ToString()
		{
			return String.Format("Ellipse:  {0} Maj {1} Min {2}", Center, MajorAxis, MinorAxis);
		}

		#endregion
	}
}
