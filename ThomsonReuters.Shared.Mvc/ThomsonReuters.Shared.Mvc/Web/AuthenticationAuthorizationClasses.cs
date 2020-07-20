using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.ModelBinding;
using System.Web.Security;

namespace ThomsonReuters.Shared.Web
{
	public class FormsAuthenticationService : IFormsAuthenticationService
	{
		public void SignIn(string userName, bool createPersistentCookie)
		{
			if (string.IsNullOrWhiteSpace(userName))
			{
				throw new ArgumentException("Value cannot be null or empty.", "userName");
			}

			FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
		}

		public void SignOut()
		{
			FormsAuthentication.SignOut();
		}

	}

	public class AccountMembershipService : IMembershipService
	{
		private readonly MembershipProvider _provider;

		public AccountMembershipService()
			: this(null)
		{
		}

		public AccountMembershipService(MembershipProvider provider)
		{
			_provider = provider ?? Membership.Provider;
		}

		public int MinPasswordLength
		{
			get
			{
				return _provider.MinRequiredPasswordLength;
			}
		}

		public bool ValidateUser(string userName, string password)
		{
			if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
			if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

			return _provider.ValidateUser(userName, password);
		}

		public MembershipCreateStatus CreateUser(string userName, string password, string email)
		{
			if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
			if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
			if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Value cannot be null or empty.", "email");

			MembershipCreateStatus status;
			_provider.CreateUser(userName, password, email, null, null, true, null, out status);
			return status;
		}

		public bool ChangePassword(string userName, string oldPassword, string newPassword)
		{
			if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
			if (string.IsNullOrWhiteSpace(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
			if (string.IsNullOrWhiteSpace(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");

			// The underlying ChangePassword() will throw an exception rather
			// than return false in certain failure scenarios.
			try
			{
				MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
				return currentUser.ChangePassword(oldPassword, newPassword);
			}
			catch (ArgumentException)
			{
				return false;
			}
			catch (MembershipPasswordException)
			{
				return false;
			}
		}
	}
}
