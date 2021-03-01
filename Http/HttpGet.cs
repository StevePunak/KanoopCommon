using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using KanoopCommon.Logging;

namespace KanoopCommon.Http
{
	public class HttpGet : HttpTransaction
	{
		#region Constructor

		public HttpGet(String uri)
			: base(uri, "GET") {}

		#endregion

		#region Public Access Methods

		public void AddUrlParameter(String key, Object value)
		{
			if(ParameterList.ContainsKey(key) == false)
				ParameterList.Add(key, value.ToString());
			else
				ParameterList[key] = value.ToString();
		}

		public void AddUrlParameter(HttpGetParameter parameter)
		{
			AddUrlParameter(parameter.Key, parameter.Value);
		}

		public void AddUrlParameters(HttpGetParameter.List parameters)
		{
			foreach(HttpGetParameter parameter in parameters)
				AddUrlParameter(parameter);
		}

		public override void CreateRequest()
		{
			/** build request */
			String parameters = CreateParameterString();
			String request = parameters.Length != 0
				? String.Format("{0}?{1}", URI.TrimEnd('/'), parameters)
				: URI.TrimEnd('/');

			if (_httpRequest == null)
			{
				_httpRequest = (HttpWebRequest)WebRequest.Create(request);
			}

			foreach(IHttpHeader header in Headers)
			{
				String key = header.Key;
				String value = header.Value;
				_httpRequest.Headers[key] = value;
			}

			if(SslValidationDisabled)
			{
				_httpRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
			}

			if (String.IsNullOrEmpty(Authorization) == false)
			{
				byte[] authBytes = ASCIIEncoding.UTF8.GetBytes(Authorization.ToCharArray());
				_httpRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(authBytes);
			}

			_httpRequest.Method = "GET";

			switch(Compression)
			{
				case CompressionTypes.Gzip:
					HttpRequest.Headers["Accept-Encoding"] = "gzip";
					break;

				case CompressionTypes.Deflate:
					HttpRequest.Headers["Accept-Encoding"] = "deflate";
					break;
			}
		}

		public override bool GetResponse()
		{
			bool result = false;

			CreateRequest();

			try
			{
				HttpResponse = (HttpWebResponse)_httpRequest.GetResponse();

				result = ProcessResponse(this);
			}
			catch (WebException e)
			{
				HttpResponse = (HttpWebResponse)e.Response;
				Message = e.Message;

				if (e.Response != null)
				{
					using (StreamReader sr = new StreamReader(e.Response.GetResponseStream()))
					{
						ResponseAsByteArray = ASCIIEncoding.UTF8.GetBytes(sr.ReadToEnd());
						if(ResponseAsByteArray.Length > 0)
						{
							Message = ASCIIEncoding.ASCII.GetString(ResponseAsByteArray);
						}
					}
				}
			}
			catch(Exception e)
			{
				Message = e.Message;
			}


			return result;
		}

		public void BeginGetResponseAsynch(HttpAsynchCallback callback)
		{
			_callback = callback;

			CreateRequest();

			_httpRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), this);
		}

		public void Abort()
		{
			_httpRequest.Abort();
		}

		private static void GetResponseCallback(IAsyncResult callbackResult)
		{
			HttpGet get = null;

			try
			{
				get = callbackResult.AsyncState as HttpGet;
				get.HttpResponse = get.HttpRequest.EndGetResponse(callbackResult) as HttpWebResponse;

				ProcessResponse(get);

				get._callback(get);
			}
			catch(Exception e)
			{
				Log.SysLogText(LogLevel.WARNING, "HTTP Asynch Response Callback EXCEPTION {0}", e.Message);
			}
		}

		private static bool ProcessResponse(HttpGet get)
		{
			try
			{
				if(	get.HttpResponse.ContentType.StartsWith("image") || 
					get.HttpResponse.ContentEncoding == "gzip" || 
					get.HttpResponse.ContentEncoding == "deflate")
				{
					BinaryReader reader = new BinaryReader(get.HttpResponse.GetResponseStream());
					MemoryStream ms = new MemoryStream();

					int byteCount = 0;
					byte[] inBuf;
					do
					{
						inBuf = reader.ReadBytes(4096);
						byteCount += inBuf.Length;
						ms.Write(inBuf, 0, inBuf.Length);
					}while(inBuf.Length != 0);
					get.ResponseAsByteArray = ms.ToArray();
				}
				else
				{
					using (StreamReader sr = new StreamReader(get.HttpResponse.GetResponseStream()))
					{
						get.ResponseAsByteArray = ASCIIEncoding.UTF8.GetBytes(sr.ReadToEnd());
	//					byte[] x = HttpUtility.UrlDecodeToBytes(ASCIIEncoding.UTF8.GetString(m_Response));
					}
				}
				get.ParseHeaders();
				get.Success = get.HttpResponse.StatusCode == HttpStatusCode.OK || get.ResponseAsByteArray.Length > 0;

			}
			catch (WebException e)
			{
				get.AsynchException = e;
				get.HttpResponse = (HttpWebResponse)e.Response;
				get.Message = e.Message;

				if (e.Response != null)
				{
					using (StreamReader sr = new StreamReader(e.Response.GetResponseStream()))
					{
						get.ResponseAsByteArray = ASCIIEncoding.UTF8.GetBytes(sr.ReadToEnd());
						if(get.ResponseAsByteArray.Length > 0)
						{
							get.Message = ASCIIEncoding.ASCII.GetString(get.ResponseAsByteArray);
						}
					}
				}
			}
			catch(Exception e)
			{
				get.AsynchException = e;
				get.Message = e.Message;
			}

			return get.Success;
		}
	}

	#endregion
}
