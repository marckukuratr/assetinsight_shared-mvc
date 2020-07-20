using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ThomsonReuters.Shared.Web.Binding
{
	/// <summary>
	/// The DefaultModelBinder chokes on decimals when you have commas in the value.
	/// e.g. 1234.45 is acceptable, but 1,234.45 binds to null. The follow code is required in Global.asax.cs as well.
	/// 
	/// protected void Application_Start() {
	///		...
	///		ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
	/// }
	/// 
	/// http://haacked.com/archive/2011/03/19/fixing-binding-to-decimals.aspx
	/// </summary>
	public class DecimalModelBinder : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
			ModelState modelState = new ModelState { Value = valueResult };

			object actualValue = null;
			try
			{
				var attemptedValue = valueResult.AttemptedValue
					.Replace("$", "")
					.Replace("€", "")
					.Replace("£", "")
					;

				if (attemptedValue.Contains("("))
				{
					attemptedValue = attemptedValue.Replace("(", "").Replace(")", "").Replace("-", "");
					attemptedValue = "-" + attemptedValue;
				}

				actualValue = Convert.ToDecimal(attemptedValue, CultureInfo.CurrentCulture);
			}
			catch (FormatException e)
			{
				modelState.Errors.Add(e);
			}

			bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
			return actualValue;
		}
	}
}
