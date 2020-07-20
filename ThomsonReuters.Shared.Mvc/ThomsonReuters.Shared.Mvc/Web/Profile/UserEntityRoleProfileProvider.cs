using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Profile;

namespace ThomsonReuters.Shared.Web.Profile
{
	public interface IUserEntityRoleProfileProvider
	{
		void DeleteUserEntityRoles(string userID, int entityContext, string entityID);
		void DeleteUserEntityRoles(string userID, int entityContext, string entityID, int role);
		void DeleteByEntity(int entityContext, string entityID);
	}

	public class UserEntityRoleProfileProvider : ProfileProvider, IUserEntityRoleProfileProvider
	{
		public override string ApplicationName { get; set; }

		public string ConnectionString { get; set; }

		public string GetProcedure { get; set; }

		public string DeleteProcedure { get; set; }

		public string SetProcedure { get; set; }


		public override void Initialize(string name, NameValueCollection config)
		{
			base.Initialize(name, config);

			// Connection String
			if (!string.IsNullOrEmpty(config["connectionStringName"]))
			{
				string conStrName = config["connectionStringName"];
				ConnectionStringSettings connObj = ConfigurationManager.ConnectionStrings[conStrName];
				if (connObj != null)
				{
					ConnectionString = connObj.ConnectionString;
				}
			}
			else
			{
				throw new ProviderException("connectionStringName not specified");
			}

			// Get Procedure
			if (!string.IsNullOrEmpty(config["getProcedureName"]))
			{
				GetProcedure = config["getProcedureName"];
			}
			else
			{
				throw new ProviderException("getProcedureName not specified");
			}

			// Delete Procedure
			if (!string.IsNullOrEmpty(config["deleteProcedureName"]))
			{
				DeleteProcedure = config["deleteProcedureName"];
			}
			else
			{
				throw new ProviderException("deleteProcedureName not specified");
			}

			// Set Procedure
			if (!string.IsNullOrEmpty(config["setProcedureName"]))
			{
				SetProcedure = config["setProcedureName"];
			}
			else
			{
				throw new ProviderException("setProcedureName not specified");
			}
		}

		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
		{
			SettingsPropertyValueCollection ret = new SettingsPropertyValueCollection();

			SqlConnection sqlCon = new SqlConnection(ConnectionString);
			SqlCommand sqlCmd = new SqlCommand(GetProcedure, sqlCon);
			sqlCmd.CommandType = CommandType.StoredProcedure;

			sqlCmd.Parameters.AddWithValue("@UserID", (string)context["UserName"]);

			try
			{
				sqlCon.Open();
				SqlDataReader reader = sqlCmd.ExecuteReader();

				UserRoles roles = new UserRoles();
				SettingsProperty sp = new SettingsProperty("Roles");
				sp.PropertyType = typeof(UserRoles);

				SettingsPropertyValue spValue = new SettingsPropertyValue(sp);
				spValue.PropertyValue = roles;

				while (reader.Read())
				{
					UserRole role = new UserRole
					{
						UserID = (string)reader["UserID"],
						EntityContext = (int)reader["EntityContext"],
						EntityID = (string)reader["EntityID"],
						Role = (int)reader["Role"]
					};
					roles.Add(role);
				}

				ret.Add(spValue);
			}
			finally
			{
				sqlCon.Close();
				sqlCmd.Dispose();
			}

			return ret;
		}

		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
		{
			UserRoles roles = (UserRoles)collection["Roles"].PropertyValue;

			if (roles.Count > 0)
			{
				SqlConnection sqlCon = new SqlConnection(ConnectionString);
				sqlCon.Open();

				foreach (var item in roles)
				{
					SqlCommand sqlCmd = new SqlCommand(SetProcedure, sqlCon);
					sqlCmd.CommandType = CommandType.StoredProcedure;

					sqlCmd.Parameters.AddWithValue("@UserID", item.UserID);
					sqlCmd.Parameters.AddWithValue("@EntityContext", item.EntityContext);
					sqlCmd.Parameters.AddWithValue("@EntityID", item.EntityID);
					sqlCmd.Parameters.AddWithValue("@Role", item.Role);

					sqlCmd.ExecuteNonQuery();
				}
			}
		}

		public override int DeleteProfiles(string[] usernames)
		{
			int ret = 0;

			if (usernames != null && usernames.Length > 0)
			{
				ret = usernames.Length;

				SqlConnection sqlCon = new SqlConnection(ConnectionString);
				sqlCon.Open();

				foreach (var item in usernames)
				{
					SqlCommand sqlCmd = new SqlCommand(DeleteProcedure, sqlCon);
					sqlCmd.CommandType = CommandType.StoredProcedure;
					sqlCmd.Parameters.AddWithValue("@UserID", item);

					sqlCmd.ExecuteNonQuery();
				}
			}

			return ret;
		}

		public override int DeleteProfiles(ProfileInfoCollection profiles)
		{
			List<string> lst = new List<string>();

			foreach (var item in profiles)
			{
				ProfileInfo pi = (ProfileInfo)item;
				lst.Add(pi.UserName);
			}

			int ret = DeleteProfiles(lst.ToArray());
			return ret;
		}

		public void DeleteByEntity(int entityContext, string entityID)
		{
			SqlConnection sqlCon = new SqlConnection(ConnectionString);
			sqlCon.Open();

			SqlCommand sqlCmd = new SqlCommand(DeleteProcedure, sqlCon);
			sqlCmd.CommandType = CommandType.StoredProcedure;
			sqlCmd.Parameters.AddWithValue("@EntityContext", entityContext);
			sqlCmd.Parameters.AddWithValue("@EntityID", entityID);

			sqlCmd.ExecuteNonQuery();
		}

		public void DeleteUserEntityRoles(string userID, int entityContext, string entityID)
		{
			SqlConnection sqlCon = new SqlConnection(ConnectionString);
			sqlCon.Open();

			SqlCommand sqlCmd = new SqlCommand(DeleteProcedure, sqlCon);
			sqlCmd.CommandType = CommandType.StoredProcedure;
			sqlCmd.Parameters.AddWithValue("@UserID", userID);
			sqlCmd.Parameters.AddWithValue("@EntityContext", entityContext);
			sqlCmd.Parameters.AddWithValue("@EntityID", entityID);

			sqlCmd.ExecuteNonQuery();
		}

		public void DeleteUserEntityRoles(string userID, int entityContext, string entityID, int role)
		{
			throw new NotImplementedException();
		}


		public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException();
		}
	}
}
