using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Geometry
{
	public class GeoTriangle : GeoPolygon
	{
		#region Public Properties

		public GeoPoint A	{ get { return Lines[0].P1 as GeoPoint; } }
		public GeoPoint B	{ get { return Lines[1].P1 as GeoPoint; } }
		public GeoPoint C	{ get { return Lines[2].P1 as GeoPoint; } }

		public GeoLine AtoB { get { return new GeoLine(A, B); } }
		public GeoLine AtoC { get { return new GeoLine(A, C); } }
		public GeoLine BtoC { get { return new GeoLine(B, C); } }
		public GeoLine BtoA { get { return new GeoLine(B, A); } }
		public GeoLine CtoA { get { return new GeoLine(C, A); } }
		public GeoLine CtoB { get { return new GeoLine(C, B); } }

		public Angle ABC { get { return new Angle(Math.Abs(BtoA.Bearing - BtoC.Bearing)); } }
		public Angle BAC { get { return new Angle(Math.Abs(AtoB.Bearing - AtoC.Bearing)); } }
		public Angle ACB { get { return new Angle(Math.Abs(CtoA.Bearing - CtoB.Bearing)); } }

		#endregion

		#region Constrcutor(s)

		public GeoTriangle(GeoPoint pA, GeoPoint pB, GeoPoint pC)
			: base(new GeoPointList() { pA, pB, pC} ) { }

		#endregion

		#region Public Utility Methods

		public Angle ClosestToAngle(Double degrees)
		{
			SortedDictionary<Double, Angle> values = new SortedDictionary<Double, Angle>();

			Double entry;

			entry = Math.Abs(degrees - ABC.Degrees);
			values.Add(entry, ABC);

			entry = Math.Abs(degrees - BAC.Degrees);
			while(values.ContainsKey(entry))
				entry += .001;
			values.Add(entry, BAC);

			entry = Math.Abs(degrees - ACB.Degrees);
			while(values.ContainsKey(entry))
				entry += .001;
			values.Add(entry, ACB);

			return values.Values.First();
		}

		#endregion
	}
}
