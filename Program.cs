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
	class Program
	{
		/// <summary>
		/// The issues.
		/// </summary>
		static List<Item> issues = new List<Item> ();

		static List<Item> failedAcitivityIssus = new List<Item> ();

		/// <summary>
		/// The issue activity map.
		/// </summary>
		Dictionary<string,string> issueActivityMap = new Dictionary<string, string> ();

		/// <summary>
		/// The locker.
		/// </summary>
		static readonly object locker = new object ();

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		static void Main (string[] args)
		{
			var queries = ReadQueries ("query.txt");

//			Issue.GetAllActivity ("CL-891");
//			return;

			try {
				System.Threading.Tasks.Parallel.ForEach (queries, query => {
					string jiraQuery = HttpUtility.UrlEncode (query);
					// max returns is 1000

					string url = string.Format ("https://jira.xxx.com/sr/jira.issueviews:searchrequest-xml/temp/SearchRequest.xml?tempMax={0}&jqlQuery={1}", 1000, jiraQuery);
					Console.WriteLine ("going to fetch data from {0}", url);
					var xml = HttpHelper.Get (url);
					Console.WriteLine ("downloaded, going to parse data.");
					ParseXml (xml);
					Console.WriteLine ("data parsed for url {0}", url);
				});
			} catch (AggregateException ex) {
				Console.WriteLine ("failed to download data from remote");
			}

			if (issues.Count > 0) {
				Console.WriteLine ("done.");
				Console.WriteLine ("{0} items fetched", issues.Count);
				Console.WriteLine ("fetching issue activity");
				FetchIssueAcivity (issues);
				if (failedAcitivityIssus.Count > 0) {
					// try again for the failed ones
					FetchIssueAcivity (failedAcitivityIssus,true);
				}
				Console.WriteLine ("done");
				Console.WriteLine ("going to build report.");
				builder.Build (issues);
				Console.WriteLine ("report built successfully");
			} else {
				Console.WriteLine ("failed to fetch data,please consider changing the cookie config");
			}
			Console.WriteLine ("Press any key to exit");
			Console.ReadKey ();
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

		/// <summary>
		/// read queries from query.txt , it may contain multiple queries , seperated by line 
		/// </summary>
		/// <returns>The queries.</returns>
		/// <param name="file">File.</param>
		static List<string> ReadQueries (string file)
		{
			var queries = new List<string> (10);
			string result = string.Empty;
			using (var sr = new StreamReader (file)) {
				while (!string.IsNullOrEmpty ((result = sr.ReadLine ()))) {
					queries.Add (result.Trim ());
				}	
			}
			return queries;
		}

		/// <summary>
		/// Parses the xml.
		/// </summary>
		/// <param name="xml">Xml.</param>
		static void ParseXml (string xml)
		{
			lock (locker) {
				var xdoc = XDocument.Parse (xml);
				var items = xdoc.Descendants ("item");

				Item entity = null;
				foreach (var item in items) {
					Console.Write (".");
					entity = new Item ();
					entity.Assignee = item.Element ("assignee").Value;
					entity.Components = "";
					entity.Created = DateTime.Parse (item.Element ("created").Value);
					entity.Description = item.Element ("description").Value;
					entity.Summary = item.Element ("summary").Value;
					entity.IsUrgentFix = GetCustomField (item, "Is Urgent Fix?");
					entity.Key = item.Element ("key").Value;
					entity.Labels = item.Element ("labels").Value;
					entity.Link = item.Element ("link").Value;
					entity.Priority = item.Element ("priority").Value;
					entity.Reporter = item.Element ("reporter").Value;
					entity.Resolution = item.Element ("resolution").Value;
					if (item.Element ("resolved") != null) {
						entity.Resolved = DateTime.Parse (item.Element ("resolved").Value);
					}
					entity.Severity = GetCustomField (item, "Severity");
					entity.Status = item.Element ("status").Value;
					entity.Title = item.Element ("title").Value;
					entity.Type = item.Element ("type").Value;
					if (item.Element ("updated") != null) {
						entity.Updated = DateTime.Parse (item.Element ("updated").Value);
					}
					//entity.Comments = ParseComments (item);

					issues.Add (entity);
				}
			}
		}

		/// <summary>
		/// Fetchs the issue acivity.
		/// </summary>
		/// <param name="issueList">Issue list.</param>
		/// <param name="isRetry">If set to <c>true</c> is retry.</param>
		static void FetchIssueAcivity (List<Item> issueList, bool isRetry=false)
		{
			Parallel.ForEach (issueList, issue => {
				if (!isRetry) {
					Console.Write (".");
				} else {
					Console.Write ("Refetching " + issue.Key);
				}
				try {
					issue.Activity = Issue.GetAllActivity (issue.Key);
				} catch {
					Console.Write ("Activity faild for " + issue.Key);
					failedAcitivityIssus.Add (issue);
				}
			});
		}

		/// <summary>
		/// Gets the custom field.
		/// </summary>
		/// <returns>The custom field.</returns>
		/// <param name="el">El.</param>
		/// <param name="fieldName">Field name.</param>
		static string GetCustomField (XElement el, string fieldName)
		{
			foreach (var item in el.Element ("customfields").Elements()) {
				if (item.Element ("customfieldname").Value == fieldName) {
					return item.Element ("customfieldvalues").Element ("customfieldvalue").Value;
				}
			}
			return "";
		}

		// parse comment in the xml
		static List<Comment> ParseComments(XElement el){
			List<Comment> list = new List<Comment> ();
			Comment obj = null;
			foreach (var item in el.Element ("comments").Elements()) {
				obj = new Comment ();
				obj.Author = item.Attribute ("author").Value;
				obj.created = DateTime.Parse (item.Attribute ("created").Value);
				obj.content = item.Value.Replace ("<p>", "").Replace ("</p>", "");

				list.Add (obj);
			}

			return list;
		}
	}
}