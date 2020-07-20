using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ThomsonReuters.Shared.ViewModels;

namespace ThomsonReuters.Shared.Web.Controllers
{
	public class ControllerHelper
	{
		public static List<EntityViewModel> GetEntityItemsFromController(HttpContextBase context, string controllerName, string values)
		{
			var entityItems = context.RunControllerFunction<List<EntityViewModel>>(controllerName, "GetEntityViewModelsByValues", values);

			entityItems = entityItems == null ? new List<EntityViewModel>() : entityItems;

			return entityItems;
		}

		public static IController GetController(string controllerName, RequestContext requestContext = null)
		{
			if (requestContext == null)
			{
				var context = new HttpContextWrapper(HttpContext.Current);

				requestContext = context.Request.RequestContext;
			}

			var cBuilder = new ControllerBuilder();

			var ret = cBuilder.GetControllerFactory().CreateController(requestContext, controllerName);
			return ret;
		}

		public static string GetControllerAlias(string controllerName, RequestContext requestContext = null)
		{
			var controller = GetController(controllerName, requestContext);

			var type = controller.GetType();
			var attribs = type.GetCustomAttributes<ControllerAliasAttribute>();

			if (attribs.HasAny())
			{
				var alias = attribs.First();

				controllerName = alias.Alias;
			}

			return controllerName;
		}
	}

	public static class ControllerHelperExtensions
	{
		public static void RunControllerMethod(this HttpContextBase context, string controllerName, string methodName, params object[] args)
		{
			var requestContext = CreateRequestContext(context, controllerName);

			var cb = new ControllerBuilder();
			var controller = cb.GetControllerFactory().CreateController(requestContext, controllerName);

			Type controllerType = controller.GetType();

			controllerType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, controller, args);
		}

		public static T RunControllerFunction<T>(this HttpContextBase context, string controllerName, string functionName, params object[] args)
		{
			var requestContext = CreateRequestContext(context, controllerName);

			var cb = new ControllerBuilder();
			var controller = cb.GetControllerFactory().CreateController(requestContext, controllerName);

			Type controllerType = controller.GetType();
			var ret = controllerType.InvokeMember(functionName, BindingFlags.InvokeMethod, null, controller, args);

			return (T)ret;
		}


		private static RequestContext CreateRequestContext(HttpContextBase context, string controller)
		{
			var uri = context.Request.Url;
			var url = string.Format("{0}/{1}", uri.GetLeftPart(UriPartial.Authority).TrimEnd('/'), controller);
			var ret = HttpHelper.GetMockRequestContext(url);
			return ret;
		}
	}
}
