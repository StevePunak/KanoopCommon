using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Geometry
{
	public class MappingGrid
	{



		public class Square
		{
			public int X { get; set; }
			public int Y { get; set; }

			public Square(int x, int y)
			{
				X = x;
				Y = y;
			}

			public override string ToString()
			{
				return String.Format("{0}, {1}", X, Y);
			}
		}
	}
}
