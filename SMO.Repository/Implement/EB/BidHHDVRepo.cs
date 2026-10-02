using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidHHDVRepo : GenericRepository<T_EB_BID_HHDV>, IBidHHDVRepo
    {
        public BidHHDVRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
