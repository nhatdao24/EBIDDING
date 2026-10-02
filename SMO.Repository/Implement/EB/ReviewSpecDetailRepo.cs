using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class ReviewSpecDetailRepo : GenericRepository<T_EB_REVIEW_SPEC_DETAIL>, IReviewSpecDetailRepo
    {
        public ReviewSpecDetailRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
