using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class ActivityLevelReceiverRepo : GenericRepository<T_WF_ACTIVITY_LEVEL_RECEIVER>, IActivityLevelReceiverRepo
    {
        public ActivityLevelReceiverRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
