using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Http
{
	public interface IHttpHeader
	{
		String Key { get; }

		String Value { get; set; }
	}
}
