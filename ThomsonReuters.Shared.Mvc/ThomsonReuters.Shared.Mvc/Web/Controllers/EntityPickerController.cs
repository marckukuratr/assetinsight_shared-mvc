using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using ThomsonReuters.Shared.Model;
using ThomsonReuters.Shared.Repositories;
using ThomsonReuters.Shared.ViewModels;
using ThomsonReuters.Shared.Web.Filters;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web.Controllers
{
	[Authorize]
	public abstract class EntityPickerController<TEntity> : Controller
		where TEntity : IEntity
	{
		public FileStreamResult Contents(string id)
		{
			var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().FirstOrDefault(f => f.EndsWith(id));
			var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			var mimeType = GetMIMEType(id);

			return new FileStreamResult(resourceStream, mimeType);
		}

		[HttpPost]
		public virtual ActionResult Search(string text, string excludeValues)
		{
			var result = new List<TEntity>();

			if (!string.IsNullOrWhiteSpace(text))
			{
				result = SearchEntities(text, excludeValues).ToList();
			}

			if (!result.Any())
			{
				var emptyItem = new EntityViewModel { EntityID = string.Empty, DisplayName = "No match found" };
				var emptyResult = new List<EntityViewModel>() { emptyItem };

				return new JsonResult { Data = new SelectList(emptyResult, "EntityID", "DisplayName") };
			}
			else
			{
				return new JsonResult { Data = new SelectList(result, "EntityID", "DisplayName") };
			}
		}

		public virtual List<EntityViewModel> GetEntityViewModelsByValues(string values)
		{
			values = values == null ? string.Empty : values.Trim();

			var ret = new List<EntityViewModel>();

			if (!string.IsNullOrWhiteSpace(values))
			{
				var entities = GetEntitiesByIdCsv(values).ToArray();

				ret = entities
					.OrderBy(t => t.DisplayName, StringComparer.InvariantCultureIgnoreCase)
					.Select(t => new EntityViewModel { EntityID = t.EntityID, DisplayName = t.DisplayName })
					.ToList();
			}

			return ret;
		}


		protected abstract IEnumerable<TEntity> SearchEntities(string text, string excludeValues);

		protected abstract IEnumerable<TEntity> GetEntitiesByIdCsv(string idCsv);


		protected static List<T> SplitIdCsv<T>(string idCsv)
		{
			idCsv = idCsv == null ? string.Empty : idCsv;

			var tType = typeof(T);
			var ret = new List<T>();

			if (tType == typeof(string))
			{
				var tmp = StringUtilities.SplitCsv(idCsv).ToList();
				ret = tmp as List<T>;
			}
			else if (tType == typeof(int))
			{
				var tmp = StringUtilities.SplitCsvToInt32(idCsv).ToList();
				ret = tmp as List<T>;
			}
			else
			{
				throw new NotSupportedException(string.Format("Id type '{0}' is not supported.", tType.Name));
			}

			if (ret == null)
			{
				throw new InvalidOperationException("Unable to split id csv.");
			}

			return ret;
		}

		private static string GetMIMEType(string fileId)
		{
			if (fileId.EndsWith(".js"))
			{
				return "text/javascript";
			}
			else if (fileId.EndsWith(".css"))
			{
				return "text/css";
			}
			else if (fileId.EndsWith(".jpg"))
			{
				return "image/jpeg";
			}
			return "text";
		}
	}
}
