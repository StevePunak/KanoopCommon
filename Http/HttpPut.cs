using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Http
{
	public class HttpPut : HttpTransaction
	{
		public HttpPut(String url)
			: base(url, "PUT")
		{
		}

		public override void CreateRequest()
		{
			_httpRequest = (HttpWebRequest)WebRequest.Create(URI);
			_httpRequest.Method = Method;
			_httpRequest.ContentType = "application/json";
			WriteHeadersToRequest();
		}

		public override bool GetResponse()
		{
			bool result = false;

			CreateRequest();
			String parameters = CreateParameterString();

			byte[] bytes = ASCIIEncoding.UTF8.GetBytes(parameters);
			int contentLength = bytes.Length + Body.Length;
			_httpRequest.ContentLength = contentLength;

			using(Stream outputStream = _httpRequest.GetRequestStream())
			{
				if(bytes.Length > 0)
				{
					outputStream.Write(bytes, 0, bytes.Length);
				}

				if(Body.Length > 0)
				{
					outputStream.Write(Body, 0, Body.Length);
				}
			}

			try
			{
				HttpResponse = (HttpWebResponse)_httpRequest.GetResponse();
				using(StreamReader sr = new StreamReader(HttpResponse.GetResponseStream()))
				{
					ResponseAsByteArray = ASCIIEncoding.UTF8.GetBytes(sr.ReadToEnd());
					result = HttpResponse.StatusCode == HttpStatusCode.OK || ResponseAsByteArray.Length > 0;
				}
			}
			catch(WebException e)
			{
				HttpResponse = (HttpWebResponse)e.Response;
				Message = e.Message;
				if(e.Response != null)
				{
					using(StreamReader sr = new StreamReader(e.Response.GetResponseStream()))
					{
						ResponseAsByteArray = ASCIIEncoding.UTF8.GetBytes(sr.ReadToEnd());
						if(ResponseAsByteArray.Length > 0)
						{
							// Message = ASCIIEncoding.ASCII.GetString(ResponseAsByteArray);
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
	}
}
