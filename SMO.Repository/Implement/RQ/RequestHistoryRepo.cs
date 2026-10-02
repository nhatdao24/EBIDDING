using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.RQ;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.RQ
{
    public class RequestHistoryRepo : GenericRepository<T_RQ_REQUEST_HISTORY>, IRequestHistoryRepo
    {
        public RequestHistoryRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        
    }
}
