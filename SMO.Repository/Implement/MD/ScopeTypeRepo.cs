using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Repository.Implement.MD
{
    public class ScopeTypeRepo : GenericRepository<T_MD_SCOPE_TYPE>, IScopeTypeRepo
    {
        public ScopeTypeRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_MD_SCOPE_TYPE> Search(T_MD_SCOPE_TYPE objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.CODE))
            {
                query = query.Where(x => x.CODE.ToLower().Contains(objFilter.CODE.ToLower()) || x.TEXT.ToLower().Contains(objFilter.CODE.ToLower()));
            }

            query = query.OrderBy(x => x.TEXT);
            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }
    }
}
