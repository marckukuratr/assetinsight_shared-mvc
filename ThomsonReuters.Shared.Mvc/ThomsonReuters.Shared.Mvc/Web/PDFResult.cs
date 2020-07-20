using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using ThomsonReuters.Shared.Web.Html;

namespace ThomsonReuters.Shared.Web
{
	public class PDFResult : ViewResult
	{
		public PDFResult(object model, string viewName)
		{
			base.ViewData = new ViewDataDictionary(model);
			base.ViewName = viewName;
		}

		public PDFResult(object model, string viewName, string downloadFileName)
		{
			base.ViewData = new ViewDataDictionary(model);
			base.ViewName = viewName;

			if (!string.IsNullOrWhiteSpace(downloadFileName))
			{
				IsDownloadAsFile = true;
				DownloadFileName = downloadFileName;
			}
			else
			{
				throw new ArgumentException("Parameter cannot be null or empty", "downloadFileName");
			}
		}

		public bool IsDownloadAsFile { get; private set; }

		public string DownloadFileName { get; private set; }

		protected override ViewEngineResult FindView(ControllerContext context)
		{
			ViewEngineResult result = base.FindView(context);
			if (result.View == null)
			{
				return result;
			}
			PDFView view = new PDFView(result, DownloadFileName);
			return new ViewEngineResult(view, view);
		}


		private class PDFView : IView, IViewEngine
		{
			private readonly string _PAGEBREAK = "PAGEBREAK";
			private readonly ViewEngineResult _result;
			private readonly string _fileName;

			public PDFView(ViewEngineResult result, string fileName)
			{
				this._result = result;
				this._fileName = fileName;
			}

			public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
			{
				throw new NotImplementedException();
			}

			public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
			{
				throw new NotImplementedException();
			}

			public void ReleaseView(ControllerContext controllerContext, IView view)
			{
				this._result.ViewEngine.ReleaseView(controllerContext, this._result.View);
			}

			public void Render(ViewContext viewContext, TextWriter writer)
			{
				var viewHtml = GetViewHtml(viewContext);
				string html;
				StyleSheet styles;

				GetHtmlAndCSS(ref viewHtml, out html, out styles);

				Document document = new Document(PageSize.A4, 30, 30, 30, 35);
				PdfWriter instance = PdfWriter.GetInstance(document, viewContext.HttpContext.Response.OutputStream);
				instance.CloseStream = true;

				document.Open();

				StringReader reader = new StringReader(html);
				var items = HTMLWorker.ParseToList(reader, styles);

				foreach (IElement elm in items)
				{
					// if a PAGEBREAK is found, then insert one into the PDF, e.g. <div>PAGEBREAK</div>
					if (elm.Chunks != null && elm.Chunks.Count == 1 && elm.Chunks[0].ToString() == _PAGEBREAK)
					{
						document.NewPage();
					}
					else
					{
						document.Add(elm);
					}
				}

				SetHeaders(viewContext);

				document.Close();
				instance.Close();
			}

			//public void Render(ViewContext viewContext, TextWriter writer)
			//{
			//	var viewHtml = GetViewHtml(viewContext);

			//	XmlParser iTextParser = GetParser(ref viewHtml);

			//	Document document = new Document();
			//	document.Open();

			//	PdfWriter instance = PdfWriter.GetInstance(document, viewContext.HttpContext.Response.OutputStream);
			//	instance.CloseStream = true;

			//	using (XmlTextReader tempReader = GetXmlReader(ref viewHtml))
			//	{
			//		iTextParser.Go(document, tempReader);
			//	}

			//	SetHeaders(viewContext);

			//	instance.Close();
			//}


			private string GetViewHtml(ViewContext viewContext)
			{
				StringBuilder sb = new StringBuilder();
				TextWriter writer = new StringWriter(sb);

				this._result.View.Render(viewContext, writer);

				var ret = sb.ToString();
				return ret;
			}

			private void GetHtmlAndCSS(ref string input, out string html, out StyleSheet styles)
			{
				var css = string.Empty;
				CSSParser.SeparateHtmlAndCss(input, out html, out css);

				styles = new StyleSheet();
				var parser = new CSSParser(css);

				parser.ApplyStyles(styles);
			}

			private void SetHeaders(ViewContext context)
			{
				context.HttpContext.Response.ContentType = "application/pdf";

				if (!string.IsNullOrWhiteSpace(_fileName))
				{
					ContentDisposition dispo = new ContentDisposition
					{
						FileName = _fileName
					};
					context.HttpContext.Response.AddHeader("Content-Disposition", dispo.ToString());
				}
			}

			private static XmlTextReader GetXmlReader(ref string source)
			{
				return new XmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(source))) { WhitespaceHandling = WhitespaceHandling.None };
			}
		}
	}
}
