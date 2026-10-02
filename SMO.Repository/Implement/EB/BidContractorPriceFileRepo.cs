using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidContractorPriceFileRepo : GenericRepository<T_EB_BID_CONTRACTOR_PRICE_FILE>, IBidContractorPriceFileRepo
    {
        public BidContractorPriceFileRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID_CONTRACTOR_PRICE_FILE> Search(T_EB_BID_CONTRACTOR_PRICE_FILE objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.BID_ID))
            {
                query = query.Where(x => x.BID_ID == objFilter.BID_ID);
            }

            query = query.OrderBy(x => x.CREATE_DATE);
            total = 0;
            return query.ToList();
        }
    }
}
