using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web.Routing
{
	public class NotEqualRouteConstraint : IRouteConstraint
	{
		private readonly List<string> _match;

		public NotEqualRouteConstraint(params string[] match)
		{
			_match = match.ToList();
		}

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			var val = values[parameterName] as string;
			var ret = !_match.Contains(val, StringComparer.OrdinalIgnoreCase);
			return ret;
		}
	}

	public class EqualRouteConstraint : IRouteConstraint
	{
		private readonly List<string> _match;

		public EqualRouteConstraint(params string[] match)
		{
			_match = match.ToList();
		}

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			var val = values[parameterName] as string;
			var ret = _match.Contains(val, StringComparer.OrdinalIgnoreCase);
			return ret;
		}
	}

	public class EqualNotEqualRouteConstraint : IRouteConstraint
	{
		private readonly List<string> _equal;
		private readonly List<string> _notEqual;

		/// <param name="equal">Use '!Constraint' for Not Equal checks</param>
		public EqualNotEqualRouteConstraint(params string[] match)
		{
			_equal = match.Where(t => !t.StartsWith("!")).ToList();
			_notEqual = match.Where(t => t.StartsWith("!")).Select(t => t.TrimStart('!')).ToList();
		}

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			var val = values[parameterName] as string;

			var equal = _equal.Contains(val, StringComparer.OrdinalIgnoreCase);
			var notEqual = !_notEqual.Contains(val, StringComparer.OrdinalIgnoreCase);

			return equal && notEqual;
		}
	}

	public class IntIdRouteContraint : IRouteConstraint
	{
		private readonly bool PassOptionalParameter = false;

		public IntIdRouteContraint() { }

		public IntIdRouteContraint(bool passOptionalParameter)
		{
			PassOptionalParameter = passOptionalParameter;
		}

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			var ret = true;

			if (parameterName == "id")
			{
				var obj = values[parameterName];

				if (PassOptionalParameter && (obj == UrlParameter.Optional))
				{
					ret = true;
				}
				else
				{
					ret = obj is int;

					if (!ret && (obj != null))
					{
						int tmp;
						ret = int.TryParse(obj.ToString(), out tmp);
					}
				}
			}

			return ret;
		}
	}
}
