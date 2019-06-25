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
	public class PointCloud2DSlice : BearingAndRangeList
	{
		#region Public Properties

		public PointD Origin { get; set; }

		public Double Bearing { get; set; }

		public Double MinimumRange { get { return Count > 0 ? this.Min(v => v.Range) : 0;  } }
		
		public override int ByteArraySize { get { return sizeof(Double) + PointD.ByteArraySize + base.ByteArraySize; } }

		#endregion

		#region Constructors

		public PointCloud2DSlice()
			: base() {}

		public PointCloud2DSlice(Double bearing, PointCloud2D cloud, Double sliceSize)
			: this(new PointD(), bearing, cloud, sliceSize) { }

		public PointCloud2DSlice(PointD origin, Double bearing, PointCloud2D cloud, Double sliceSize)
			: this(origin, bearing)
		{
			Double startAngle = bearing.SubtractDegrees(sliceSize / 2);
			int offset = cloud.VectorOffset(startAngle);
			int endOffset = cloud.VectorOffset(bearing.AddDegrees(sliceSize / 2));
			Double vectorSize = cloud.VectorSize;
			for(Double angle = startAngle;offset != endOffset;angle = angle.AddDegrees(vectorSize))
			{
				offset = cloud.VectorOffset(angle);
				if(cloud[offset].Range != 0)
				{
					Add(cloud[offset].Clone());
				}
			}
		}

		public PointCloud2DSlice(PointD origin, Double bearing)
			: base()
		{
			Origin = origin;
			Bearing = bearing;
		}

		public PointCloud2DSlice(BinaryReader br)
			: base()
		{
			Origin = new PointD(br.ReadBytes(PointD.ByteArraySize));
			Bearing = br.ReadDouble();
			LoadFromReader(br);
		}

		#endregion

		#region Public Access Methods

		public PointCloud2DSlice Scale(Double scale)
		{
			PointCloud2DSlice slice = new PointCloud2DSlice(Origin, Bearing);
			foreach(BearingAndRange bar in this)
			{
				slice.Add(bar.Scale(scale));
			}
			return slice;
		}

		#endregion

		#region Static Utility Methods

		#endregion

		#region Serialization

		public override byte[] Serialize()
		{
			Double x1 = this.ByteArraySize;
			double x2 = base.ByteArraySize;
			byte[] serialized = new byte[ByteArraySize];
			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(serialized)))
			{
				bw.Write(Origin.Serialize());
				bw.Write(Bearing);
				base.Serialize(bw);
			}
			return serialized;
		}

		#endregion

		#region Utility

		public new PointCloud2DSlice Clone()
		{
			PointCloud2DSlice list = new PointCloud2DSlice()
			{
				Origin = Origin,
				Bearing = Bearing,
			};
			foreach(BearingAndRange rab in this)
			{
				list.Add(rab.Clone());
			}
			return list;
		}

		#endregion
	}
}
