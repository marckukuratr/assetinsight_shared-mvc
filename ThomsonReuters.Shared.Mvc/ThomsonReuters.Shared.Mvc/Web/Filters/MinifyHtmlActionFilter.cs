using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ThomsonReuters.Utilities;

namespace ThomsonReuters.Shared.Web.Filters
{
	// http://arranmaclean.wordpress.com/2010/08/10/minify-html-with-net-mvc-actionfilter/

	public class MinifyHtmlActionFilter : ActionFilterAttribute
	{
		public bool IsDisabled { get; set; }

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!IsDisabled)
			{
				var minify = SharedWebConfigHelper.IsMinifyResources;

				if (minify.HasValue == true &&
					minify.Value == MinificationType.HtmlScriptStyles)
				{
					var response = filterContext.HttpContext.Response;
					response.Filter = new ReposeFilterStream(response.Filter);
				}
			}
		}

		private class ReposeFilterStream : MemoryStream
		{
			private readonly Stream actualStream;

			public ReposeFilterStream(Stream stream)
			{
				actualStream = stream;
			}

			public override bool CanRead { get { return true; } }
			public override bool CanSeek { get { return true; } }
			public override bool CanWrite { get { return true; } }
			public override void Flush() { actualStream.Flush(); }
			public override long Length { get { return actualStream.Length; } }

			public override int Read(byte[] buffer, int offset, int count)
			{
				return actualStream.Read(buffer, offset, count);
			}
			public override long Seek(long offset, SeekOrigin origin)
			{
				return actualStream.Seek(offset, origin);
			}
			public override void SetLength(long value)
			{
				actualStream.SetLength(value);
			}
			public override void Close()
			{
				actualStream.Close();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				var isTextHtml = false;

				try
				{
					// We need to optimize only html. Applying optimization to any file download will curropt the file
					isTextHtml = StringUtilities.AreEqualCaseInsensitive(HttpContext.Current.Response.ContentType, System.Net.Mime.MediaTypeNames.Text.Html);
				}
				catch { }

				if (isTextHtml)
				{
					// capture the data and convert to string 
					byte[] data = new byte[count];
					Buffer.BlockCopy(buffer, offset, data, 0, count);
					string s = Encoding.Default.GetString(buffer);

					// filter the string
					StringBuilder sb = new StringBuilder();
					using (StringReader sr = new StringReader(s))
					{
						while (true)
						{
							var line = sr.ReadLine();

							if (line != null)
							{
								line = line.TrimStart();

								if (!string.IsNullOrWhiteSpace(line))
								{
									sb.AppendLine(line);
								}
							}
							else
							{
								break;
							}
						}
					}

					s = sb.ToString().TrimEnd("\r\n".ToCharArray());

					if (!string.IsNullOrWhiteSpace(s))
					{
						// write the data to stream 
						byte[] outdata = Encoding.Default.GetBytes(s);
						actualStream.Write(outdata, 0, outdata.GetLength(0));
					}
				}
				else
				{
					actualStream.Write(buffer, 0, count);
				}
			}
		}
	}
}
