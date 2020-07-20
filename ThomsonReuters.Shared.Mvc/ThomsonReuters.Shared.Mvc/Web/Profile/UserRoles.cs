using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThomsonReuters.Shared.Web.Profile
{
	public class UserRole
	{
		public string UserID { get; set; }
		public int EntityContext { get; set; }
		public string EntityID { get; set; }
		public int Role { get; set; }
	}

	public class UserRoles : List<UserRole>
	{
	}
}
