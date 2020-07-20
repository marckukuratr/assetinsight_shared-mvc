using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThomsonReuters.Shared.Model
{
	public interface IActionLog
	{
		int ActionLogId { get; set; }
		string SessionId { get; set; }
		string User { get; set; }
		string IP { get; set; }
		string Area { get; set; }
		string Controller { get; set; }
		string Action { get; set; }
		string Url { get; set; }
		string UrlReferrer { get; set; }
		string HttpMethod { get; set; }
		string EventType { get; set; }
		string Data { get; set; }
		DateTime DateTime { get; set; }
	}
}
