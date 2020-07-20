using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThomsonReuters.Data.Repository;
using ThomsonReuters.Shared.Model;

namespace ThomsonReuters.Shared.Repositories
{
	public interface IActionLogRepository : IRepository
	{
		void AddToActionLogs(IActionLog actionLog, bool saveChanges);

		IQueryable<IActionLog> GetActionLogs(string sessionID, bool orderAscendingByDateTime);

		IQueryable<IActionLog> GetActionLogs(string user, string sessionID, object eventType, DateTime? startDate, DateTime? endDate, bool isExcludeUser, bool isExcludeEvent);

		IQueryable<IActionLog> GetActionLogs(IEnumerable<string> users, string sessionID, IEnumerable<object> eventTypes, DateTime? startDate, DateTime? endDate, bool isExcludeUser, bool isExcludeEvent);

		IQueryable<IGrouping<string, IActionLog>> GetMostCommonDataGrouped(IEnumerable<string> users, string sessionID, object eventType, DateTime? startDate, DateTime? endDate, bool isExcludeUser, bool isExcludeEvent, int records);

		void SaveChanges();
	}
}
