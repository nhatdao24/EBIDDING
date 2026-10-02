using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class ReviewRatingRepo : GenericRepository<T_EB_REVIEW_RATING>, IReviewRatingRepo
    {
        public ReviewRatingRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
        public override IList<T_EB_REVIEW_RATING> Search(T_EB_REVIEW_RATING objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.REVIEW_ID))
            {
                query = query.Where(x => x.REVIEW_ID.ToLower().Contains(objFilter.REVIEW_ID.ToLower()));
            }

            query = query.OrderBy(x => x.C_ORDER);
            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }
    }
}
