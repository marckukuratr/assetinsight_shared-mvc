using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ThomsonReuters.Shared.Web.Filters
{
	public class OptionalAuthorizeAttribute : AuthorizeAttribute
	{
		private readonly bool _authorize;

		public OptionalAuthorizeAttribute()
		{
			_authorize = true;
		}

		public OptionalAuthorizeAttribute(bool authorize)
		{
			_authorize = authorize;
		}

		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			bool ret = true;
			if (_authorize)
			{
				ret = base.AuthorizeCore(httpContext);
			}
			return ret;
		}
	}
}
