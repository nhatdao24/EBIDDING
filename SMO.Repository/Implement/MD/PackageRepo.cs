using System.Collections.Generic;
using System.Linq;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;

namespace SMO.Repository.Implement.MD
{
    public class PackageRepo :GenericRepository<T_MD_PACKAGE>, IPackageRepo
    {
        public PackageRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
        public override IList<T_MD_PACKAGE> Search(T_MD_PACKAGE objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.CODE))
            {
                query = query.Where(x => x.TEXT.ToLower().Contains(objFilter.CODE.ToLower()));
            }

            query = query.OrderByDescending(x => x.TEXT);
            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }
    }
}
