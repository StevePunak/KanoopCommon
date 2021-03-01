using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Extensions;
using System.IO;
using System.Reflection;
using KanoopCommon.Logging;

namespace KanoopCommon.Http
{
	public class HttpHeaderList : List<IHttpHeader>
	{
		public HttpHeaderList()
			: base() {}

		public HttpHeaderList(IEnumerable<IHttpHeader> other)
			: base(other) {}

		public IHttpHeader this[String key]
		{
			get
			{
				IHttpHeader header;
				if(TryGetValue(key, out header) == false)
				{
					throw new ArgumentOutOfRangeException("Invalid index '{0}'", key);
				}
				return header;
			}
		}

		public bool Contains(Type type)
		{
			return this.Find(h => h.GetType() == type) != null;
		}

		public bool TryGetValue(Type type, out IHttpHeader header)
		{
			header = this.Find(h => h.GetType() == type);
			return header != null;
		}

		public bool TryGetValue(String key, out IHttpHeader header)
		{
			header = this.Find(h => h.Key == key);
			return header != null;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach(IHttpHeader header in this)
			{
				sb.AppendFormat("{0}\r\n", header);
			}
			return sb.ToString();
		}

		public static HttpHeaderList Parse(String headerString)
		{
			HttpHeaderList list = new HttpHeaderList();
			using(TextReader tr = new StringReader(headerString))
			{
				String line;
				while((line = tr.ReadLine()) != null)
				{
					int index = line.IndexOf(':');
					if(index > 0)
					{
						String key = line.Substring(0, index);
						String value = line.Substring(index + 1).Trim();
						Type type;
						if(_stringToTypeIndex.TryGetValue(key, out type))
						{
							try
							{
								ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(String) });
								if(constructor != null)
								{
									IHttpHeader header = constructor.Invoke(new object[] { value } ) as IHttpHeader;
									if(header != null)
									{
										list.Add(header);
									}
								}
							}
							catch(Exception)
							{}
						}
					}
				}
			}
			return list;
		}

		static Dictionary<String, Type> _stringToTypeIndex;
		static HttpHeaderList()
		{
			_stringToTypeIndex = new Dictionary<String, Type>();
			foreach(Type type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if(type.IsSubclassOf(typeof(HttpHeader.HeaderBase)) && type.IsAbstract == false)
				{
					ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(String) });
					if(constructor != null)
					{
						IHttpHeader header = constructor.Invoke(new object[] { String.Empty } ) as IHttpHeader;
						if(header != null)
						{
							_stringToTypeIndex.Add(header.Key, type);
						}
					}
				}
			}
		}

	};

	public class HttpHeader
	{
		#region Strings

		public class Strings
		{
			public const String ApplicationJson			= "application/json";
		}

		#endregion

		public abstract class DateHeader : HeaderBase
		{
			public DateTime Date { get; private set; }

			public DateHeader(DateTime when)
				: base(when.ToHttpHeaderString()) 
			{
				Date = when;
			}

			public DateHeader(String value)
				: base(value)
			{
				DateTime date;
				DateTime.TryParse(value, out date);
				Date = date;
			}
		}

		public class LastModified : DateHeader
		{
			public override String Key { get { return "Last-Modified"; } }

			public LastModified(DateTime when)
				: base(when.ToHttpHeaderString()) {}

			public LastModified(String value)
				: base(value) {}
		}

		public class Date : DateHeader
		{
			public override String Key { get { return "Date"; } }

			public Date(DateTime when)
				: base(when.ToHttpHeaderString()) {}

			public Date(String value)
				: base(value) {}
		}

		public class Expires : DateHeader
		{
			public override String Key { get { return "Expires"; } }

			public Expires(DateTime when)
				: base(when.ToHttpHeaderString()) {}

			public Expires(String value)
				: base(value) {}
		}

		public class ContentLength : HeaderBase
		{
			public override String Key { get { return "Content-Length"; } }

			public int Length { get; private set; }

			public ContentLength(UInt32 length)
				: this((int)length) {}

			public ContentLength(int length)
				: base(length.ToString()) 
			{
				Length = length;
			}
		}
		
		public class ContentType : HeaderBase
		{
			public override String Key { get { return "Content-Type"; } }

			public String Type { get; private set; }

			public ContentType(String type)
				: base(type) 
			{
				Type = type;
			}
		}
		
		public class FileName : HeaderBase
		{
			public override String Key { get { return "FileName"; } }

			public FileName(String value)
				: base(value) {} 
		}
		
		public class CacheControl : HeaderBase
		{
			public override String Key { get { return "Cache-Control"; } }

			public TimeSpan MaxAge { get; private set; }

			public String Scope { get; set; }

			public CacheControl(TimeSpan maxAge)
				: base(maxAge.TotalSeconds.ToString()) {}

			public CacheControl(String value)
				: base(value)
			{
				String[] parts = value.Split(',');
				Scope = parts[0].Trim();

				int seconds = 0;
				if(parts.Length > 1)
				{
					int.TryParse(parts[1], out seconds);
				}
				MaxAge = TimeSpan.FromSeconds(seconds);
			}

			public override string ToString()
			{
				return String.Format("{0}, {1}", Scope, MaxAge.TotalSeconds);
			}
		}

		public class GeneralHeader : HeaderBase
		{
			string _key;
			public override String Key { get { return _key; } }

			public GeneralHeader(String key, String value)
				: base(value) 
			{
				_key = key;
			} 
		}
		
		public abstract class HeaderBase : IHttpHeader
		{
			public String Value { get; set; }

			public abstract String Key { get; }

			protected HeaderBase(String value)
			{
				Value = value;
			}

			public override string ToString()
			{
				return String.Format("{0}: {1}", Key, Value);
			}
		}


	}
}
