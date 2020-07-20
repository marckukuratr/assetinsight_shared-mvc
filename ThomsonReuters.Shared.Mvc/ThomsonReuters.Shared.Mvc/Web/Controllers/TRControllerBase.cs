using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ThomsonReuters.Data.Repository;
using ThomsonReuters.Logging;
using ThomsonReuters.Net;
using ThomsonReuters.Shared.Model;
using ThomsonReuters.Shared.ViewModels;
using ThomsonReuters.Shared.Web.Authentication;
using ThomsonReuters.Shared.Web.Filters;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web.Controllers
{
	public enum ViewEngineType
	{
		Aspx,
		Razor
	}

	[StandardAuthenticationFilter]
	[Authorize]
	[Stopwatch]
	[MinifyHtmlActionFilter]
	public abstract class TRControllerBase : Controller, IErrorViewModelProvider
	{
		public const string LOG_SIGN_IN = "SignIn";
		public const string CURRENT_SAFE_USER_KEY = "SAFEAuthenticatedUserID";
		public const string ADMIN_LOGIN_KEY = "IsAdminLoggedIn";
		public const string ADMIN_LAST_TIMESTAMP = "AdminLastTimeStamp";
		public const string IMPERSONATE_KEY = "Impersonate";

		private IFormsAuthenticationService _formsService;
		private IMembershipService _membershipService;
		private TimeProfiler _profiler;
		private AuthorizationManager _authorizationManager;

		public IFormsAuthenticationService FormsService
		{
			get
			{
				return _formsService;
			}
			set // for UnitTesting only
			{
				_formsService = value;
			}
		}

		public IMembershipService MembershipService
		{
			get
			{
				return _membershipService;
			}
			set // for UnitTesting only
			{
				_membershipService = value;
			}
		}

		public AuthorizationManager AuthorizationManager
		{
			get
			{
				return _authorizationManager;
			}
		}

		/// <summary>
		/// Upon Authorization, SSO should postback to this URL. By default, AI Apps should use /Account/Auth
		/// </summary>
		protected string GetAuthorizationPostbackUri()
		{
			////return $"{requestContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority)}/Account/Auth";
			return $"{ConfigurationUtilities.GetString("Shared.SiteBaseUrl") + ConfigurationUtilities.GetString("Shared.AuthorizationAction")}";
		}

		protected TimeProfiler Profiler
		{
			get
			{
				if (_profiler == null)
				{
					_profiler = new TimeProfiler();
				}
				return _profiler;
			}
		}

		public virtual IActionLogFilterSettings ActionLogSettings { get; set; }

		public static string HostMachineName
		{
			get
			{
				var ret = Dns.GetHostEntry("localhost").HostName;

				ret = System.Net.WebUtility.HtmlEncode(ret);

				return ret;
			}
		}

		public string CurrentSAFEAuthenticatedUserID
		{
			get { return GetSAFEAuthenticatedUserID(); }
			set { System.Web.HttpContext.Current.Session[CURRENT_SAFE_USER_KEY] = value; }
		}

		public string CurrentUser { get { return User.Identity.Name; } }

		protected string ControllerName
		{
			get { return this.RouteGetControllerName(); }
		}

		protected string Area
		{
			get { return this.RouteGetAreaName(); }
		}

		protected string ActionName
		{
			get { return this.RouteGetActionName(); }
		}

		protected virtual string ApplicationCode
		{
			get { return string.Empty; }
		}

		protected ModelError[] ModelErrors
		{
			get
			{
				var modelStates = ModelState.Values.Where(t => (t.Errors != null && t.Errors.Count > 0)).ToArray();
				var errs = modelStates.Select(t => t.Errors).SelectMany(t => t).ToArray();
				return errs;
			}
		}


		// MVC stages of execution

		protected override void Initialize(RequestContext requestContext)
		{
			// changed auth services to always instantiate by default

			_formsService = new FormsAuthenticationService();
			_membershipService = new AccountMembershipService();
			_authorizationManager = new AuthorizationManager(GetAuthorizationPostbackUri());

			base.Initialize(requestContext);
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!string.IsNullOrWhiteSpace(CurrentUser))
			{
				SetUser(CurrentUser);
			}

			base.OnActionExecuting(filterContext);
		}


		[OutputCache(NoStore = true, Location = System.Web.UI.OutputCacheLocation.None)]
		public ActionResult HttpHeaders(int? id)
		{
			var sb = new System.Text.StringBuilder();
			var headers = Request.Headers.AllKeys.Select(t => new KeyValuePair<string, string>(t, Request.Headers[t])).ToList();

			headers.ForEach(t => sb.AppendLine(string.Format("{0}: {1}", t.Key, t.Value)));

			var data = sb.ToString();

			return Content(sb.ToString(), System.Net.Mime.MediaTypeNames.Text.Plain);
		}

		public virtual ActionResult Processes()
		{
			var sb = new StringBuilder();

			sb.AppendLine("<style type='text/css'>");
			sb.AppendLine("table {");
			sb.AppendLine("border-collapse: collapse;");
			sb.AppendLine("}");
			sb.AppendLine("td {");
			sb.AppendLine("border-collapse: collapse;");
			sb.AppendLine("border: 1px solid black;");
			sb.AppendLine("}");
			sb.AppendLine("</style>");
			sb.AppendFormat("<div>Host: {0}</div>", HostMachineName);

			sb.AppendLine("<table>");
			sb.AppendLine();

			var processes = System.Diagnostics.Process.GetProcesses().OrderBy(t => t.ProcessName, StringComparer.InvariantCultureIgnoreCase).ToArray();

			foreach (var process in processes)
			{
				var memInfo = string.Format("PrivateMemorySize: {0:0,0}", process.PrivateMemorySize64);
				var line = string.Format("<tr><td>{0}</td><td>{1}</td></tr>", process.ProcessName, memInfo);

				sb.AppendLine(line);
			}

			sb.AppendLine("</table>");
			//sb.Replace(Environment.NewLine, "<br/>");

			var ret = sb.ToString();
			return Content(ret, "text/html");
		}


		// Admin Features

		protected virtual void CoreAdmin_SetLogin(bool isLogin, bool doLogging = true)
		{
			Session[ADMIN_LOGIN_KEY] = isLogin;

			if (isLogin)
			{
				Session[ADMIN_LAST_TIMESTAMP] = DateTime.Now;
			}
			else
			{
				Session[ADMIN_LAST_TIMESTAMP] = null;
			}

			if (doLogging)
			{
				CoreAdmin_LogLoginLogout(isLogin);
			}
		}

		protected virtual void CoreAdmin_LogLoginLogout(bool isLogin)
		{
			var log = GetBasicActionLog();

			if (isLogin)
			{
				log.EventType = "AdminLogin";
			}
			else
			{
				log.EventType = "AdminLogout";
				log.Data = string.Format("{0} signed out", CurrentSAFEAuthenticatedUserID);
			}

			ActionLogSettings.Repository.AddToActionLogs(log, true);
		}

		public static bool CoreAdmin_IsLoggedIn(HttpContextBase context)
		{
			var ret = false;
			if (context.Session[ADMIN_LOGIN_KEY] != null)
			{
				try
				{
					ret = (bool)context.Session[ADMIN_LOGIN_KEY];
				}
				catch (Exception ex)
				{
					TREventLog.Log(ex.Message, LogType.Error);
				}
			}
			return ret;
		}

		public static bool CoreAdmin_IsLoggedIn(HttpContext context = null)
		{
			if (context == null)
			{
				context = System.Web.HttpContext.Current;
			}

			var ret = false;

			if (context.Session[ADMIN_LOGIN_KEY] != null)
			{
				try
				{
					ret = (bool)context.Session[ADMIN_LOGIN_KEY];
				}
				catch (Exception ex)
				{
					TREventLog.Log(ex.Message, LogType.Error);
				}
			}
			return ret;
		}

		public static DateTime? CoreAdmin_GetLastTimeStamp(HttpContextBase context)
		{
			DateTime? ret = null;
			try
			{
				if (context.Session[ADMIN_LAST_TIMESTAMP] != null)
				{
					ret = (DateTime)context.Session[ADMIN_LAST_TIMESTAMP];
				}
			}
			catch(Exception ex)
			{
				TREventLog.Log(ex.Message, LogType.Error);
			}
			return ret;
		}

		public static DateTime? CoreAdmin_GetLastTimeStamp(HttpContext context = null)
		{
			if (context == null)
			{
				context = System.Web.HttpContext.Current;
			}

			DateTime? ret = null;
			try
			{
				if (context.Session[ADMIN_LAST_TIMESTAMP] != null)
				{
					ret = (DateTime)context.Session[ADMIN_LAST_TIMESTAMP];
				}
			}
			catch (Exception ex)
			{
				TREventLog.Log(ex.Message, LogType.Error);
			}
			return ret;
		}

		public static void CoreAdmin_SetLastTimeStamp(HttpContextBase context)
		{
			context.Session[ADMIN_LAST_TIMESTAMP] = DateTime.Now;
		}

		public static void CoreAdmin_SetLastTimeStamp(HttpContext context = null)
		{
			if (context == null)
			{
				context = System.Web.HttpContext.Current;
			}

			context.Session[ADMIN_LAST_TIMESTAMP] = DateTime.Now;
		}

		protected void CoreAdmin_LogImpersonate(string newUser, string logEvent = IMPERSONATE_KEY)
		{
			var log = GetBasicActionLog();
			log.EventType = logEvent;
			log.Data = newUser;

			ActionLogSettings.Repository.AddToActionLogs(log, true);
		}


		// User handling

		protected abstract void SetUser(string userID);

		protected void LogSignIn(string userID)
		{
			var log = GetBasicActionLog();
			log.User = userID;
			log.EventType = LOG_SIGN_IN;
			log.Data = userID;

			ActionLogSettings.Repository.AddToActionLogs(log, true);
		}

		private string GetSAFEAuthenticatedUserID()
		{
			string ret = System.Web.HttpContext.Current.Session[CURRENT_SAFE_USER_KEY] as string;

			if (string.IsNullOrWhiteSpace(ret))
			{
				var log = ActionLogSettings.Repository.GetActionLogs(Session.SessionID, false).FirstOrDefault(t => t.EventType == LOG_SIGN_IN);

				if (log != null)
				{
					ret = log.User;
				}
			}

			return ret;
		}


		// State helpers

		protected void CleanModelState()
		{
			ModelState.Clear();
		}

		/// <summary>
		/// <para>Sometimes state of collection based data in model will not be reflected properly on postbacks.</para>
		/// <para>Use this function to manually clean those stale data by specifying the collection property name.</para>
		/// </summary>
		protected void CleanModelState(string[] keysContains)
		{
			foreach (var keyContains in keysContains)
			{
				CleanModelState(keyContains);
			}
		}

		/// <summary>
		/// <para>Sometimes state of collection based data in model will not be reflected properly on postbacks.</para>
		/// <para>Use this function to manually clean those stale data by specifying the collection property name.</para>
		/// </summary>
		protected void CleanModelState(string keyContains)
		{
			string[] keys = ModelState.Keys.ToArray();
			foreach (var key in keys)
			{
				if (key.Contains(keyContains))
				{
					ModelState.Remove(key);
				}
			}
		}


		// Error handling related

		public abstract IErrorViewModel NewErrorViewModel();

		public virtual ActionResult Error(string msg = MessageContants.ERR_MSG_ERROR)
		{
			return GetErrorResult(CommonViewNames.VIEW_ERROR, msg);
		}

		public virtual ActionResult NotFound(string msg = MessageContants.ERR_MSG_NOTFOUND)
		{
			return GetErrorResult(CommonViewNames.VIEW_NOTFOUND, msg);
		}

		public virtual ActionResult Unauthorized(string msg = MessageContants.ERR_MSG_NOTFOUND)
		{
			return GetErrorResult(CommonViewNames.VIEW_UNAUTHORIZED, msg);
		}

		/// <summary>
		/// <para>When overriden in derived class, specifies the route to redirect for default error page</para>
		/// <para>Default Route: /Error/Error</para>
		/// </summary>
		public virtual RedirectToRouteResult ErrorRedirectRouteResult()
		{
			var ret = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "Error" }));
			return ret;
		}

		private ActionResult GetErrorResult(string viewName, string message)
		{
			var model = NewErrorViewModel();

			model.Url = HttpContext.Request.Url;
			model.Message = message;

			return View(viewName, model);
		}


		// Action Logging

		protected virtual IActionLog GetBasicActionLog()
		{
			var log = ActionLogSettings.NewActionLog();
			log.SessionId = HttpContext.Session.SessionID;
			log.User = CurrentUser;
			log.IP = HttpContext.Request.UserHostAddress;
			log.Area = Area;
			log.Controller = ControllerName;
			log.Action = ActionName;
			log.Url = HttpHelper.ToAbsoluteUrl(HttpContext.Request.Url.ToString());
			log.UrlReferrer = HttpContext.Request.UrlReferrer == null ? null : HttpContext.Request.UrlReferrer.ToString();
			log.HttpMethod = HttpContext.Request.HttpMethod != null ?
					(HttpContext.Request.HttpMethod.Length > 6 ? HttpContext.Request.HttpMethod.Substring(0, 6) : HttpContext.Request.HttpMethod) : null;
			log.DateTime = DateTime.UtcNow;
			return log;
		}


		// Render Razor/Aspx to string and output stream

		protected string RenderRazorToString<TModel>(ExceptionContext filterContext, ViewEngineType viewType, string viewName, TModel model)
		{
			var ret = string.Empty;

			ViewData.Model = model;

			using (var writer = new StringWriter())
			{
				switch (viewType)
				{
					case ViewEngineType.Razor:
						var viewEngineResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
						var viewContext = new ViewContext(ControllerContext, viewEngineResult.View, ViewData, TempData, writer);
						viewEngineResult.View.Render(viewContext, writer);
						viewEngineResult.ViewEngine.ReleaseView(ControllerContext, viewEngineResult.View);
						ret = writer.GetStringBuilder().ToString();
						break;
					case ViewEngineType.Aspx:
					default:
						var view = new WebFormView(ControllerContext, viewName);
						var vdd = new ViewDataDictionary<TModel>(model);
						var viewCxt = new ViewContext(ControllerContext, view, vdd, new TempDataDictionary(), writer);
						viewCxt.View.Render(viewCxt, writer);
						ret = writer.ToString();
						break;
				}
			}

			return ret;
		}

		protected void RenderRazorToResponse<TModel>(ExceptionContext filterContext, ViewEngineType viewType, string viewName, TModel model)
		{
			var strContent = RenderRazorToString(filterContext, viewType, viewName, model);

			filterContext.HttpContext.Response.Write(strContent);
			filterContext.HttpContext.Response.Flush();
			filterContext.HttpContext.Response.End();
		}
	}
}
