using NHibernate.Linq;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ContractorScopeRepo : GenericRepository<T_EB_CONTRACTOR_SCOPE>, IContractorScopeRepo
    {
        public ContractorScopeRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_CONTRACTOR_SCOPE> Search(T_EB_CONTRACTOR_SCOPE objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.CONTRACTOR_ID))
            {
                query = query.Where(x => x.CONTRACTOR_ID == objFilter.CONTRACTOR_ID);
            }

            query = query.Fetch(x => x.Scope);

            total = 0;
            return query.ToList();
        }
    }
}
