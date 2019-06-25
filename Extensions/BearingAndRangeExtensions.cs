using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.Geometry;
using KanoopCommon.Logging;

namespace KanoopCommon.Extensions
{
	public static class BearingAndRangeExtensions
	{
		public static byte[] MakeBlob(this BearingAndRangeList vectors)
		{
			byte[] output = new byte[sizeof(Double) * vectors.Count];

			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(output)))
			{
				for(int offset = 0;offset < vectors.Count;offset++)
				{
					IVector vector = vectors[offset];
					bw.Write(vector.Range);
				}
			}
			return output;
		}

		/// <summary>
		/// Create a new Point Cloud offseting in 2D space by the 'offset' vector
		/// </summary>
		/// <param name="inputVectors"></param>
		/// <param name="offsetFromOrigin"></param>
		/// <returns></returns>
		public static PointCloud2D Move(this PointCloud2D inputVectors, BearingAndRange offsetFromOrigin)
		{
			PointCloud2D output = new PointCloud2D(inputVectors.Count);

			Double vectorSize = (Double)360 / (Double)inputVectors.Count;

			PointD originalOrigin = new PointD(0, 0);
			PointD newOrigin = originalOrigin.GetPointAt(offsetFromOrigin);

			int offset = 0;
			for(Double originalBearing = 0;originalBearing < 360;originalBearing += vectorSize, offset++)
			{
				if(inputVectors[offset].Range != 0)
				{
					PointD pointFromOriginalOrigin = originalOrigin.GetPointAt(inputVectors[offset]);
					Line oline = new Line(originalOrigin, pointFromOriginalOrigin);
					Line line = new Line(newOrigin, pointFromOriginalOrigin);

					Double outputOffset = line.Bearing / vectorSize;
					output[(int)outputOffset].Bearing = line.Bearing;
					output[(int)outputOffset].Range = line.Length;
				}
			}

			return output;
		}

		public static Double ShortestRangeBetween(this BearingAndRange[] pointCloud, Double a1, Double a2)
		{
			Double vectorSize = (Double)360 / (Double)pointCloud.Length;
			int startOffset = (int)((Double)a1 / vectorSize);
			int endOffset = (int)((Double)a2 / vectorSize);

			Double shortest = 0;

			for(int offset = startOffset;offset != endOffset;offset = offset + 1 == pointCloud.Length ? 0 : offset + 1)
			{
				if(pointCloud[offset].Range != 0 && (shortest == 0 || pointCloud[offset].Range < shortest))
				{
					shortest = pointCloud[offset].Range;
				}
			}
			return shortest;
		}

	}
}
