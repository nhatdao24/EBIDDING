using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidContractorPriceRepo : GenericRepository<T_EB_BID_CONTRACTOR_PRICE>, IBidContractorPriceRepo
    {
        public BidContractorPriceRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID_CONTRACTOR_PRICE> Search(T_EB_BID_CONTRACTOR_PRICE objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.BID_ID))
            {
                query = query.Where(x => x.BID_ID == objFilter.BID_ID);
            }

            query = query.OrderBy(x => x.C_ORDER);
            total = 0;
            return query.ToList();
        }
    }
}
