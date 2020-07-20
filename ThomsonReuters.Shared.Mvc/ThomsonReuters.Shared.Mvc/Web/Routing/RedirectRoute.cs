using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using ThomsonReuters.Net;
using ThomsonReuters.Utilities;

// http://haacked.com/archive/2008/12/15/redirect-routes-and-other-fun-with-routing-and-lambdas.aspx

namespace ThomsonReuters.Shared.Web.Routing
{
	// TODO: Temporarily disabled due to Veracode flaws

	//internal class RedirectRouteHttpHandler : IHttpHandler
	//{
	//	public RedirectRouteHttpHandler(string targetUrl, bool isReusable, bool isPermanent)
	//	{
	//		this.IsReusable = isReusable;
	//		this.IsPermanent = IsPermanent;
	//		this.TargetUrl = CleansedUrl(targetUrl);
	//	}

	//	public bool IsReusable { get; private set; }
	//	private bool IsPermanent { get; set; }
	//	private string TargetUrl { get; set; }

	//	public void ProcessRequest(HttpContext context)
	//	{
	//		if (IsPermanent)
	//		{
	//			var httpStatInfo = HttpStatusInfo.GetByStatusCode(System.Net.HttpStatusCode.MovedPermanently);

	//			context.Response.StatusCode = (int)System.Net.HttpStatusCode.MovedPermanently;
	//			context.Response.AddHeader("Location", this.TargetUrl);
	//		}
	//		else
	//		{
	//			context.Response.Redirect(this.TargetUrl, false);
	//		}
	//	}

	//	private string CleansedUrl(string url)
	//	{
	//		var uri = new Uri(url, UriKind.RelativeOrAbsolute);
	//		var ret = uri.OriginalString;

	//		if (uri.IsAbsoluteUri)
	//		{
	//			ret = uri.PathAndQuery;
	//		}

	//		return ret;
	//	}
	//}

	//public class RedirectRouteHandler : IRouteHandler
	//{
	//	public RedirectRouteHandler(string targetUrl, bool isPermanent)
	//	{
	//		this.TargetUrl = targetUrl;
	//		this.IsPermanent = isPermanent;
	//	}

	//	public string TargetUrl { get; private set; }
	//	public bool IsPermanent { get; private set; }

	//	public IHttpHandler GetHttpHandler(RequestContext requestContext)
	//	{
	//		var url = TargetUrl;
	//		var isAbsoluteRedirect = IsAbsoluteRedirect(TargetUrl);

	//		if (!isAbsoluteRedirect)
	//		{
	//			url = url.TrimStart("~/".ToCharArray());

	//			Route route = new Route(url, null);
	//			var vpd = route.GetVirtualPath(requestContext, requestContext.RouteData.Values);

	//			if (vpd != null)
	//			{
	//				var pathBase = string.Format("/{0}", vpd.VirtualPath.TrimEnd('/'));
	//				var query = UrlUtilities.SafeGetQueryString(requestContext.HttpContext.Request.QueryString, prefixQuestion: true);

	//				url = string.Format("{0}{1}", pathBase, query);
	//			}
	//		}

	//		var ret = new RedirectRouteHttpHandler(url, true, IsPermanent);
	//		return ret;
	//	}

	//	public static bool IsAbsoluteRedirect(string targetUrl)
	//	{
	//		targetUrl = targetUrl.ToLower();

	//		var ret =
	//			targetUrl.StartsWith(Uri.UriSchemeHttp) ||
	//			targetUrl.StartsWith(Uri.UriSchemeHttps) ||
	//			targetUrl.StartsWith(Uri.UriSchemeFtp) ||
	//			targetUrl.StartsWith(Uri.UriSchemeFile);

	//		return ret;
	//	}
	//}

	//public static class RedirectRouteExtensions
	//{
	//	public static Route MapRedirect(this RouteCollection routes, string url, string targetUrl)
	//	{
	//		return MapRedirect(routes, url, targetUrl, null, null);
	//	}

	//	public static Route MapRedirect(this RouteCollection routes, string url, string targetUrl, object defaults)
	//	{
	//		return MapRedirect(routes, url, targetUrl, defaults, null);
	//	}

	//	public static Route MapRedirect(this RouteCollection routes, string url, string targetUrl, object defaults, object constraints)
	//	{
	//		var isPermanant = true; // RedirectRouteHandler.IsAbsoluteRedirect(targetUrl);

	//		var rvdDefaults = CreateRouteValueDictionary(defaults);
	//		var rvdConstraints = CreateRouteValueDictionary(constraints);

	//		Route route = new Route(url, rvdDefaults, rvdConstraints, new RedirectRouteHandler(targetUrl, isPermanant));
	//		routes.Add(route);
	//		return route;
	//	}


	//	private static RouteValueDictionary CreateRouteValueDictionary(object values)
	//	{
	//		RouteValueDictionary ret = null;

	//		if (values != null)
	//		{
	//			IDictionary<string, object> dictionary = values as IDictionary<string, object>;

	//			if (dictionary != null)
	//			{
	//				ret = new RouteValueDictionary(dictionary);
	//			}
	//			else
	//			{
	//				ret = new RouteValueDictionary(values);
	//			}
	//		}

	//		return ret;
	//	}
	//}
}
