using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidInternalAnswerFileRepo : GenericRepository<T_EB_BID_INTERNAL_ANSWER_FILE>, IBidInternalAnswerFileRepo
    {
        public BidInternalAnswerFileRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
