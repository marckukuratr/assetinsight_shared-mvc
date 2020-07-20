using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThomsonReuters.Shared.Model
{
	public class DefaultActionLog : IActionLog
	{
		public int ActionLogId { get; set; }
		public string SessionId { get; set; }
		public string User { get; set; }
		public string IP { get; set; }
		public string Area { get; set; }
		public string Controller { get; set; }
		public string Action { get; set; }
		public string Url { get; set; }
		public string UrlReferrer { get; set; }
		public string HttpMethod { get; set; }
		public string EventType { get; set; }
		public string Data { get; set; }
		public DateTime DateTime { get; set; }
	}
}
