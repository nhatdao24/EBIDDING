using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewFinalContractorDetailRepo : GenericRepository<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL>, IReviewFinalContractorDetailRepo
    {
        public ReviewFinalContractorDetailRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

    }
}
