using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ThomsonReuters.Shared.Web.Controllers;

namespace ThomsonReuters.Shared.Web.Filters
{
	public abstract class AdminLoginAuthorizeAttribute : AuthorizeAttribute
	{
		public const string DEF_LOGIN_REDIRECT = "/Admin/Login";
		public const string DEF_LOGOUT_REDIRECT = "/Admin/Logout";

		protected readonly string LoginRedirect = DEF_LOGIN_REDIRECT;
		protected readonly string LogoutinRedirect = DEF_LOGOUT_REDIRECT;


		public AdminLoginAuthorizeAttribute()
		{
		}

		public AdminLoginAuthorizeAttribute(string loginRedirect, string logoutinRedirect)
		{
			LoginRedirect = loginRedirect;
			LogoutinRedirect = logoutinRedirect;
		}


		protected abstract TimeSpan GetExpireTimeout();

		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			var ret = TRControllerBase.CoreAdmin_IsLoggedIn(httpContext);

			if (ret)
			{
				ret = false;

				var lastTS = TRControllerBase.CoreAdmin_GetLastTimeStamp(httpContext);

				if (lastTS.HasValue)
				{
					var diff = DateTime.Now - lastTS.Value;
					var to = GetExpireTimeout();

					ret = diff < to;
				}
			}

			if (ret)
			{
				TRControllerBase.CoreAdmin_SetLastTimeStamp(httpContext);
			}

			return ret;
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			var isLoggedIn = TRControllerBase.CoreAdmin_IsLoggedIn(filterContext.HttpContext);

			if (!isLoggedIn) // Unauthorized due to non login
			{
				filterContext.Result = new RedirectResult(LoginRedirect);
			}
			else // Unauthorized due expiration (assume)
			{
				filterContext.Result = new RedirectResult(LogoutinRedirect);
			}
		}
	}
}
