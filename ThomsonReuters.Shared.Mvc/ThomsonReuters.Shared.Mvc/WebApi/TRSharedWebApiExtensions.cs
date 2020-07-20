using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace ThomsonReuters.Shared.WebApi
{
	public static class TRSharedWebApiExtensions
	{
		public static bool IsAttributeDefined<TAttribute>(this HttpRequestMessage request, bool inherit = false)
			where TAttribute : Attribute
		{
			var routeData = WebApiUtils.GetCoreRouteData(request);
			var descrAction = WebApiUtils.GetRouteHttpActionDescriptor(routeData);

			var ret = false;

			if (descrAction != null)
			{
				ret = descrAction.IsAttributeDefined<TAttribute>();

				if (!ret)
				{
					ret = descrAction.ControllerDescriptor.IsAttributeDefined<TAttribute>();
				}
			}

			return ret;
		}

		public static bool IsAttributeDefined<TAttribute>(this HttpControllerDescriptor controllerDesc, bool inherit = false)
			where TAttribute : Attribute
		{
			var attribs = controllerDesc.GetCustomAttributes<TAttribute>(inherit);
			var ret = attribs.HasAny();
			return ret;
		}

		public static bool IsAttributeDefined<TAttribute>(this HttpActionDescriptor actionDesc, bool inherit = false)
			where TAttribute : Attribute
		{
			var attribs = actionDesc.GetCustomAttributes<TAttribute>(inherit);
			var ret = attribs.HasAny();
			return ret;
		}
	}
}
