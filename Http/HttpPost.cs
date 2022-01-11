using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Specialized;

namespace KanoopCommon.Http
{
	public class HttpPost : HttpTransaction
	{
		public HttpPost(String url)
			: base(url, "POST") 
		{
			_httpRequest = (HttpWebRequest)WebRequest.Create(URI);
		}

		public override void CreateRequest()
		{
			_httpRequest.Method = Method;
			if(_httpRequest.ContentType == null)
			{
				_httpRequest.ContentType = "application/x-www-form-urlencoded";
			}

			WriteHeadersToRequest();

			if(SslValidationDisabled)
			{
				_httpRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
			}

            if (String.IsNullOrEmpty(Authorization) == false)
            {
                byte[] authBytes = ASCIIEncoding.UTF8.GetBytes(Authorization.ToCharArray());
                _httpRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(authBytes);
            }
		}
		public bool UploadFiles(String file, NameValueCollection formFields = null)
		{
			return UploadFiles(new List<String>() { file }, formFields);
		}

		public bool UploadFiles(List<String> files, NameValueCollection formFields = null)
		{
			bool result = false;
			string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

			CreateRequest();
//			_httpRequest = (HttpWebRequest) WebRequest.Create(URI);
			_httpRequest.ContentType = String.Format("multipart/form-data; boundary={0}", boundary);
			_httpRequest.Method = "POST";
			_httpRequest.KeepAlive = true;

			Stream memStream = new System.IO.MemoryStream();

			var boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
															boundary + "\r\n");
			var endBoundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
																boundary + "--");


			string formdataTemplate = "\r\n--" + boundary +
								"\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

			if(formFields != null)
			{
				foreach(string key in formFields.Keys)
				{
					string formitem = string.Format(formdataTemplate, key, formFields[key]);
					byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
					memStream.Write(formitembytes, 0, formitembytes.Length);
				}
			}

			HttpHeaderList headers = new HttpHeaderList(Headers);		// make a copy of our headers

			const String HDR_DISPOSITION = "Content-Disposition";
			const String HDR_CONTENT_TYPE = "Content-Type";
			headers.Add(new HttpHeader.GeneralHeader(HDR_DISPOSITION, ""));
			headers.Add(new HttpHeader.GeneralHeader(HDR_CONTENT_TYPE, "application/octet-stream"));

			for(int i = 0;i < files.Count;i++)
			{
				memStream.Write(boundarybytes, 0, boundarybytes.Length);

				String dispHeader = String.Format("form-data; name =\"{0}\"; filename=\"{1}\"", "uploadFile", files[i]);

				headers[HDR_DISPOSITION].Value = dispHeader;

				String headersString = headers.ToString() + "\r\n";
				Byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(headersString);

				memStream.Write(headerbytes, 0, headerbytes.Length);

				using(FileStream fileStream = new FileStream(files[i], FileMode.Open, FileAccess.Read))
				{
					var buffer = new byte[1024];
					var bytesRead = 0;
					while((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
					{
						memStream.Write(buffer, 0, bytesRead);
					}
				}
			}

			memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
			_httpRequest.ContentLength = memStream.Length;

			using(Stream requestStream = _httpRequest.GetRequestStream())
			{
				memStream.Position = 0;
				byte[] tempBuffer = new byte[memStream.Length];
				memStream.Read(tempBuffer, 0, tempBuffer.Length);
				memStream.Close();
				requestStream.Write(tempBuffer, 0, tempBuffer.Length);
			}

			using(var response = _httpRequest.GetResponse())
			{
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
	
		public override bool GetResponse()
		{
			bool result = false;

            CreateRequest();
			String parameters = CreateParameterString();

			byte[] bytes = ASCIIEncoding.UTF8.GetBytes(parameters);
			int nContentLength = bytes.Length + Body.Length;
			_httpRequest.ContentLength = nContentLength;

			Stream os = _httpRequest.GetRequestStream();

			if(bytes.Length > 0)
			{
				os.Write(bytes, 0, bytes.Length);
			}

			if(Body.Length > 0)
			{
				os.Write(Body, 0, Body.Length);
			}
			os.Close();

			try
			{
				HttpResponse = (HttpWebResponse)_httpRequest.GetResponse();
				using (StreamReader  sr = new StreamReader(HttpResponse.GetResponseStream()))
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
					using (StreamReader sr = new StreamReader(e.Response.GetResponseStream()))
					{
						ResponseAsByteArray = ASCIIEncoding.UTF8.GetBytes(sr.ReadToEnd());
						if (ResponseAsByteArray.Length > 0)
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

	}
}
