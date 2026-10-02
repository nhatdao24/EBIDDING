using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class ProjectRepo : GenericRepository<T_MD_PROJECT>, IProjectRepo
    {
        public ProjectRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_MD_PROJECT> Search(T_MD_PROJECT objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.CODE))
            {
                query = query.Where(x => x.NAME.ToLower().Contains(objFilter.CODE.ToLower()));
            }

            query = query.OrderByDescending(x => x.NAME);
            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }
    }
}
