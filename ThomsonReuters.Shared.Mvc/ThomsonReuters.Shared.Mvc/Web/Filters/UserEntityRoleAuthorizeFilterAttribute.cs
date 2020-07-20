using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace ThomsonReuters.Shared.Web.Filters
{
	public abstract class UserEntityRoleAuthorizeFilterAttribute : ActionFilterAttribute
	{
		public bool IsDisabled { get; set; }
		public bool IsDeny { get; protected set; }
		public int EntityContext { get; protected set; }
		public int[] Roles { get; protected set; }
		public string Message { get; set; }
		public virtual bool DoNotThrowExceptionOnFail { get; set; }
		public Func<ActionExecutingContext, bool> AuthorizeRoutine { get; private set; }


		public UserEntityRoleAuthorizeFilterAttribute(int entityContext, params int[] roles)
		{
			EntityContext = entityContext;
			Roles = roles;
		}

		public UserEntityRoleAuthorizeFilterAttribute(bool isDeny, int entityContext, params int[] roles)
			: this(entityContext, roles)
		{
			IsDeny = isDeny;
		}

		public UserEntityRoleAuthorizeFilterAttribute(Func<ActionExecutingContext, bool> authorizeRoutine)
		{
			AuthorizeRoutine = authorizeRoutine;
		}


		public sealed override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!IsDisabled)
			{
				bool canPass = false;

				if (AuthorizeRoutine != null)
				{
					canPass = AuthorizeRoutine(filterContext);
				}
				else
				{
					canPass = Authorize(filterContext);
				}

				if (!canPass)
				{
					var noMessage = string.IsNullOrWhiteSpace(Message);

					if (!DoNotThrowExceptionOnFail)
					{
						var ex = noMessage ? new UnauthorizedEntityAccessException() : new UnauthorizedEntityAccessException(Message);
						throw ex;
					}
					else
					{
						filterContext.Result = noMessage ? new HttpUnauthorizedResult() : new HttpUnauthorizedResult(Message);
					}
				}
			}

			base.OnActionExecuting(filterContext);
		}

		protected abstract bool Authorize(ActionExecutingContext filterContext);

		protected TRole[] GetRolesAs<TRole>()
			where TRole : struct
		{
			return GetRolesAs<TRole>(Roles);
		}

		protected static TRole[] GetRolesAs<TRole>(int[] roles)
			where TRole : struct
		{
			TRole[] ret = null;

			if (roles != null && roles.Length > 0)
			{
				List<TRole> lst = new List<TRole>();
				foreach (var item in roles)
				{
					lst.Add((TRole)(object)item);
				}
				ret = lst.ToArray();
			}

			return ret;
		}
	}
}
