using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThomsonReuters.Shared.ViewModels
{
	public interface ICustomCommandViewModel<TCommandType>
	{
		TCommandType CustomCommand { get; set; }

		string CustomCommandArgument { get; set; }

		T CustomCommandArgumentAs<T>();

		T CustomCommandArgumentAs<T>(T defaultValue);
	}
}
