using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidContractorQuestionFileRepo : GenericRepository<T_EB_BID_CONTRACTOR_QUESTION_FILE>, IBidContractorQuestionFileRepo
    {
        public BidContractorQuestionFileRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
