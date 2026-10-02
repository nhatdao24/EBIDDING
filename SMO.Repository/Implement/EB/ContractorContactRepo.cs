using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ContractorContactRepo : GenericRepository<T_EB_CONTRACTOR_CONTACT>, IContractorContactRepo
    {
        public ContractorContactRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_CONTRACTOR_CONTACT> Search(T_EB_CONTRACTOR_CONTACT objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.CONTRACTOR_ID))
            {
                query = query.Where(x => x.CONTRACTOR_ID == objFilter.CONTRACTOR_ID);
            }

            return base.Paging(query, pageSize, pageIndex, out total);
        }
    }
}
