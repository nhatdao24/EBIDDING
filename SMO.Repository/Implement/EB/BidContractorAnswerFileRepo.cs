using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidContractorAnswerFileRepo : GenericRepository<T_EB_BID_CONTRACTOR_ANSWER_FILE>, IBidContractorAnswerFileRepo
    {
        public BidContractorAnswerFileRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
