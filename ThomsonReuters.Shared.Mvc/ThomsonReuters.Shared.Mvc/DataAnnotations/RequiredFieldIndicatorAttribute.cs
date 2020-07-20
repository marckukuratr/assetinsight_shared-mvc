using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ThomsonReuters.Shared.DataAnnotations
{
	public class RequiredFieldIndicatorAttribute : ValidationAttribute, IClientValidatable
	{
		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			var rule = new ModelClientValidationRule();
			rule.ValidationType = "reqfldind";

			yield return rule;
		}

		public override bool IsValid(object value)
		{
			return true;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			return null;
		}
	}
}
