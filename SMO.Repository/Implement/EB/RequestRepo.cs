using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class RequestRepo : GenericRepository<T_EB_REQUEST>, IRequestRepo
    {
        public RequestRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_REQUEST> Search(T_EB_REQUEST objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_REQUEST>();

            if (!string.IsNullOrWhiteSpace(objFilter.MA_SO_THUE))
            {
                query = query.Where(x => x.MA_SO_THUE.IsLike(string.Format("%{0}%", objFilter.MA_SO_THUE)) || x.TEN_DAI.IsLike(string.Format("%{0}%", objFilter.MA_SO_THUE)) || x.TEN_NGAN.IsLike(string.Format("%{0}%", objFilter.MA_SO_THUE)) || x.DMS.IsLike(string.Format("%{0}%", objFilter.MA_SO_THUE)));
            }

            if (!string.IsNullOrWhiteSpace(objFilter.STATUS))
            {
                query = query.Where(x => x.STATUS == objFilter.STATUS);
            }
            query = query.Fetch(x => x.ListRevenue).Eager
                .Fetch(x => x.Country).Eager
                .Fetch(x => x.NumberOfProject).Eager
                .Fetch(x => x.Option).Eager
                .Fetch(x => x.ListScope).Eager.Fetch(x => x.ListScope.First().Scope).Eager.TransformUsing(Transformers.DistinctRootEntity);
            query = query.OrderBy(x => x.CREATE_DATE).Desc;
            return base.Paging(query, pageSize, pageIndex, out total);
        }
    }
}
