using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class ActivityLevelReceiverComRepo : GenericRepository<T_WF_ACTIVITY_LEVEL_RECEIVER_COM>, IActivityLevelReceiverComRepo
    {
        public ActivityLevelReceiverComRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
