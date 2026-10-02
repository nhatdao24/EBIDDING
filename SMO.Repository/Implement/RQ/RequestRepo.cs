using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.RQ;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.RQ
{
    public class RequestRepo : GenericRepository<T_RQ_REQUEST>, IRequestRepo
    {
        public RequestRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        
    }
}
