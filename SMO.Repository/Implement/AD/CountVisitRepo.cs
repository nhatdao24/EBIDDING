using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMO.Core.Entities;
using SMO.Core.Entities.AD;
using SMO.Repository.Common;
using SMO.Repository.Interface.AD;
using NHibernate.Criterion;

namespace SMO.Repository.Implement.AD
{
    public class CountVisitRepo : GenericRepository<T_AD_COUNT_VISIT>, ICountVisitRepo
    {
        public CountVisitRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_AD_COUNT_VISIT> Search(T_AD_COUNT_VISIT objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.ACCESS_TYPE))
            {
                query = query.Where(x => x.ACCESS_TYPE.Contains(objFilter.ACCESS_TYPE));
            }

            if (objFilter != null && objFilter.CREATE_DATE.HasValue)
            {
                var startDate = objFilter.CREATE_DATE.Value.Date;
                var endDate = startDate.AddDays(1);
                query = query.Where(x => x.CREATE_DATE >= startDate && x.CREATE_DATE < endDate);
            }
            if (!string.IsNullOrWhiteSpace(objFilter.CREATE_BY))
            {
                query = query.Where(x => x.CREATE_BY == objFilter.CREATE_BY);
            }
            if (!string.IsNullOrWhiteSpace(objFilter.VIEW_TYPE))
            {
                query = query.Where(x => x.VIEW_TYPE == objFilter.VIEW_TYPE);
            }

            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }
    }
}
