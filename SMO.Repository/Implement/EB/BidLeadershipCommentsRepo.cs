using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidLeadershipCommentsRepo : GenericRepository<T_EB_BID_LEADERSHIP_COMMENTS>, IBidLeadershipCommentsRepo
    {
        public BidLeadershipCommentsRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
