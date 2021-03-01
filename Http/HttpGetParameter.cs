using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Http
{
	public class HttpGetParameter
	{
		public String Key { get; set; }
		public Object Value { get; set; }

		public HttpGetParameter() { }
		public HttpGetParameter(String key, Object value) 
		{
			Key = key;
			Value = value;
		}

		public class List : List<HttpGetParameter> { }
	}
}
