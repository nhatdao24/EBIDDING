using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class ActivityLevelSenderRepo : GenericRepository<T_WF_ACTIVITY_LEVEL_SENDER>, IActivityLevelSenderRepo
    {
        public ActivityLevelSenderRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
