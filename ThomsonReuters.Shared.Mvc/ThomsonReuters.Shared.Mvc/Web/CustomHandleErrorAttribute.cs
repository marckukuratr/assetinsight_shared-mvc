using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using ThomsonReuters.Logging;
using ThomsonReuters.Shared.ViewModels;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web
{
	// Concepts borrowed from:
	// http://www.codeproject.com/Articles/422572/Exception-Handling-in-ASP-NET-MVC
	// http://stackoverflow.com/questions/8348817/how-can-i-show-authenticated-but-unauthorized-users-an-unauthorized-page-mvc-3

	/// <summary>
	/// <para>*** NOTES ***</para>
	/// <para>AJAX Errors:</para>
	/// <para>The custom results are available at the 'jqXHR' parameter under the following property names for different content Types</para>
	/// <para>Text: responseText</para>
	/// <para>Html: responseText</para>
	/// <para>JSON: responseText, responseJSON</para>
	/// <para>XML: responseText, responseXML</para>
	/// <para>For HTML, a partial view will be executed as per the View property</para>
	/// </summary>
	public class CustomHandleErrorAttribute : HandleErrorAttribute
	{
		protected enum AjaxContentTypes
		{
			Text,
			Html,
			Json,
			Xml,
			Script,
		}

		private static Type elpInterfaceType = typeof(IErrorLogProvider);
		private IErrorLogProvider _errorLogProvider = null;


		public CustomHandleErrorAttribute()
		{
			RedirectControllerName = "Home";
			RedirectActionName = "Error";
			TryAndUseControllerAsErrorViewModelProvider = true;
		}


		public Type ErrorLogProviderType { get; set; }

		protected IErrorLogProvider ErrorLogProvider
		{
			get
			{
				_errorLogProvider = _errorLogProvider ?? CreateErrorLogProvider(ErrorLogProviderType);
				return _errorLogProvider;
			}
		}

		/// <summary>
		/// Default : true
		/// </summary>
		public bool TryAndUseControllerAsErrorViewModelProvider { get; set; }

		/// <summary>
		/// <para>Applicable only if TryAndUseControllerAsErrorViewModelProvider evaluates false on execution</para>
		/// <para>Default : Home</para>
		/// </summary>
		public string RedirectControllerName { get; set; }

		/// <summary>
		/// <para>Applicable only if TryAndUseControllerAsErrorViewModelProvider evaluates false on execution</para>
		/// <para>Default : Error</para>
		/// </summary>
		public string RedirectActionName { get; set; }


		public override void OnException(ExceptionContext filterContext)
		{
			// Handle only if CustomError is enabled and Exception is not handled elsewhere
			if (filterContext.HttpContext.IsCustomErrorEnabled &&
				!filterContext.ExceptionHandled)
			{
				// TODO : Currently only handles 500 type exception. Need new strategy to handle other types (may be through Applicaton_Error event)
				var isStatus500 = new HttpException(null, filterContext.Exception).GetHttpCode() == 500;

				// Handle only StatusCode = 500 error. Other errors will be reported according to CustomError redirects
				if (isStatus500 &&
					ExceptionType.IsInstanceOfType(filterContext.Exception))
				{
					LogError(filterContext);

					EvaluateErrorResult(filterContext);

					FinalTouch(filterContext);
				}
			}
			else
			{
				base.OnException(filterContext);
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

		protected virtual void EvaluateErrorResult(ExceptionContext filterContext, string resultMessage = MessageContants.ERR_MSG_ERROR)
		{
			if (!filterContext.HttpContext.Request.IsAjaxRequest())
			{
				var controller = filterContext.Controller as IErrorViewModelProvider;

				if (TryAndUseControllerAsErrorViewModelProvider == true && controller != null)
				{
					var model = controller.NewErrorViewModel();
					model.Error = filterContext.Exception;
					model.Message = resultMessage;
					model.Url = filterContext.HttpContext.Request.Url;

					filterContext.Result = new ViewResult
					{
						MasterName = Master,
						ViewName = View,
						ViewData = new ViewDataDictionary(model)
					};
				}
				else
				{
					filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = RedirectControllerName, action = RedirectActionName }));
				}
			}
			else
			{
				// These results are available at the 'jqXHR' parameter under the following property names for different content Types
				filterContext.Result = AjaxGetErrorResult(filterContext, resultMessage);
			}

			SetViewBagUser(filterContext);
		}

		protected virtual void FinalTouch(ExceptionContext filterContext,
			bool isExceptionHandled = true,
			int statusCode = 500,
			bool trySkipIisCustomErrors = true)
		{
			// Advertise that we handled the error
			filterContext.ExceptionHandled = isExceptionHandled;

			filterContext.HttpContext.Response.Clear();
			filterContext.HttpContext.Response.StatusCode = statusCode;
			filterContext.HttpContext.Response.TrySkipIisCustomErrors = trySkipIisCustomErrors; // Dont let the CustomError specifications take over
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

		protected virtual ActionResult AjaxGetErrorResult(ExceptionContext filterContext, string resultMessage)
		{
			ActionResult ret = new EmptyResult();
			var type = AjaxIdentifyContentType(filterContext);

			switch (type)
			{
				case AjaxContentTypes.Html:
					ret = new PartialViewResult
					{
						ViewName = View,
					};
					break;
				case AjaxContentTypes.Xml:
					ret = new ContentResult
					{
						Content = AjaxCreateXmlResponse(resultMessage),
						ContentType = System.Net.Mime.MediaTypeNames.Text.Xml,
					};
					break;
				case AjaxContentTypes.Json:
					ret = new JsonResult
					{
						JsonRequestBehavior = JsonRequestBehavior.AllowGet,
						Data = new
						{
							Success = false,
							Message = resultMessage,
						},
					};
					break;
				case AjaxContentTypes.Text:
					ret = new ContentResult
					{
						Content = resultMessage,
						ContentType = System.Net.Mime.MediaTypeNames.Text.Plain,
					};
					break;
				case AjaxContentTypes.Script:
				default:
					break;
			}

			return ret;
		}

		protected AjaxContentTypes AjaxIdentifyContentType(ExceptionContext filterContext)
		{
			AjaxContentTypes ret = AjaxContentTypes.Text;
			var acceptTypes = filterContext.HttpContext.Request.AcceptTypes;

			if (acceptTypes.Any(t => StringUtilities.AreEqualCaseInsensitive(t, "text/plain")))
			{
				ret = AjaxContentTypes.Text;
			}
			else if (acceptTypes.Any(t => StringUtilities.AreEqualCaseInsensitive(t, "text/html")))
			{
				ret = AjaxContentTypes.Html;
			}
			else if (acceptTypes.Any(t => StringUtilities.AreEqualCaseInsensitive(t, "application/json")))
			{
				ret = AjaxContentTypes.Json;
			}
			else if (acceptTypes.Any(t => StringUtilities.AreEqualCaseInsensitive(t, "application/xml")))
			{
				ret = AjaxContentTypes.Xml;
			}

			return ret;
		}

		protected string AjaxCreateXmlResponse(string resultMessage)
		{
			StringBuilder sb = new StringBuilder();

			XmlWriterSettings settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
			};

			using (XmlWriter wr = XmlWriter.Create(sb, settings))
			{
				wr.WriteStartElement("Error");

				wr.WriteElementString("Success", bool.FalseString);
				wr.WriteElementString("Message", resultMessage);

				wr.WriteEndElement();

				wr.Flush();
			}

			var ret = sb.ToString();
			return ret;
		}

		protected virtual void SetViewBagUser(ExceptionContext filterContext)
		{
		}

		protected static bool IsHttpExceptionOfType(ExceptionContext filterContext, HttpStatusCode statusCode)
		{
			var ret = false;
			var httpException = filterContext.Exception as HttpException;

			if (httpException != null)
			{
				var code = httpException.GetHttpCode();

				ret = code == (int)statusCode;
			}

			return ret;
		}
	}
}
