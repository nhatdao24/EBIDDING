using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.AD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.AD
{
    public class ConnectionRepo : GenericRepository<T_AD_CONNECTION>, IConnectionRepo
    {
        public ConnectionRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
