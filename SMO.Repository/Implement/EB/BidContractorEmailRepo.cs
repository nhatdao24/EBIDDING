using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidContractorEmailRepo : GenericRepository<T_EB_BID_CONTRACTOR_EMAIL>, IBidContractorEmailRepo
    {
        public BidContractorEmailRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID_CONTRACTOR_EMAIL> Search(T_EB_BID_CONTRACTOR_EMAIL objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID_CONTRACTOR_EMAIL>();

            if (!string.IsNullOrWhiteSpace(objFilter.BID_ID))
            {
                query = query.Where(x => x.BID_ID == objFilter.BID_ID &&  x.TYPE == objFilter.TYPE);
            }
         

            query = query
                .Fetch(x => x.Contractor).Eager
                .Fetch(x => x.Email).Eager;

            query = query.TransformUsing(Transformers.DistinctRootEntity);
            total = 0;
            return query.List();
        }
    }
}
