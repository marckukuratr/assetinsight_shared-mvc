using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web.Authentication
{
	public class SafeAuthentication
	{
		public const string SETTING_SAFE_AUTH_KEY = "Shared.SafeAuthKey";

		private string AuthKey
		{
			get
			{
				return ConfigurationManager.AppSettings[SETTING_SAFE_AUTH_KEY];
			}
		}

		public void Validate(string uid, string time, string digest)
		{
			if (string.IsNullOrWhiteSpace(AuthKey))
			{
				throw new AuthenticationException("Missing Auth Key in configuration file");
			}
			if (string.IsNullOrWhiteSpace(uid))
			{
				throw new AuthenticationException("Argument cannot be null or empty: uid");
			}
			if (string.IsNullOrWhiteSpace(time))
			{
				throw new AuthenticationException("Argument cannot be null or empty: time");
			}
			if (string.IsNullOrWhiteSpace(digest))
			{
				throw new AuthenticationException("Argument cannot be null or empty: digest");
			}

			var hashMD52 = new MD5CryptoServiceProvider();
			var sPassword = uid + time + AuthKey;

			var pwdHash2 = hashMD52.ComputeHash(System.Text.Encoding.Default.GetBytes(sPassword));
			var hexStr = string.Empty;

			for (int x = 0; x < pwdHash2.Length; x++)
			{
				var hexChar = Hex(pwdHash2[x]);

				if (hexChar.Length == 1)
				{
					hexStr += "0";
				}
				hexStr += hexChar;
			}

			if (!StringUtilities.AreEqualCaseInsensitive(digest, hexStr))
			{
				throw new AuthenticationException("Authorization Error");
			}
		}

		private string Hex(byte b)
		{
			int val = Convert.ToInt32(b);
			return val.ToString("X");
		}
	}

	public class AuthenticationException : Exception
	{
		public AuthenticationException(string message) : base(message) { }
		public AuthenticationException(string message, Exception innerException) : base(message, innerException) { }
	}
}
