using System;
using System.Collections.Generic;

namespace reportgen
{
	/// <summary>
	/// Issue Item.
	/// </summary>
	public class Item
	{
		public string Key;
		public string Title;
		public string Link;
		public string Description;
		public string Type;
		public string Priority;
		public string Components;
		public string Labels;
		public string Severity;
		public string IsUrgentFix;
		public string Status;
		public string Resolution;
		public string Assignee;
		public string Reporter;
		public DateTime? Created;
		public DateTime? Updated;
		public DateTime? Resolved;
		public string Summary;
		public string Activity;
		public List<Comment> Comments = new List<Comment> ();
	};

	public class Comment
	{
		public string Author;
		public DateTime created;
		public string content;
	}
}

