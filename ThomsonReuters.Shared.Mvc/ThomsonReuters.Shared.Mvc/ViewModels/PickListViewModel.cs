using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ThomsonReuters.Shared.ViewModels
{
	public class PickListViewModel
	{
		public const string DYN_PICKLIST_ACTION_KEY = "DynamicPicklistAction";
		public const string DYN_PICKLIST_DEF_ACTION = "DynamicLoadPickList";

		public PickListViewModel(string[] selectedValues, IEnumerable<SelectListItem> sourceItems)
		{
			SelectedValues = selectedValues;
			SourceItems = sourceItems;
		}

		public string[] SelectedValues { get; private set; }
		public IEnumerable<SelectListItem> SourceItems { get; private set; }
		public int Min { get; set; }
		public int Max { get; set; }
	}
}
