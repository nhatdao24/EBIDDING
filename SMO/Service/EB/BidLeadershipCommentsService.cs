using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Service.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using NHibernate.Criterion;
using SMO.Service.CM;
using NHibernate.Linq;
using SMO.Repository.Implement.CM;

namespace SMO.Service.EB
{
    public class BidLeadershipCommentsService : GenericService<T_EB_BID_LEADERSHIP_COMMENTS, BidLeadershipCommentsRepo>
    {
        public BidLeadershipCommentsService() : base()
        {
        }

    }
}
