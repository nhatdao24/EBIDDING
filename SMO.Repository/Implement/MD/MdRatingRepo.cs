using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class MdRatingRepo : GenericRepository<T_MD_RATING>, IMdRatingRepo
    {
        public MdRatingRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
        public override IList<T_MD_RATING> Search(T_MD_RATING objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.PKID))
            {
                query = query.Where(x => x.RATING.ToLower().Contains(objFilter.RATING.ToLower()));
            }

            query = query.OrderBy(x => x.CREATE_DATE);
            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }
    }
}
