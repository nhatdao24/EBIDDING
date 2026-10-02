using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class MdReviewSpecDetailRepo : GenericRepository<T_MD_REVIEW_SPEC_DETAIL>, IMdReviewSpecDetailRepo
    {
        public MdReviewSpecDetailRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
