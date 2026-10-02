using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewSpecScoreRepo : GenericRepository<T_EB_REVIEW_SPEC_SCORE>, IReviewSpecScoreRepo
    {
        public ReviewSpecScoreRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

    }
}
