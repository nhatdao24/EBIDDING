using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class NumberOfProjectRepo : GenericRepository<T_MD_NUMBER_OF_PROJECT>, INumberOfProjectRepo
    {
        public NumberOfProjectRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
        public override IList<T_MD_NUMBER_OF_PROJECT> Search(T_MD_NUMBER_OF_PROJECT objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.CODE))
            {
                query = query.Where(x => x.CODE.ToLower().Contains(objFilter.CODE.ToLower()) || x.TEXT.ToLower().Contains(objFilter.CODE.ToLower()));
            }

            query = query.OrderBy(x => x.C_ORDER);
            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }
    }
}
