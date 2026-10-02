using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewContractorRepo : GenericRepository<T_EB_REVIEW_CONTRACTOR>, IReviewContractorRepo
    {
        public ReviewContractorRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_REVIEW_CONTRACTOR> Search(T_EB_REVIEW_CONTRACTOR objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_REVIEW_CONTRACTOR>();

            if (!string.IsNullOrWhiteSpace(objFilter.REVIEW_ID))
            {
                query = query.Where(x => x.REVIEW_ID == objFilter.REVIEW_ID);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.CONTRACTOR_ID))
            {
                query = query.Where(x => x.CONTRACTOR_ID == objFilter.CONTRACTOR_ID);
            }

            query = query.Fetch(x => x.Contractor).Eager.Fetch(x => x.ListDetail).Eager;
            query = query.TransformUsing(Transformers.DistinctRootEntity);
            total = 0;
            return query.List();
        }
    }
}
