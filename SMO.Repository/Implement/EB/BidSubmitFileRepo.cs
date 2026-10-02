using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidSubmitFileRepo : GenericRepository<T_EB_BID_SUBMIT_FILE>, IBidSubmitFileRepo
    {
        public BidSubmitFileRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
