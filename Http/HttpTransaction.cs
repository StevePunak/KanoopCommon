using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace KanoopCommon.Http
{
	public abstract class HttpTransaction
	{
		#region Enumerations

		public enum CompressionTypes
		{
			None = 0,
			Gzip = 1,
			Deflate = 2
		}

		#endregion

		#region Public Properties

		public String URI { get; set; }

		public bool SslValidationDisabled {  get; set; }

		public Dictionary<String, String> ParameterList { get; set; }

		protected HttpWebRequest		_httpRequest;
		public HttpWebRequest HttpRequest
		{
			get 
			{
				if(_httpRequest == null)
				{
					_httpRequest = (HttpWebRequest)WebRequest.Create(URI);
				}
				return _httpRequest; 
			}
		}

		protected byte[] _body;
		public byte[] Body
		{
			get
			{
				if(_body == null)
				{
					_body = new byte[0];
				}
				return _body;
			}
			set { _body = value; }
		}

		public void SetBody(String body)
		{
			_body = ASCIIEncoding.UTF8.GetBytes(body);
		}

		public String Authorization { get; set; }

		protected String Method { get; set; }

		public HttpWebResponse HttpResponse { get; protected set; }

		public String Message { get; protected set; }

		String _shortMessage;
		public String ShortMessage { get { return GetShortMessage(); } }

		public String ResponseAsString
		{
			get { return ASCIIEncoding.UTF8.GetString(ResponseAsByteArray); }
		}

		public byte[] ResponseAsByteArray { get; protected set; }

		public CompressionTypes Compression { get; set; }

		HttpHeaderList _headers;
		public HttpHeaderList Headers
		{
			get
			{
				if(_headers == null)
				{
					_headers = new HttpHeaderList();
				}
				return _headers;
			}
		}

		public Exception AsynchException { get; protected set; }

		public bool Success { get; protected set; }

		#endregion

		#region Protected / Private Member Variables

		protected HttpAsynchCallback _callback;

		#endregion

		#region Constructor

		protected HttpTransaction(String uri, String method)
		{
			URI = uri;
			Method = method;
			ParameterList = new Dictionary<string,string>();
			SslValidationDisabled = false;
			_shortMessage = null;
			_callback = null;

			Success = false;
		}

		#endregion

		#region Utility

		private String GetShortMessage()
		{
			String message = _shortMessage;

			const int MAX_SHORT_MESSAGE_LEN = 256;
			const String TITLE_START = "<title>";
			const String TITLE_END = "</title>";
			if(message == null)
			{
				if(Message != null && Message.Length > MAX_SHORT_MESSAGE_LEN)
				{
					int index1, index2;
					if(	(index1 = Message.IndexOf(TITLE_START)) > 0 &&
						(index2 = Message.IndexOf(TITLE_END)) > 0 )
					{
						index1 += TITLE_START.Length;
						message = Message.Substring(index1, index2 - index1);
					}
					else
					{
						message = "No short version of this message available";
					}
				}
				else if(Message != null)
				{
					message = Message;
				}
			}

			return message;
		}

		public static void ExtractAuthorizationFromUrl(String url, out String newUrl, out String authorization)
		{
			newUrl = url;
			authorization = String.Empty;
			if(url.Split(':').Length > 2 && url.Contains('@'))
			{
				String[] parts = url.Split(new char[] { ':', '/', '@' } );
				authorization = String.Format("{0}:{1}", parts[3], parts[4]);
				newUrl = url.Replace(authorization, String.Empty).Replace("@", String.Empty);
			}
		}

		public override string ToString()
		{
			return ResponseAsString;
		}

		#endregion

		#region Parameter / Header Parsing

		protected String CreateParameterString()
		{
			StringBuilder sb = new StringBuilder(1024);
			int x = 0;
			foreach(KeyValuePair<String, String> kvp in ParameterList)
			{
				sb.AppendFormat("{0}={1}", kvp.Key, kvp.Value);
				if(++x != ParameterList.Count)
				{
					sb.Append('&');
				}
			}
			return sb.ToString();
		}

		protected void ParseHeaders()
		{
			Headers.Clear();
			for(int x = 0;x < HttpResponse.Headers.Count;x++)
			{
				Headers.Add(new HttpHeader.GeneralHeader(HttpResponse.Headers.Keys[x], HttpResponse.Headers[x]));
			}

		}

		protected void WriteHeadersToRequest()
		{
			foreach(IHttpHeader header in Headers)
			{
				String key = header.Key;
				String value = header.Value;
				_httpRequest.Headers[key] = value;
			}
		}

		#endregion

		#region Abstract Methods

		public abstract void CreateRequest();

		public abstract bool GetResponse();

		#endregion
	}
}
