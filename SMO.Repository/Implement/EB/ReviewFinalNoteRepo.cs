using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewFinalNoteRepo : GenericRepository<T_EB_REVIEW_FINAL_NOTE>, IReviewFinalNoteRepo
    {
        public ReviewFinalNoteRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
