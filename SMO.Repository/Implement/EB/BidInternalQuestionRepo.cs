using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidInternalQuestionRepo : GenericRepository<T_EB_BID_INTERNAL_QUESTION>, IBidInternalQuestionRepo
    {
        public BidInternalQuestionRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID_INTERNAL_QUESTION> Search(T_EB_BID_INTERNAL_QUESTION objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID_INTERNAL_QUESTION>();

            if (!string.IsNullOrWhiteSpace(objFilter.BID_ID))
            {
                query = query.Where(x => x.BID_ID == objFilter.BID_ID);
            }

            if (objFilter.IS_BID_CLOSE.HasValue)
            {
                query = query.Where(x => x.IS_BID_CLOSE == objFilter.IS_BID_CLOSE);
            }
            //query = query.Fetch(x => x.ListAnswer).Eager
            //    .Fetch(x => x.ListFile).Eager;

            query = query.Fetch(x => x.ListAnswer).Eager
                .Fetch(x => x.ListAnswer.First().ListFile).Eager
                .Fetch(x => x.ListAnswer.First().ListFile.First().FileUpload).Eager
                .Fetch(x => x.ListFile).Eager
                .Fetch(x => x.ListFile.First().FileUpload).Eager;

            query = query.TransformUsing(Transformers.DistinctRootEntity);
            total = 0;
            return query.List().ToList();
        }
    }
}
