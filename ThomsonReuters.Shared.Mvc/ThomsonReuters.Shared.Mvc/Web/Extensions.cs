using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ThomsonReuters.Shared.Web
{
	public static class TRSharedWebExtensions
	{
		public static List<UploadedFile> GetUploadedFiles(this HttpRequestBase request)
		{
			List<UploadedFile> ret = new List<UploadedFile>();
			var length = request.Files.Count;

			for (int i = 0; i < length; i++)
			{
				var pfile = request.Files[i];
				var uf = pfile.GetUploadedFile();

				ret.Add(uf);
			}

			return ret;
		}

		public static UploadedFile GetUploadedFile(this HttpPostedFileBase postedFile)
		{
			var memoryStream = new MemoryStream();
			postedFile.InputStream.CopyTo(memoryStream);

			UploadedFile uf = new UploadedFile
			{
				FileName = postedFile.FileName,
				ContentType = postedFile.ContentType,
				Length = postedFile.ContentLength,
				DataStream = memoryStream
			};

			return uf;
		}

		public static bool IsDebug(this HtmlHelper htmlHelper)
		{
#if DEBUG
			return true;
#else
			return false;
#endif
		}
	}
}
