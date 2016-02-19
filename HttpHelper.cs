using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace reportgen
{
	public static class HttpHelper
	{
		/// <summary>
		/// Get the content of specified url.
		/// </summary>
		/// <param name="url">URL.</param>
		public static string Get (string url)
		{
			string cookieValue = ReadTxtFile ("cookie.txt");
			string result = string.Empty;
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("JSESSIONID", cookieValue);
			cc.Add (new Uri (url), cookie);

			X509Certificate cert = X509Certificate.CreateFromCertFile ("key.cer");
			HttpWebRequest httpWebReq = (HttpWebRequest)WebRequest.Create (url);
			httpWebReq.ClientCertificates.Add (cert);
			httpWebReq.CookieContainer = cc;

			bool cookieExpired = false;

			try {
				using (WebResponse webResponse = httpWebReq.GetResponse ()) {
					using (Stream stream = webResponse.GetResponseStream ()) {
						using (StreamReader streamReader = new StreamReader (stream, Encoding.UTF8)) {
							result = streamReader.ReadToEnd ();
						}
					}
				}
			} catch (Exception e) {
				cookieExpired = true;
			}

			if (!cookieExpired) {
				return result;
			}

			throw new ArgumentException ("cookie expired");
		}

		/// <summary>
		/// Gets a request for sending request with custom headers.
		/// </summary>
		/// <returns>The request.</returns>
		/// <param name="url">URL.</param>
		public static HttpWebRequest GetRequest (string url)
		{
			string cookieValue = ReadTxtFile ("cookie.txt");
			string result = string.Empty;
			CookieContainer cc = new CookieContainer ();
			Cookie cookie = new Cookie ("JSESSIONID", cookieValue);
			cc.Add (new Uri (url), cookie);

			X509Certificate cert = X509Certificate.CreateFromCertFile ("key.cer");
			HttpWebRequest httpWebReq = (HttpWebRequest)WebRequest.Create (url);
			httpWebReq.ClientCertificates.Add (cert);
			httpWebReq.CookieContainer = cc;

			return httpWebReq;
		}

		/// <summary>
		/// Reads the text file.
		/// </summary>
		/// <returns>The text file.</returns>
		/// <param name="file">File.</param>
		static string ReadTxtFile (string file)
		{
			string result = string.Empty;
			using (var sr = new StreamReader (file)) {
				result = sr.ReadLine ();	
			}
			return result;
		}
	}
}

