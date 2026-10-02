using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewFinalHistoryRepo : GenericRepository<T_EB_REVIEW_FINAL_HISTORY>, IReviewFinalHistoryRepo
    {
        public ReviewFinalHistoryRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

    }
}
