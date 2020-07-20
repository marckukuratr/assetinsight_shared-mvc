using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web
{
	public enum MinificationType
	{
		None = 0,
		ScriptStyles = 1,
		HtmlScriptStyles = 2,
	}

	[Flags]
	public enum WebApplicationHostingCaps
	{
		/// <summary>
		/// Specifies whether application is running in HTTPS.
		/// </summary>
		Https = 1,
		/// <summary>
		/// Specifies whether Load Balancing is enabled for the application.
		/// </summary>
		LoadBalancing = 2,
		/// <summary>
		/// Specifies whether the front end infrastructure (Load Balancer) redirects to a different port where application is listening.
		/// </summary>
		PortRedirection = 4,
	}

	public class AppSettingKeys
	{
		public const string APP_SITE_BASE_URL = "Shared.SiteBaseUrl";
		public const string APP_SETTING_REMOVE_HEADER = "Shared.RemoveResponseHeaders";
		public const string APP_SETTING_MINIFY_RESOURCES = "Shared.MinifyResources";
		public const string APP_SETTING_HOSTING_CAPS = "Shared.HostingCaps";
	}

	public class SharedWebConfigHelper
	{
		public static string RemoveResponseHeaders
		{
			get
			{
				var s = ConfigurationUtilities.GetString(AppSettingKeys.APP_SETTING_REMOVE_HEADER, false);
				return s;
			}
		}

		public static MinificationType? IsMinifyResources
		{
			get
			{
				var s = ConfigurationUtilities.GetString(AppSettingKeys.APP_SETTING_MINIFY_RESOURCES, false);

				MinificationType tmp;
				if (Enum.TryParse<MinificationType>(s, true, out tmp))
				{
					var allVals = Enum.GetValues(typeof(MinificationType)).Cast<MinificationType>().ToList();
					if (allVals.Contains(tmp))
					{
						return tmp;
					}
					else
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}
		}

		public static WebApplicationHostingCaps HostingCaps
		{
			get
			{
				var ret = (WebApplicationHostingCaps)ConfigurationUtilities.GetInt(AppSettingKeys.APP_SETTING_HOSTING_CAPS, false, 0);
				return ret;
			}
		}
	}

	public static class CommonViewNames
	{
		public const string VIEW_ERROR = "Error";
		public const string VIEW_UNAUTHORIZED = "Unauthorized";
		public const string VIEW_NOTFOUND = "NotFound";
	}

	public static class MessageContants
	{
		public const string ERR_MSG_UNKNOWN = "UNKNOWN ERROR.";
		public const string ERR_MSG_LOGIN = "Sorry, an error occurred while login into the application.";
		public const string ERR_MSG_ERROR = "Sorry, an error occurred while processing your request.";
		public const string ERR_MSG_NOTFOUND = "Sorry, the resource you have requested does not exist.";
		public const string ERR_MSG_UNAUTHORIZED = "Sorry, but you are unable to access this resource.<br/>Please contact an administrator if you believe you<br/>have received this warning in error.";
		public const string ERR_MSG_USER_NOTFOUND = "Sorry, ACCESS DENIED User profile doesn’t exist in LDAP";
	}
}
