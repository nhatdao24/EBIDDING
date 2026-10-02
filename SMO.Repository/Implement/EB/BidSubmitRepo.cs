using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidSubmitRepo : GenericRepository<T_EB_BID_SUBMIT>, IBidSubmitRepo
    {
        public BidSubmitRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID_SUBMIT> Search(T_EB_BID_SUBMIT objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID_SUBMIT>();

            if (!string.IsNullOrWhiteSpace(objFilter.BID_ID))
            {
                query = query.Where(x => x.BID_ID == objFilter.BID_ID);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.CONTRACTOR_ID))
            {
                query = query.Where(x => x.CONTRACTOR_ID == objFilter.CONTRACTOR_ID);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.TYPE))
            {
                query = query.Where(x => x.TYPE == objFilter.TYPE);
            }

            //query = query.Fetch(x => x.ListAnswer).Eager
            //    .Fetch(x => x.ListFile).Eager;

            query = query.Fetch(x => x.ListFile).Eager
                .Fetch(x => x.ListFile.First().FileUpload).Eager;

            query = query.TransformUsing(Transformers.DistinctRootEntity);
            total = 0;
            return query.List().ToList();
        }
    }
}
