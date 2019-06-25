using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Geometry
{
	public class GeoGrid : GeoRectangle
	{
        public Int32 NumberOfRows { get; private set;}

        public Int32 NumberOfColumns {get; private set;}

		public GeoGrid(GeoPoint p1, GeoPoint p2, GeoPoint p3, GeoPoint p4, Int32 numberOfRows, Int32 numberOfColumns) :
            base (p1, p2, p3, p4)
        {
            NumberOfRows = numberOfRows;
            NumberOfColumns = numberOfColumns;
        }


		public override string ToString()
		{
			return String.Format("NW: {0}  NE: {1}  SE: {2}  SW: {3} NR: {4} NC: {5}", NorthWest, NorthEast, SouthEast, SouthWest, NumberOfRows, NumberOfColumns);
		}

	}
}
