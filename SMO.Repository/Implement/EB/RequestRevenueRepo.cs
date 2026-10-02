using NHibernate.Linq;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class RequestRevenueRepo : GenericRepository<T_EB_REQUEST_REVENUE>, IRequestRevenueRepo
    {
        public RequestRevenueRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_REQUEST_REVENUE> Search(T_EB_REQUEST_REVENUE objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.REQUEST_ID))
            {
                query = query.Where(x => x.REQUEST_ID == objFilter.REQUEST_ID);
            }

            total = 0;
            return query.ToList();
        }
    }
}
