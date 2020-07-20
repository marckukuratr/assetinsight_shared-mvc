using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ThomsonReuters.Shared.Web
{
	public class JsonResultException : Exception
	{
		public JsonResultException(JsonResult result, Exception innerException, HttpStatusCode status = HttpStatusCode.InternalServerError)
			: base("Unexpected Error", innerException)
		{
			Result = result;
			Status = status;
		}

		public JsonResult Result { get; protected set; }
		public HttpStatusCode Status { get; protected set; }

		public override string Message
		{
			get
			{
				if (Result != null && Result.Data != null)
				{
					return Result.Data.ToString();
				}

				return base.Message;
			}
		}
	}
}
