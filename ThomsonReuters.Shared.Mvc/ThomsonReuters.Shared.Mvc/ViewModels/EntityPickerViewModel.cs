using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ThomsonReuters.Shared.ViewModels
{
	public abstract class EntityPickerViewModel
	{
		public class MarkingOption
		{
			public string FieldName { get; set; }
			public string SelectTitle { get; set; }
			public string UnselectTitle { get; set; }
			public string IconTooltip { get; set; }
			public string Values { get; set; }
		}

		public EntityPickerViewModel()
		{
			MinChars = 4;
			Delay = 750;
			AutoFocus = true;

			EmptyText = "Click here to add...";
			EmptyTextDisabled = "Not Specified";

			ExtractControllerNameFromClassName();
		}

		public EntityPickerViewModel(string controller)
			: base()
		{
			if (!string.IsNullOrWhiteSpace(controller))
			{
				Controller = controller;
			}
		}

		public uint MinChars { get; set; }
		public uint Delay { get; set; }
		public bool AutoFocus { get; set; }

		public string Controller { get; set; }
		public string SearchUrl { get; set; }
		public string CustomCommand { get; set; }
		public string CustomCommandArgs { get; set; }

		public string ID { get; set; }
		public string FullFieldName { get; set; }

		public bool IsDisabled { get; set; }
		public virtual bool IsMulti { get; set; }
		public int? MultiLimit { get; set; }
		public string EmptyText { get; set; }
		public string EmptyTextDisabled { get; set; }
		public IEnumerable<SelectListItem> Items { get; set; }
		public string Values { get; set; }
		public string ExcludeValues { get; set; }

		public MarkingOption Marking { get; set; }

		public dynamic AdditionalAttributes { get; set; }

		/// <param name="field">
		/// Optional: leave null for generating field name for current name. Provide an alternate field name to replace actual template field name.
		/// </param>
		public List<string> CreateFieldIdAndName(ViewDataDictionary viewData, string field = null)
		{
			var ret = new List<string>();

			var id = viewData.TemplateInfo.GetFullHtmlFieldId(null);
			var name = viewData.TemplateInfo.GetFullHtmlFieldName(null);

			if (!string.IsNullOrWhiteSpace(field))
			{
				var templateFieldPrefix = viewData.TemplateInfo.HtmlFieldPrefix;
				var lenTFP = templateFieldPrefix.Length;

				id = id.Remove(id.Length - lenTFP, lenTFP);
				id += field;

				name = name.Remove(name.Length - lenTFP, lenTFP);
				name += field;
			}

			ret.Add(id);
			ret.Add(name);

			return ret;
		}

		public static MarkingOption CreateMarkingOption(string fieldName, string values, string selectTitle, string unselectTitle, string iconTooltip = "Click here to mark this entity")
		{
			var emptyArgs = new List<string>();

			if (string.IsNullOrWhiteSpace(fieldName))
			{
				emptyArgs.Add("fieldName");
			}
			if (string.IsNullOrWhiteSpace(selectTitle))
			{
				emptyArgs.Add("selectTitle");
			}
			if (string.IsNullOrWhiteSpace(unselectTitle))
			{
				emptyArgs.Add("unselectTitle");
			}

			if (emptyArgs.Any())
			{
				var argsMsg = string.Join(", ", emptyArgs);
				throw new ArgumentNullException(argsMsg);
			}

			var ret = new MarkingOption
			{
				FieldName = fieldName,
				Values = values,
				SelectTitle = selectTitle,
				UnselectTitle = unselectTitle,
				IconTooltip = iconTooltip,
			};
			return ret;
		}

		private void ExtractControllerNameFromClassName()
		{
			string name = this.GetType().Name;

			if (name.ToUpper().EndsWith("VIEWMODEL"))
			{
				name = name.Substring(0, name.Length - 9);
			}

			Controller = name;
		}
	}
}
