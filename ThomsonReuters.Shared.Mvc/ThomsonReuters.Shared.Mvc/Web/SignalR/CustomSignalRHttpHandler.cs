using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Http;

namespace ThomsonReuters.Shared.Web.SignalR
{
	[Obsolete("This class is experimental. Do not use.", true)]
	public class CustomSignalRHttpHandler : DefaultHttpHandler
	{
		private static char[] CARR_EQUAL = "=".ToArray();

		private IConnection Connection { get; set; }

		public CustomSignalRHttpHandler(IConnection connection)
			: base(connection)
		{
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return Task.Run(async () =>
			{
				//var urlRequest = request.RequestUri;
				//var urlBase = new Uri(urlRequest.GetLeftPart(UriPartial.Authority));

				//var cookiesLookup = GetCookiesLookup(urlBase);

				var response = await base.SendAsync(request, cancellationToken);

				return response;
			});
		}

		private System.Net.Cookie GetBigIpSetCookieNameValue(HttpResponseMessage response, Uri urlBase)
		{
			var cookieHeader = base.CookieContainer.GetCookieHeader(urlBase);

			if (!string.IsNullOrWhiteSpace(cookieHeader))
			{
				var split = cookieHeader.Trim().Split(CARR_EQUAL, StringSplitOptions.RemoveEmptyEntries);

				var name = split[0].Trim();
				var value = split[1].Trim();

				var ret = new System.Net.Cookie(name: name, value: value)
				{
					Domain = response.RequestMessage.RequestUri.Host,
				};

				return ret;
			}

			return null;
		}

		private Dictionary<string, System.Net.Cookie> GetCookiesLookup(Uri urlBase)
		{
			var ret = new Dictionary<string, System.Net.Cookie>();
			var cookies = base.CookieContainer.GetCookies(urlBase);

			foreach (var item in cookies)
			{
				var cookie = (System.Net.Cookie)item;
				ret.Add(cookie.Name, cookie);
			}

			return ret;
		}
	}
}
