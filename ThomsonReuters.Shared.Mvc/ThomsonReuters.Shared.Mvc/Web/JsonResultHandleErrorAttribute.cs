using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using ThomsonReuters.Logging;

namespace ThomsonReuters.Shared.Web
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class JsonResultHandleErrorAttribute : FilterAttribute, IExceptionFilter
	{
		private static Type elpInterfaceType = typeof(IErrorLogProvider);
		private IErrorLogProvider _errorLogProvider = null;


		public Type ErrorLogProviderType { get; set; }

		protected IErrorLogProvider ErrorLogProvider
		{
			get
			{
				_errorLogProvider = _errorLogProvider ?? CreateErrorLogProvider(ErrorLogProviderType);
				return _errorLogProvider;
			}
		}


		public void OnException(ExceptionContext filterContext)
		{
			if (filterContext.Exception != null && filterContext.Exception is JsonResultException)
			{
				LogError(filterContext);

				var ex = (JsonResultException)filterContext.Exception;

				filterContext.ExceptionHandled = true;
				filterContext.HttpContext.Response.Clear();
				filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
				filterContext.HttpContext.Response.StatusCode = (int)ex.Status;
				filterContext.Result = ex.Result;
			}
		}


		protected virtual void LogError(ExceptionContext filterContext)
		{
			// Log the error if an error log provider is specified
			if (ErrorLogProvider != null && filterContext.Exception != null)
			{
				ErrorLogProvider.LogError(filterContext.Exception);
			}
		}

		protected IErrorLogProvider CreateErrorLogProvider(Type type)
		{
			IErrorLogProvider ret = null;

			if (type != null)
			{
				var isValidType = type.GetInterfaces().Any(t => t == elpInterfaceType);

				if (isValidType)
				{
					ret = Activator.CreateInstance(type) as IErrorLogProvider;
				}
				else
				{
					throw new Exception("Specified type does not implement IErrorLogProvider.");
				}
			}

			return ret;
		}
	}
}
