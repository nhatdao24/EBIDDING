using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.RQ;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.RQ
{
    public class RequestToRepo : GenericRepository<T_RQ_REQUEST_TO>, IRequestToRepo
    {
        public RequestToRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        
    }
}
