using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewSpecRepo : GenericRepository<T_EB_REVIEW_SPEC>, IReviewSpecRepo
    {
        public ReviewSpecRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_REVIEW_SPEC> Search(T_EB_REVIEW_SPEC objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_REVIEW_SPEC>();
            var subQuery = QueryOver.Of<T_EB_REVIEW_SPEC>();
            if (!string.IsNullOrWhiteSpace(objFilter.REVIEW_ID))
            {
                query = query.Where(x => x.REVIEW_ID == objFilter.REVIEW_ID);
                subQuery = subQuery.Where(x => x.REVIEW_ID == objFilter.REVIEW_ID);
            }

            var queryListDetail = NHibernateSession.QueryOver<T_EB_REVIEW_SPEC_DETAIL>()
                .WithSubquery.WhereProperty(x => x.HEADER_ID).In(
                    subQuery.Select(Projections.Property<T_EB_REVIEW_SPEC>(p => p.PKID))
                ).OrderBy(x => x.C_ORDER).Asc.Future();

            query = query.OrderBy(p => p.C_ORDER).Asc;
            var result = query.List();

            var lstDetail = queryListDetail.ToList();
            foreach (var item in result)
            {
                item.ListDetail = lstDetail.Where(x => x.HEADER_ID == item.PKID).ToList();
            }
            total = 0;
            return result;
        }

        public override T_EB_REVIEW_SPEC Get(object id, dynamic param = null)
        {
            var query = NHibernateSession.QueryOver<T_EB_REVIEW_SPEC>();

            query = query.Where(x => x.PKID == id).Fetch(x => x.ListDetail).Eager;

            return query.SingleOrDefault();
        }
    }
}
