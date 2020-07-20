using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThomsonReuters.Shared.Model
{
	public interface IEntity
	{
		string EntityID { get; set; }
		string DisplayName { get; set; }
	}
}
