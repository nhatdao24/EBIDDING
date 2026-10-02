using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidContractorRepo : GenericRepository<T_EB_BID_CONTRACTOR>, IBidContractorRepo
    {
        public BidContractorRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID_CONTRACTOR> Search(T_EB_BID_CONTRACTOR objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID_CONTRACTOR>();

            if (!string.IsNullOrWhiteSpace(objFilter.BID_ID))
            {
                query = query.Where(x => x.BID_ID == objFilter.BID_ID);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.CONTRACTOR_ID))
            {
                query = query.Where(x => x.CONTRACTOR_ID == objFilter.CONTRACTOR_ID);
            }

            query = query.Fetch(x => x.Contractor).Eager.Fetch(x => x.EmailSent).Eager;

            total = 0;
            return query.List();
        }
    }
}
