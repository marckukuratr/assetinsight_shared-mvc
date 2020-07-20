using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThomsonReuters.Shared.Web
{
	public abstract class NotFoundException : ApplicationException
	{
		public NotFoundException()
			: base() { }
		public NotFoundException(string message)
			: base(message) { }
		public NotFoundException(string message, Exception innerException)
			: base(message, innerException) { }
	}

	public class EntityNotFoundException : NotFoundException
	{
		public EntityNotFoundException()
			: base() { }
		public EntityNotFoundException(string message)
			: base(message) { }
		public EntityNotFoundException(string message, Exception innerException)
			: base(message, innerException) { }
	}
}
