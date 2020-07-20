using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThomsonReuters.Shared.Web
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class ControllerAliasAttribute : Attribute
	{
		public ControllerAliasAttribute(string alias)
		{
			if (string.IsNullOrWhiteSpace(alias))
			{
				throw new ArgumentNullException("alias");
			}

			Alias = alias;
		}

		public string Alias { get; protected set; }
	}
}
