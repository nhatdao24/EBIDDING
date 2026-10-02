using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewCouncilNoteRepo : GenericRepository<T_EB_REVIEW_COUNCIL_NOTE>, IReviewCouncilNoteRepo
    {
        public ReviewCouncilNoteRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
