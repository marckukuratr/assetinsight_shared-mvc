using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using ThomsonReuters.Shared.Web.Filters;
using ThomsonReuters.Shared.Web.Authentication;
using ThomsonReuters.Shared.Web;
using ThomsonReuters.Utilities;
using IdentityModel.Client;
using System.Net.Http;
using ThomsonReuters.Shared.Web.Controllers;
using ThomsonReuters.Secrets;
using Amazon.SecretsManager;

namespace ThomsonReuters.Shared.Web.Authentication
{

	public class AuthorizationManager
	{
		private string _postbackAuthUri;
		public AuthorizationManager(string postbackAuthUri)
		{
			_postbackAuthUri = postbackAuthUri;
		}

		public string GetSSOUrl(string returnUrl)
		{
			var url = string.Format(ConfigurationUtilities.GetString("SSO.AuthorizationUrl"), ConfigurationUtilities.GetString("SSO.ClientId"));
			var encodedUrl = HttpUtility.UrlEncode(returnUrl);
			return $"{url}&redirect_uri={_postbackAuthUri}&state={encodedUrl}";
		}

		public async System.Threading.Tasks.Task<string> Authorize(string code)
		{
			var client = new HttpClient();
			var secret = await GetClientSecret();

			// confirm legitimacy of the token
			var response = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
			{
				Address = ConfigurationUtilities.GetString("SSO.TokenUrl"),

				ClientId = ConfigurationUtilities.GetString("SSO.ClientId"),
				ClientSecret = secret,////ConfigurationUtilities.GetString("SSO.Secret"),

				Code = code,
				RedirectUri = _postbackAuthUri
			});

			if (response.IsError)
			{
				throw new AuthenticationException(response.Error);
			}

			// get specific details about the user
			var userInfoResponse = await client.GetUserInfoAsync(new UserInfoRequest
			{
				Address = ConfigurationUtilities.GetString("SSO.UserInfoUrl"),

				Token = response.AccessToken
			});

			if (userInfoResponse.IsError)
			{
				throw new AuthenticationException(userInfoResponse.Error);
			}

			return userInfoResponse.Json.TryGetString("uid");

			//note: we can also parse other user details from json response if necessary. e.g.:
			//userInfoResponse.Json.TryGetString("givenName"),
			//userInfoResponse.Json.TryGetString("sn") };
		}

		private async System.Threading.Tasks.Task<string> GetClientSecret()
		{
			var secrets = new Secrets.Secret(new AmazonSecretsManagerClient());

			return await secrets.GetSecret(ConfigurationUtilities.GetString("SSO.SecretKey"));
		}
	}
}















