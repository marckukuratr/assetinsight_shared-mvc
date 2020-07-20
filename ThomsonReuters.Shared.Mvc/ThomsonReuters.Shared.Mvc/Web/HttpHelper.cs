using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web
{
	public static class HttpHelper
	{
		private static string _HostName;
		private static HttpApplication _HttpApplicationInstance;


		private static string HostName
		{
			get
			{
				if (_HostName != null)
				{
					_HostName = CommonUtilities.GetHostName();
				}
				return _HostName;
			}
		}

		public static HttpApplication HttpApplicationInstance
		{
			get
			{
				if (_HttpApplicationInstance == null)
				{
					try
					{
						_HttpApplicationInstance = HttpContext.Current.ApplicationInstance;
					}
					catch { }
				}
				return _HttpApplicationInstance;
			}
			set
			{
				_HttpApplicationInstance = value;
			}
		}


		public static HttpContext GetMockHttpContext(string url)
		{
			var uri = new Uri(url);
			var urlLeft = uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped);
			var query = uri.GetComponents(UriComponents.Query, UriFormat.Unescaped);

			var req = new HttpRequest(null, urlLeft, query);
			var res = new HttpResponse(TextWriter.Null);
			var ret = new HttpContext(req, res);

			return ret;
		}

		public static HttpContextBase GetMockHttpContextBase(string url)
		{
			var httpContext = GetMockHttpContext(url);

			HttpContextWrapper ret = new HttpContextWrapper(httpContext);
			return ret;
		}

		public static RouteData GetUrlRouteData(string url)
		{
			var httpContextBase = GetMockHttpContextBase(url);
			return GetUrlRouteData(httpContextBase);
		}

		public static RequestContext GetMockRequestContext(string url)
		{
			var httpContextBase = GetMockHttpContextBase(url);
			var routeData = GetUrlRouteData(httpContextBase);

			RequestContext ret = new RequestContext(httpContextBase, routeData);
			return ret;
		}


		public static string GetDangerousInputText(Exception ex, bool friendlyMessage = false)
		{
			var ret = default(string);

			if (ex is HttpRequestValidationException)
			{
				ret = StringUtilities.StringBetween(ex.Message, "=\"", "\")");
			}

			if (friendlyMessage)
			{
				ret = string.Format("Input data '{0}'contains special HTML/Script characters like <, >, <!, &# etc., which are not allowed in normal input fields.", ret);
			}

			return ret;
		}


		public static Uri BaseUri()
		{
			var url = HttpContext.Current.Request.Url;

			if (url.IsLoopback ||
				StringComparer.InvariantCultureIgnoreCase.Equals(url.Host, HostName))
			{
				url = new Uri(url.GetLeftPart(UriPartial.Authority));
				return url;
			}

			var caps = SharedWebConfigHelper.HostingCaps;
			var scheme = (caps & WebApplicationHostingCaps.Https) == WebApplicationHostingCaps.Https ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
			var hasPortRedir = (caps & WebApplicationHostingCaps.PortRedirection) == WebApplicationHostingCaps.PortRedirection;
			var urlBuilder = hasPortRedir ? new UriBuilder(scheme, url.Host) : new UriBuilder(scheme, url.Host, url.Port);

			return urlBuilder.Uri;
		}

		/// <summary>
		/// Base url without ending '/'
		/// </summary>
		public static string BaseUrl()
		{
			return BaseUri().AbsoluteUri.TrimEnd('/');
		}

		public static string ToAbsoluteUrl(string relativeOrAbsoluteUrl)
		{
			var uri = new Uri(relativeOrAbsoluteUrl, UriKind.RelativeOrAbsolute);

			if (uri.IsAbsoluteUri)
			{
				relativeOrAbsoluteUrl = uri.PathAndQuery;
			}

			var baseUrl = BaseUri();
			var combinedUri = default(Uri);

			if (Uri.TryCreate(baseUrl, relativeOrAbsoluteUrl, out combinedUri))
			{
				return combinedUri.AbsoluteUri;
			}

			throw new Exception(string.Format("Could not create absolute url for '{0}' using base url '{0}'", relativeOrAbsoluteUrl, baseUrl));
		}


		private static RouteData GetUrlRouteData(HttpContextBase httpContextBase)
		{
			RouteData ret = RouteTable.Routes.GetRouteData(httpContextBase);
			return ret;
		}

		public static bool IsUnSupportedIEBrowser(HttpRequestBase request)
		{
			var ret = false;

			if (request.Browser != null)
			{
				var isBrowserIE = StringComparer.InvariantCultureIgnoreCase.Equals(request.Browser.Browser, "InternetExplorer");
				var isLesserVersion = request.Browser.MajorVersion < 11;

				ret = isBrowserIE && isLesserVersion;
			}

			return ret;
		}


		public static string GetCookiesInfoAsString(HttpCookieCollection requestCookies, HttpCookieCollection responseCookies)
		{
			var sb = new System.Text.StringBuilder();

			sb.AppendLine("###### " + CommonUtilities.GetHostName() + " ######");

			sb.AppendLine("*** Request Cookies ***");

			BuildCookiesInforString(requestCookies, sb);

			sb.AppendLine("*** Response Cookies ***");

			BuildCookiesInforString(responseCookies, sb);

			var ret = sb.ToString();
			return ret;
		}

		private static void BuildCookiesInforString(HttpCookieCollection cookies, StringBuilder sb)
		{
			var cookiesArray = new HttpCookie[cookies.Count];

			cookies.CopyTo(cookiesArray, 0);

			foreach (var cookie in cookiesArray)
			{
				sb.AppendLine("---COOKIE---");
				sb.AppendLine("Name: " + cookie.Name);
				sb.AppendLine("Value: " + cookie.Value);
				sb.AppendLine("Path: " + cookie.Path);
				sb.AppendLine("Domain: " + cookie.Domain);
			}
		}
	}
}
