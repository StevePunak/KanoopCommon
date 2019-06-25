using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.Extensions;
using KanoopCommon.Logging;

namespace KanoopCommon.Geometry
{
	public class PointCloud2D : BearingAndRangeList
	{
		public Double VectorSize { get { return (Double)360 / (Double)this.Count; } }

		public int FilledVectors { get { return this.Count(bar => bar.Range != 0); } }

		public PointCloud2D()
			: base() {}

		public PointCloud2D(Double size)
			: base((int)size)	{}

		public PointCloud2D(byte[] serialized)
		{
			using(BinaryReader br = new BinaryReader(new MemoryStream(serialized)))
			{
				base.LoadFromReader(br);
			}
		}

		/// <summary>
		/// return a slice of the given point cloud
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="bearing"></param>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public PointCloud2DSlice GetPointCloud2DSlice(PointD origin, Double bearing, Double from, Double to)
		{
			PointCloud2DSlice slice = new PointCloud2DSlice(origin, bearing);
			for(Double offset = from / VectorSize;offset != to / VectorSize;offset = offset.AddDegrees(VectorSize))
			{
				if(this[(int)offset].Range != 0)
				{
					slice.Add(this[(int)offset]);
				}
			}
			return slice;
		}

		/// <summary>
		/// Get the range at the given bearing
		/// </summary>
		/// <param name="bearing"></param>
		/// <returns></returns>
		public Double GetRangeAtBearing(Double bearing)
		{
			Double offset = bearing / VectorSize;
			return this[(int)offset].Range;
		}

		/// <summary>
		/// Dump the vectors to the system log
		/// </summary>
		public void DumpToLog(String tag)
		{
			Log.SysLogText(LogLevel.DEBUG, "Dumping Point Cloud {0}", tag);
			foreach(BearingAndRange bar in this)
			{
				if(bar.Range != 0)
				{
					Log.SysLogText(LogLevel.DEBUG, "   {0}", bar);
				}
			}
		}

		/// <summary>
		/// Return the offset into this cloud for the give angle
		/// </summary>
		/// <param name="bearing"></param>
		/// <returns></returns>
		public int VectorOffset(Double bearing)
		{
			return (int)((Double)bearing / VectorSize);
		}

		public new PointCloud2D Clone()
		{
			PointCloud2D list = new PointCloud2D();
			foreach(BearingAndRange rab in this)
			{
				list.Add(rab.Clone());
			}
			return list;
		}
	}
}
