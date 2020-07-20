using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;

namespace ThomsonReuters.Shared.Web.Profile
{
	public class UserEntityRoleProfile : ProfileBase
	{
		private static ProfileProvider _provider;
		private static Dictionary<string, UserEntityRoleProfile> _profileCache = new Dictionary<string, UserEntityRoleProfile>(StringComparer.OrdinalIgnoreCase);


		private static ProfileProvider Provider
		{
			get
			{
				if (_provider == null)
				{
					// Get the first provider
					foreach (var item in HttpContext.Current.Profile.Providers)
					{
						_provider = (ProfileProvider)item;
						break;
					}
				}
				return _provider;
			}
		}

		private static IUserEntityRoleProfileProvider EntityRoleProfileProvider
		{
			get { return (IUserEntityRoleProfileProvider)Provider; }
		}

		public UserRoles Roles
		{
			get
			{
				return ((UserRoles)(this.GetPropertyValue("Roles")));
			}
			set
			{
				this.SetPropertyValue("Roles", value);
			}
		}


		public static UserEntityRoleProfile GetProfile(string userID)
		{
			UserEntityRoleProfile ret = null;
			if (!_profileCache.TryGetValue(userID, out ret))
			{
				ret = (UserEntityRoleProfile)ProfileBase.Create(userID);
				_profileCache.Add(userID, ret);
			}
			return ret;
		}

		public override void Save()
		{
			Provider.SetPropertyValues(Context, PropertyValues);
		}


		public TRole? GetRole<TRole>(int entityContext, string entityID)
			where TRole : struct
		{
			var roles = from ur in Roles
						where (ur.EntityContext == entityContext && ur.EntityID == entityID)
						select ur;

			TRole? ret = null;
			if (roles.Count() > 0)
			{
				int roleVal = roles.ToArray()[0].Role;
				ret = (TRole)(object)roleVal;
			}
			return ret;
		}

		public bool HasRole<TRole>(int entityContext, string entityID, params TRole[] roles)
			where TRole : struct
		{
			List<TRole> lstRoles = new List<TRole>(roles);

			int count = Roles.Count(r => (r.EntityContext == entityContext && r.EntityID == entityID && lstRoles.Contains((TRole)(object)r.Role) == true));

			return count > 0;
		}

		public void DeleteEntityRoles(int entityContext, string entityID)
		{
			InvalidateCache();
			EntityRoleProfileProvider.DeleteByEntity(entityContext, entityID);
		}

		public void DeleteUserEntityRoles(int entityContext, string entityID)
		{
			InvalidateCache();
			EntityRoleProfileProvider.DeleteUserEntityRoles(UserName, entityContext, entityID);
		}

		public void DeleteUserEntityRoles<TRole>(int entityContext, string entityID, params TRole[] roles)
		{
			throw new NotImplementedException();
		}

		public void SetRole<TRole>(int entityContext, string entityID, TRole role, bool save = false)
			where TRole : struct
		{
			int intRole = Convert.ToInt32(role);
			int count = Roles.Count(r => r.EntityContext == entityContext && r.EntityID == entityID && r.Role == intRole);

			if (count == 0)
			{
				InvalidateCache(UserName);

				UserRole newRole = new UserRole { UserID = this.UserName, EntityContext = entityContext, EntityID = entityID, Role = intRole };
				Roles.Add(newRole);

				if (save)
				{
					Save();
				}
			}
		}


		private static void InvalidateCache(string userID = null)
		{
			if (userID == null)
			{
				_profileCache.Clear();
			}
			else
			{
				if (_profileCache.ContainsKey(userID))
				{
					_profileCache.Remove(userID);
				}
			}
		}
	}
}
