using NHibernate;
using SMO.Core.Entities;
using SMO.Core.Entities.MD;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Repository.Implement.MD
{
    public class HhdvRepo : GenericRepository<T_MD_HHDV>, IHhdvRepo
    {
        public HhdvRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
        public override IList<T_MD_HHDV> Search(T_MD_HHDV objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.Query<T_MD_HHDV>();

            // Tìm kiếm theo CODE hoặc NAME
            if (!string.IsNullOrWhiteSpace(objFilter.CODE))
            {
                query = query.Where(x => x.CODE.Contains(objFilter.CODE) || x.NAME.Contains(objFilter.CODE));
            }

            // Sắp xếp theo CODE
            query = query.OrderBy(x => x.CODE);

            // Phân trang
            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }
    }
}
