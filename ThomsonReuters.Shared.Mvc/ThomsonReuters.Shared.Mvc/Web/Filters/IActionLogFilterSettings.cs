using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThomsonReuters.Shared.Model;
using ThomsonReuters.Shared.Repositories;

namespace ThomsonReuters.Shared.Web.Filters
{
	/// <summary>
	/// IMPORTANT! : Filters are cached since MVC 3. Be carefull to maintain states inside the filters
	/// </summary>
	public interface IActionLogFilterSettings
	{
		IActionLogRepository Repository { get; set; }
		IActionLog NewActionLog();
	}
}
