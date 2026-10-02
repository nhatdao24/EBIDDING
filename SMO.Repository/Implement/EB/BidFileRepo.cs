using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidFileRepo : GenericRepository<T_EB_BID_FILE>, IBidFileRepo
    {
        public BidFileRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
