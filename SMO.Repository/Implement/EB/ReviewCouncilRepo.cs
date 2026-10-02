using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewCouncilRepo : GenericRepository<T_EB_REVIEW_COUNCIL>, IReviewCouncilRepo
    {
        public ReviewCouncilRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_REVIEW_COUNCIL> Search(T_EB_REVIEW_COUNCIL objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_REVIEW_COUNCIL>();

            if (!string.IsNullOrWhiteSpace(objFilter.REVIEW_ID))
            {
                query = query.Where(x => x.REVIEW_ID == objFilter.REVIEW_ID);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.USER_NAME))
            {
                query = query.Where(x => x.USER_NAME == objFilter.USER_NAME);
            }

            query = query.Fetch(x => x.User).Eager;

            total = 0;
            return query.List();
        }
    }
}
