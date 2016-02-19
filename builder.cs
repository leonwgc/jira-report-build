using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace reportgen
{
	public class builder
	{
		const string head = "<table border=1><tr>\n    <th>Key</th>\n    <th>Summary</th>\n    <th>Issue Type</th>\n    <th>Status</th>\n    <th>Priority</th>\n    <th>Resolution</th>\n    <th>Assignee</th>\n    <th>Reporter</th>\n    <th>created</th>\n    <th>updated</th>\n    <th>resolved</th>\n    <th>labels</th>\n    <th>Is Urgent Fix</th>\n    <th>Severity</th>\n <th>Activity</th>\n   </tr>";
		const string body = "<tr>\n    <td><a href='https://jira.dianrong.com/browse/{0}'> {0}</a></td>\n    <td>{1}</td>\n    <td>{2}</td>\n    <td>{3}</td>\n    <td>{4}</td>\n    <td>{5}</td>\n    <td>{6}</td>\n    <td>{7}</td>\n    <td>{8}</td>\n    <td>{9}</td>\n    <td>{10}</td>\n    <td>{11}</td>\n    <td>{12}</td>\n    <td>{13}</td>\n <td>{14}</td>\n  </tr> ";
		const string tail = "</table>";

		/// <summary>
		/// Build to generate xls
		/// </summary>
		/// <param name="items">Items.</param>
		public static void Build (List<Item> items)
		{
			var list = items.OrderByDescending (item => item.Created);
			StringBuilder sb = new StringBuilder (head);
			foreach (var item in list) {
				sb.AppendFormat (body,
					item.Key,
					item.Summary,
					item.Type,
					item.Status,
					item.Priority,
					item.Resolution,
					item.Assignee,
					item.Reporter,
					item.Created,
					item.Updated,
					item.Resolved,
					item.Labels,
					item.IsUrgentFix,
					item.Severity,
					item.Activity
				);
			}

			sb.Append (tail);

			var date = DateTime.Now.ToString ("yyyy-MM-dd");
			var reportName = string.Format ("report-{0}-total-{1}-items.xls", date, items.Count);

			using (StreamWriter sw = new StreamWriter (reportName,false,Encoding.UTF8)) {
				sw.Write (sb.ToString ());
				sw.Dispose ();
			}
		}
	}
}

