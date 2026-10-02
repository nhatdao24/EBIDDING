using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidSpecContractorRepo : GenericRepository<T_EB_BID_SPEC_CONTRACTOR>, IBidSpecContractorRepo
    {
        public BidSpecContractorRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID_SPEC_CONTRACTOR> Search(T_EB_BID_SPEC_CONTRACTOR objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID_SPEC_CONTRACTOR>();
            if (!string.IsNullOrWhiteSpace(objFilter.BID_ID))
            {
                query = query.Where(x => x.BID_ID == objFilter.BID_ID);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.CONTRACTOR_ID))
            {
                var lstContractor = objFilter.CONTRACTOR_ID.Split(',').ToList();
                query = query.Where(x => x.CONTRACTOR_ID.IsIn(lstContractor));
            }

            var result = query.List().ToList();

            total = 0;
            return result;
        }
    }
}
