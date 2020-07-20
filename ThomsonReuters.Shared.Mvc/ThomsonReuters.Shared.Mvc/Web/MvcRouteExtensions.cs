using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web
{
	public static class MvcRouteExtensions
	{
		public static string RouteGetControllerName(this RouteData route)
		{
			var ret = (string)route.Values["controller"];
			return ret;
		}

		public static string RouteGetActionName(this RouteData route)
		{
			var ret = (string)route.Values["action"];
			return ret;
		}

		public static string RouteGetAreaName(this RouteData route)
		{
			string ret = null;

			if (route.DataTokens.ContainsKey("area"))
			{
				ret = (string)route.DataTokens["area"];
			}

			return ret;
		}

		public static bool RouteHasRouteParam(this RouteData route, string key)
		{
			var ret = route.Values.ContainsKey(key);
			return ret;
		}

		public static string RouteGetIdParam(this RouteData route)
		{
			var ret = default(string);

			if (route.Values.ContainsKey("id"))
			{
				var obj = route.Values["id"];

				if (obj != null)
				{
					ret = obj.ToString();
				}
			}

			return ret;
		}


		public static string RouteGetControllerName(this Controller controller)
		{
			return RouteGetControllerName(controller.RouteData);
		}

		public static string RouteGetActionName(this Controller controller)
		{
			return RouteGetActionName(controller.RouteData);
		}

		public static string RouteGetAreaName(this Controller controller)
		{
			return RouteGetAreaName(controller.RouteData);
		}

		public static bool RouteHasRouteParam(this Controller controller, string key)
		{
			return RouteHasRouteParam(controller.RouteData, key);
		}

		public static string RouteGetIdParam(this Controller controller)
		{
			return RouteGetIdParam(controller.RouteData);
		}


		public static string RouteGetControllerName(this ViewContext viewContext)
		{
			return RouteGetControllerName(viewContext.RouteData);
		}

		public static string RouteGetActionName(this ViewContext viewContext)
		{
			return RouteGetActionName(viewContext.RouteData);
		}

		public static string RouteGetAreaName(this ViewContext viewContext)
		{
			return RouteGetAreaName(viewContext.RouteData);
		}

		public static bool RouteHasRouteParam(this ViewContext viewContext, string key)
		{
			return RouteHasRouteParam(viewContext.RouteData, key);
		}

		public static string RouteGetIdParam(this ViewContext viewContext)
		{
			return RouteGetIdParam(viewContext.RouteData);
		}



		public static bool RouteControllerNameIs(this RouteData route, string name)
		{
			var check = route.RouteGetControllerName();
			var ret = StringUtilities.AreEqualCaseInsensitive(check, name);
			return ret;
		}

		public static bool RouteActionNameIs(this RouteData route, string name)
		{
			var check = route.RouteGetActionName();
			var ret = StringUtilities.AreEqualCaseInsensitive(check, name);
			return ret;
		}

		public static bool RouteAreaNameIs(this RouteData route, string name)
		{
			var check = route.RouteGetAreaName();
			var ret = StringUtilities.AreEqualCaseInsensitive(check, name);
			return ret;
		}


		public static bool RouteControllerNameIs(this Controller controller, string name)
		{
			return RouteControllerNameIs(controller.RouteData, name);
		}

		public static bool RouteActionNameIs(this Controller controller, string name)
		{
			return RouteActionNameIs(controller.RouteData, name);
		}

		public static bool RouteAreaNameIs(this Controller controller, string name)
		{
			return RouteAreaNameIs(controller.RouteData, name);
		}


		public static bool RouteControllerNameIs(this ViewContext viewContext, string name)
		{
			return RouteControllerNameIs(viewContext.RouteData, name);
		}

		public static bool RouteActionNameIs(this ViewContext viewContext, string name)
		{
			return RouteActionNameIs(viewContext.RouteData, name);
		}

		public static bool RouteAreaNameIs(this ViewContext viewContext, string name)
		{
			return RouteAreaNameIs(viewContext.RouteData, name);
		}


		public static string GetLocationData(this RouteData route)
		{
			var r = route.Route as Route;
			var cntrName = route.RouteGetControllerName();
			var cntrAliasName = ThomsonReuters.Shared.Web.Controllers.ControllerHelper.GetControllerAlias(cntrName);

			var url = "UF:";
			var area = "AR:" + route.RouteGetAreaName();
			var controller = "CN:" + cntrName;
			var controllerAlias = "CNA:" + cntrAliasName;
			var action = "AC:" + route.RouteGetActionName();
			var id = "PM:" + (route.RouteHasRouteParam("id") ? (route.Values["id"] as string) : null);

			if (r != null)
			{
				url += r.Url;
			}


			var ret = string.Join(";", new string[] { url, area, controller, controllerAlias, action, id });
			return ret;
		}
	}
}
