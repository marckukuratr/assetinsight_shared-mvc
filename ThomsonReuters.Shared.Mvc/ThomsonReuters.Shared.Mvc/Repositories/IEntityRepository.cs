using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThomsonReuters.Shared.Model;

namespace ThomsonReuters.Shared.Repositories
{
	public interface IEntityRepository<TEntity>
		where TEntity : IEntity
	{
		TEntity GetByID(string id);

		IEnumerable<TEntity> GetByIDCsv(string idCsv);
		IEnumerable<TEntity> GetByIDs(string[] ids);

		IEnumerable<TEntity> Search(string searchstring, string excludeValues);
	}
}
