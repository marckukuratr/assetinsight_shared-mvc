using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace ThomsonReuters.Shared.Web
{
	public static class ClientResourceManagerExtensions
	{
		public const string CONTEXT_KEY = "ClientResources";

		private const string JS_FILE_TEMPLATE = "<script type=\"text/javascript\" src=\"{0}\"></script>";
		private const string CSS_FILE_TEMPLATE = "<link href=\"{0}\" rel=\"stylesheet\"/>";

		private const string JS_TAG_TEMPLATE_START = "<script type=\"text/javascript\">";
		private const string JS_TAG_TEMPLATE_END = "</script>";

		private const string CSS_TAG_TEMPLATE_START = "<style type=\"text/css\">";
		private const string CSS_TAG_TEMPLATE_END = "</style>";


		private static ClientResourceContext Context
		{
			get
			{
				var ret = HttpContext.Current.Items[CONTEXT_KEY] as ClientResourceContext;
				if (ret == null)
				{
					ret = new ClientResourceContext();
					HttpContext.Current.Items[CONTEXT_KEY] = ret;
				}
				return ret;
			}
		}


		public static IHtmlString AddScriptBlock(this HtmlHelper html, HelperResult result, bool renderTag = false)
		{
			var data = result.ToHtmlString();

			Context.Add(ClientResourceContext.ResourceType.ScriptBlock, data, renderTag);

			// In ajax context render the view immediately
			if (html.ViewContext.HttpContext.Request.IsAjaxRequest())
			{
				return result;
			}
			return null;
		}

		/// <summary>
		/// Note: Duplicates will be ignored
		/// </summary>
		public static IHtmlString AddScriptFile(this HtmlHelper html, string url)
		{
			url = GetUrlhelper(html).Content(url);

			var justAdded = Context.Add(ClientResourceContext.ResourceType.ScriptFile, url);

			// In ajax context render the view immediately
			if (html.ViewContext.HttpContext.Request.IsAjaxRequest() && justAdded)
			{
				var text = string.Format(JS_FILE_TEMPLATE, url);
				return html.Raw(text);
			}
			return null;
		}


		public static IHtmlString AddStyleBlock(this HtmlHelper html, HelperResult result, bool renderTag = false)
		{
			var data = result.ToHtmlString();
			Context.Add(ClientResourceContext.ResourceType.StyleBlock, data, renderTag);

			if (html.ViewContext.HttpContext.Request.IsAjaxRequest())
			{
				return result;
			}
			return null;
		}

		/// <summary>
		/// Note: Duplicates will be ignored
		/// </summary>
		public static IHtmlString AddStyleFile(this HtmlHelper html, string url)
		{
			url = GetUrlhelper(html).Content(url);

			var justAdded = Context.Add(ClientResourceContext.ResourceType.StyleFile, url);

			// In ajax context render the view immediately
			if (html.ViewContext.HttpContext.Request.IsAjaxRequest() && justAdded)
			{
				var text = string.Format(CSS_FILE_TEMPLATE, url);
				return html.Raw(text);
			}
			return null;
		}


		public static IHtmlString RenderScripts(this HtmlHelper html)
		{
			if (html.ViewContext.HttpContext.Request.IsAjaxRequest())
			{
				return MvcHtmlString.Empty;
			}

			StringBuilder sb = new StringBuilder();
			var rFiles = Context.Resources.Where(t => t.Type == ClientResourceContext.ResourceType.ScriptFile).ToArray();
			var rBlocks = Context.Resources.Where(t => t.Type == ClientResourceContext.ResourceType.ScriptBlock).ToArray();

			// Files
			foreach (var r in rFiles)
			{
				var text = string.Format(JS_FILE_TEMPLATE, r.Data);
				sb.AppendLine(text);
			}

			// Blocks
			if (rBlocks.Any())
			{
				foreach (var cRes in rBlocks)
				{
					if (cRes.RenderTag)
					{
						sb.AppendLine(JS_TAG_TEMPLATE_START);
					}

					sb.AppendLine(cRes.Data);

					if (cRes.RenderTag)
					{
						sb.AppendLine(JS_TAG_TEMPLATE_END);
					}
				}
			}

			var ret = new HtmlString(sb.ToString());
			return ret;
		}

		public static IHtmlString RenderStyles(this HtmlHelper html)
		{
			if (html.ViewContext.HttpContext.Request.IsAjaxRequest())
			{
				return MvcHtmlString.Empty;
			}

			StringBuilder sb = new StringBuilder();
			var rFiles = Context.Resources.Where(t => t.Type == ClientResourceContext.ResourceType.StyleFile).ToArray();
			var rBlocks = Context.Resources.Where(t => t.Type == ClientResourceContext.ResourceType.StyleBlock).ToArray();

			foreach (var r in rFiles)
			{
				var text = string.Format(CSS_FILE_TEMPLATE, r.Data);
				sb.AppendLine(text);
			}

			if (rBlocks.Any())
			{
				foreach (var cRes in rBlocks)
				{
					if (cRes.RenderTag)
					{
						sb.AppendLine(CSS_TAG_TEMPLATE_START);
					}

					sb.AppendLine(cRes.Data);

					if (cRes.RenderTag)
					{
						sb.AppendLine(CSS_TAG_TEMPLATE_END);
					}
				}
			}

			var ret = new HtmlString(sb.ToString());
			return ret;
		}


		public static bool SearchClientResource(this HtmlHelper html, string search)
		{
			search = search.ToLower();
			var ret = Context.Resources.Any(t => t.Data.ToLower(CultureInfo.InvariantCulture).Contains(search));
			return ret;
		}


		private static UrlHelper GetUrlhelper(HtmlHelper html)
		{
			var ret = new UrlHelper(html.ViewContext.RequestContext, html.RouteCollection);
			return ret;
		}
	}

	internal class ClientResourceContext
	{
		public enum ResourceType
		{
			ScriptBlock,
			ScriptFile,
			StyleBlock,
			StyleFile,
		}

		public class ClientResource
		{
			public ResourceType Type { get; set; }
			public string Data { get; set; }
			public bool RenderTag { get; set; }
		}

		public ClientResourceContext()
		{
			Resources = new List<ClientResource>();
		}

		public List<ClientResource> Resources { get; private set; }

		public bool Add(ResourceType type, string data, bool renderTag = false)
		{
			if (type == ResourceType.ScriptFile || type == ResourceType.StyleFile)
			{
				var ldata = data.ToLower(CultureInfo.InvariantCulture);
				var isAddedAlready = Resources.Any(t => t.Data.ToLower() == ldata);

				if (isAddedAlready)
				{
					return false;
				}
			}

			var item = new ClientResource
			{
				Type = type,
				Data = data,
				RenderTag = renderTag,
			};

			Resources.Add(item);

			return true;
		}
	}
}
