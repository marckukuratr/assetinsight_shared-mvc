using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Http;

namespace ThomsonReuters.Shared.Web.SignalR
{
	public class CustomSignalRHttpClient : DefaultHttpClient, IHttpClient
	{
		private IConnection _connection;
		private HttpMessageHandler _httpHandler;

		public new void Initialize(IConnection connection)
		{
			_connection = connection;

			base.Initialize(connection);
		}

		protected override HttpMessageHandler CreateHandler()
		{
			if (_httpHandler == null)
			{
				_httpHandler = new DefaultHttpHandler(_connection);
			}
			return _httpHandler;
		}
	}
}
