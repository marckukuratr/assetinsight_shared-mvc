using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ThomsonReuters.Shared.Web
{
	public class CustomSelectListItem : SelectListItem
	{
		public bool IsDisabled { get; set; }
		public bool IsObsolete { get; set; }
		public bool UseTitle { get; set; }
		public string Title { get; set; }
		public string Class { get; set; }
	}
}
