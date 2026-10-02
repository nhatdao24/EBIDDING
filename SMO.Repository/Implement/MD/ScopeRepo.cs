using NHibernate.Linq;
using NHibernate.Criterion;
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
    public class ScopeRepo : GenericRepository<T_MD_SCOPE>, IScopeRepo
    {
        public ScopeRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_MD_SCOPE> Search(T_MD_SCOPE objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_MD_SCOPE>();

            if (!string.IsNullOrWhiteSpace(objFilter.CODE))
            {
                query = query.Where(x => x.CODE.IsLike(string.Format("%{0}%", objFilter.CODE)) || x.TEXT.IsLike(string.Format("%{0}%", objFilter.CODE)));
            }
            query = query.Fetch(x => x.SCOPE_TYPE).Eager;

            query = query.OrderBy(x => x.TYPE_CODE).Asc.ThenBy(m => m.TEXT).Asc;
            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }
    }
}
