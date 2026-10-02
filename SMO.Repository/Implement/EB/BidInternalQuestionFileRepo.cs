using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidInternalQuestionFileRepo : GenericRepository<T_EB_BID_INTERNAL_QUESTION_FILE>, IBidInternalQuestionFileRepo
    {
        public BidInternalQuestionFileRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
