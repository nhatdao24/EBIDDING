using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ContractorFinanceRepo : GenericRepository<T_EB_CONTRACTOR_FINANCE>, IContractorFinanceRepo
    {
        public ContractorFinanceRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_CONTRACTOR_FINANCE> Search(T_EB_CONTRACTOR_FINANCE objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.CONTRACTOR_ID))
            {
                query = query.Where(x => x.CONTRACTOR_ID == objFilter.CONTRACTOR_ID);
            }

            total = 0;
            return query.ToList();
        }

        public override T_EB_CONTRACTOR_FINANCE Get(object id, dynamic param = null)
        {
            var query = NHibernateSession.QueryOver<T_EB_CONTRACTOR_FINANCE>();

            query = query.Where(x => x.PKID == id);
            query = query.Fetch(x => x.File).Eager;
            query = query.TransformUsing(Transformers.DistinctRootEntity);
            return query.SingleOrDefault();
        }
    }
}
