using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidInternalAnswerRepo : GenericRepository<T_EB_BID_INTERNAL_ANSWER>, IBidInternalAnswerRepo
    {
        public BidInternalAnswerRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
