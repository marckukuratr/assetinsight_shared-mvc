using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace ThomsonReuters.Shared.WebApi
{
	public static class WebApiUtils
	{
		public static IHttpRouteData GetCoreRouteData(HttpRequestMessage request)
		{
			var routeData = request.GetRouteData();
			var subRoutes = routeData.GetSubRoutes();

			if (subRoutes.HasAny())
			{
				routeData = subRoutes.LastOrDefault();
			}

			return routeData;
		}


		public static HttpControllerDescriptor GetRouteHttpControllerDescriptor(IHttpRouteData routeData)
		{
			var route = routeData.Route;
			var tmpObj = default(object);

			if (route != null && route.DataTokens != null)
			{
				if (route.DataTokens.TryGetValue("controller", out tmpObj))
				{
					var ret = (HttpControllerDescriptor)tmpObj;
					return ret;
				}
				else
				{
					var actDesc = GetRouteHttpActionDescriptor(routeData);

					if (actDesc != null)
					{
						return actDesc.ControllerDescriptor;
					}
				}
			}

			return null;
		}

		public static HttpActionDescriptor GetRouteHttpActionDescriptor(IHttpRouteData routeData)
		{
			var route = routeData.Route;
			var tmpObj = default(object);

			if (route != null && route.DataTokens != null)
			{
				if (route.DataTokens.TryGetValue("actions", out tmpObj))
				{
					var actDescs = (IEnumerable<HttpActionDescriptor>)tmpObj;

					var ret = actDescs.FirstOrDefault();
					return ret;
				}
			}

			return null;
		}


		public static string GetResponseHeadersInfo(HttpResponseMessage response)
		{
			var ret = string.Empty;

			var resBuilder = new StringBuilder();

			resBuilder.AppendLine(string.Format("{0} {1}", (int)response.StatusCode, response.ReasonPhrase));

			foreach (var h in response.Headers)
			{
				resBuilder.AppendLine(string.Format("{0}: {1}", h.Key, h.Value.FirstOrDefault()));
			}

			if (response.Content != null)
			{
				foreach (var h in response.Content.Headers)
				{
					resBuilder.AppendLine(string.Format("{0}: {1}", h.Key, h.Value.FirstOrDefault()));
				}
			}

			ret = resBuilder.ToString();

			return ret;
		}

	}
}
