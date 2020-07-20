using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web
{
	public class HttpHeadersCleanupModule : IHttpModule
	{
		public HttpHeadersCleanupModule()
		{
		}

		private static string[] _removeResponseHeaders;

		public static string[] RemoveResponseHeaders
		{
			get
			{
				if (_removeResponseHeaders == null)
				{
					List<string> ret = new List<string>();
					var s = SharedWebConfigHelper.RemoveResponseHeaders;

					if (!string.IsNullOrWhiteSpace(s))
					{
						var items = StringUtilities.SplitCsv(s).ToList();

						foreach (var item in items)
						{
							var h = item == null ? string.Empty : item.Trim();
							if (!string.IsNullOrWhiteSpace(h))
							{
								ret.Add(h);
							}
						}
					}

					_removeResponseHeaders = ret.ToArray();
				}

				return _removeResponseHeaders;
			}
		}

		public void Init(HttpApplication context)
		{
			context.PreSendRequestHeaders += RemoveHeaders;
		}

		public void Dispose()
		{
		}

		private static void RemoveHeaders(object sender, EventArgs e)
		{
			try
			{
				if (HttpRuntime.UsingIntegratedPipeline)
				{
					var httpApp = sender as HttpApplication;

					if (httpApp != null)
					{
						var responseHeaders = httpApp.Context.Response.Headers;

						foreach (var item in RemoveResponseHeaders)
						{
							responseHeaders.Remove(item);
						}
					}
				}
			}
			catch { }
		}
	}
}
