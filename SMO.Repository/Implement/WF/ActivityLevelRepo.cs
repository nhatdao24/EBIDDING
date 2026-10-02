using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class ActivityLevelRepo : GenericRepository<T_WF_ACTIVITY_LEVEL>, IActivityLevelRepo
    {
        public ActivityLevelRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
