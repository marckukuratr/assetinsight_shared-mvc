using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThomsonReuters.Shared.ViewModels
{
	public interface IErrorViewModel
	{
		Uri Url { get; set; }
		Exception Error { get; set; }
		string Message { get; set; }
	}

	public interface IErrorViewModelProvider
	{
		IErrorViewModel NewErrorViewModel();
	}
}
