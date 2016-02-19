using System;
using HtmlAgilityPack;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace reportgen
{
	public static class Issue
	{
		const string URL = "https://jira.dianrong.com/browse/{0}?page=com.atlassian.jira.plugin.system.issuetabpanels:all-tabpanel&_pjax=true&_=1453102454626";
		static readonly object locker = new object ();

		/// <summary>
		/// Gets all activity
		/// </summary>
		/// <returns>The all activity.</returns>
		/// <param name="issueId">Issue identifier.</param>
		public static string GetAllActivity (string issueId)
		{
			string requestUrl = string.Format (URL, issueId);
			var request = HttpHelper.GetRequest (requestUrl);
			request.Headers.Add ("X-Requested-With", "XMLHttpRequest");
			request.Headers.Add ("X-PJAX", "true");
			//request.Headers.Add ("X-AUSERNAME", "guochao.wang@dianrong.com");
			string content = string.Empty;

			using (WebResponse webResponse = request.GetResponse ()) {
				using (Stream stream = webResponse.GetResponseStream ()) {
					using (StreamReader streamReader = new StreamReader (stream, Encoding.UTF8)) {
						content = streamReader.ReadToEnd ();
					}
				}
			}

		
//			HtmlDocument doc = new HtmlDocument ();
//			doc.LoadHtml (content);
//			string all = doc.DocumentNode.SelectSingleNode ("//div[@class=\"issuePanelContainer\"]").InnerText;
//
//			return all;
			return ExtractActivity (content);
		}

		/// <summary>
		/// Extracts all activity.  operation history and comments now .
		/// </summary>
		/// <returns>The activity.</returns>
		/// <param name="html">Html.</param>
		static string ExtractActivity (string html)
		{
			var doc = new HtmlDocument ();
			doc.LoadHtml (html);

			var container = doc.DocumentNode.SelectSingleNode ("//div[@class=\"issuePanelContainer\"]");

			StringBuilder sb = new StringBuilder ();
			foreach (var child in container.ChildNodes) {
				if (child.OuterHtml.Contains ("changehistory-")) {
					// change history
					var childDoc = new HtmlDocument ();
					childDoc.LoadHtml (child.OuterHtml);
					var username = childDoc.DocumentNode.SelectSingleNode ("//a[@class=\"user-hover user-avatar\"]").InnerText;
					var date = DateTime.Parse (childDoc.DocumentNode.SelectSingleNode ("//time").Attributes ["datetime"].Value);
					sb.AppendFormat ("<p><b>{0}</b> 于{1} 做了如下更新: </p>", username, date);

					// table data
					var tbody = childDoc.DocumentNode.SelectSingleNode ("//tbody");
					foreach (var tr in tbody.ChildNodes) {
						if (tr.OuterHtml.Contains ("td")) {
							var trDoc = new HtmlDocument ();
							trDoc.LoadHtml (tr.OuterHtml);
							var activityName = trDoc.DocumentNode.SelectSingleNode ("//td[@class=\"activity-name\"]").InnerText.Replace ("\n", "").Trim ();
							var oldValue = trDoc.DocumentNode.SelectSingleNode ("//td[@class=\"activity-old-val\"]").InnerText.Replace ("\n", "").Trim ();
							var newValue = trDoc.DocumentNode.SelectSingleNode ("//td[@class=\"activity-new-val\"]").InnerText.Replace ("\n", "").Trim ();
							//sb.AppendFormat ("<p>{0} 于{1} 更改了{2} : {3} --> {4}</p>", username, date, activityName, oldValue, newValue);
							sb.AppendFormat ("<p>{0} : {1} --> {2}</p>", activityName, oldValue, newValue);
						}
					}
				} else if(child.OuterHtml.Contains ("comment-")){
					// comment
					var childDoc = new HtmlDocument ();
					childDoc.LoadHtml (child.OuterHtml);
					var username = childDoc.DocumentNode.SelectSingleNode ("//a[@class=\"user-hover user-avatar\"]").InnerText;
					var date = DateTime.Parse (childDoc.DocumentNode.SelectSingleNode ("//time").Attributes ["datetime"].Value);
					var comment = childDoc.DocumentNode.SelectSingleNode ("//p").InnerText;
					sb.AppendFormat ("<p><b>{0}</b> 于{1} 添加了Comment:{2} </p>", username, date, comment);
				}

			}

			return sb.ToString ();
		}
			
	}
}

