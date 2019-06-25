using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.CommonObjects
{
	public class ImageException : CommonException
	{
		public ImageException(String format, params object[] args)
			: base(format, args) {}
	}
}
