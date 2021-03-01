using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Http
{
	public class HttpDelete : HttpPost
	{
		public HttpDelete(String uri)
			: base(uri) 
		{
			Method = "DELETE";
		}
	}
}
