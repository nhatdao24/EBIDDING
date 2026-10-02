using NHibernate.Linq;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ContratorRevenueRepo : GenericRepository<T_EB_CONTRACTOR_REVENUE>, IContractorRevenueRepo
    {
        public ContratorRevenueRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_CONTRACTOR_REVENUE> Search(T_EB_CONTRACTOR_REVENUE objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.CONTRACTOR_ID))
            {
                query = query.Where(x => x.CONTRACTOR_ID == objFilter.CONTRACTOR_ID);
            }

            total = 0;
            return query.ToList();
        }
    }
}
