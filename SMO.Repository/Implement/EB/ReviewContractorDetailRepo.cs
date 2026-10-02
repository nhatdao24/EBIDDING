using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewContractorDetailRepo : GenericRepository<T_EB_REVIEW_CONTRACTOR_DETAIL>, IReviewContractorDetailRepo
    {
        public ReviewContractorDetailRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

    }
}
