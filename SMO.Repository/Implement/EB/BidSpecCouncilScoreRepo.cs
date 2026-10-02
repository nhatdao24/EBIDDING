using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidSpecCouncilScoreRepo : GenericRepository<T_EB_BID_SPEC_COUNCIL_SCORE>, IBidSpecCouncilScoreRepo
    {
        public BidSpecCouncilScoreRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID_SPEC_COUNCIL_SCORE> Search(T_EB_BID_SPEC_COUNCIL_SCORE objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID_SPEC_COUNCIL_SCORE>();
            if (!string.IsNullOrWhiteSpace(objFilter.BID_ID))
            {
                query = query.Where(x => x.BID_ID == objFilter.BID_ID);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.CONTRACTOR_ID))
            {
                var lstContractor = objFilter.CONTRACTOR_ID.Split(',').ToList();
                query = query.Where(x => x.CONTRACTOR_ID.IsIn(lstContractor));
            }

            if (!string.IsNullOrWhiteSpace(objFilter.USER_SCORE))
            {
                query = query.Where(x => x.USER_SCORE == objFilter.USER_SCORE);
            }


            var result = query.List();
            total = 0;
            return result;
        }
    }
}
