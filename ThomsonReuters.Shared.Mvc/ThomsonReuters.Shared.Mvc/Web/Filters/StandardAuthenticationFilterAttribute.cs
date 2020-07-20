using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace ThomsonReuters.Shared.Web.Filters
{
	public class StandardAuthenticationFilterAttribute : ActionFilterAttribute, IAuthenticationFilter
	{
		public virtual void OnAuthentication(AuthenticationContext filterContext)
		{
		}

		public virtual void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
		{
		}
	}
}
