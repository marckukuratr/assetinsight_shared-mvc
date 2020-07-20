using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;

namespace ThomsonReuters.Shared.Web.Filters
{
	public class AuthenticateActionLogFilterAttribute : ActionLogFilterAttribute
	{
		public AuthenticateActionLogFilterAttribute(Type settingsType)
			: base(settingsType)
		{
		}

		protected override string BuildData(ActionExecutingContext filterContext)
		{
			string ret = null;

			StringBuilder sb = new StringBuilder();
			XmlWriter wr = XmlTextWriter.Create(sb, WriterSettings);

			wr.WriteStartElement("Data");

			// Http headers
			base.WriteData_HttpHeaders(filterContext, wr, true);
			// Form data
			base.WriteData_Form(filterContext, wr, true);

			wr.WriteEndElement();
			wr.Flush();

			ret = sb.ToString();
			return ret;
		}
	}
}
