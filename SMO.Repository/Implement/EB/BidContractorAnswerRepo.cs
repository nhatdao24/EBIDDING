using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidContractorAnswerRepo : GenericRepository<T_EB_BID_CONTRACTOR_ANSWER>, IBidContractorAnswerRepo
    {
        public BidContractorAnswerRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
