using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThomsonReuters.Shared.Web
{
	public class UnauthorizedEntityAccessException : UnauthorizedAccessException
	{
		public UnauthorizedEntityAccessException()
			: base() { }

		public UnauthorizedEntityAccessException(string message)
			: base(message) { }

		public UnauthorizedEntityAccessException(string message, Exception innerException)
			: base(message, innerException) { }
	}
}
