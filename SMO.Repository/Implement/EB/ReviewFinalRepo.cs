using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewFinalRepo : GenericRepository<T_EB_REVIEW_FINAL>, IReviewFinalRepo
    {
        public ReviewFinalRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_REVIEW_FINAL> Search(T_EB_REVIEW_FINAL objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.DESCRIPTION))
            {
                query = query.Where(x => x.DESCRIPTION.Contains(objFilter.DESCRIPTION));
            }

            if (!string.IsNullOrWhiteSpace(objFilter.CREATE_BY))
            {
                query = query.Where(x => x.CREATE_BY == objFilter.CREATE_BY);
            }
            if (!string.IsNullOrWhiteSpace(objFilter.STATUS))
            {
                if (objFilter.STATUS == "01")
                {
                    query = query.Where(x => x.STATUS != "02");
                }
                else
                {
                    query = query.Where(x => x.STATUS == objFilter.STATUS);
                }
            }
            if (objFilter.FROM_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE >= objFilter.FROM_DATE.Value);
            }

            if (objFilter.TO_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE < objFilter.TO_DATE.Value.AddDays(1));
            }

            query = query.OrderByDescending(x => x.CREATE_DATE);
            return base.Paging(query, pageSize, pageIndex, out total);
        }

        public IList<T_EB_REVIEW_FINAL> Search_ProcessByMe(T_EB_REVIEW_FINAL objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.DESCRIPTION))
            {
                query = query.Where(x => x.DESCRIPTION.Contains(objFilter.DESCRIPTION));
            }
            if (!string.IsNullOrWhiteSpace(objFilter.STATUS))
            {
                query = query.Where(x => x.STATUS == objFilter.STATUS);
            }
            
            if (objFilter.FROM_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE >= objFilter.FROM_DATE.Value);
            }

            if (objFilter.TO_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE < objFilter.TO_DATE.Value.AddDays(1));
            }

            var subQuery = NHibernateSession.QueryOver<T_EB_REVIEW_FINAL_USER>();

            subQuery = subQuery.Where(x => x.USER_NAME == objFilter.CREATE_BY);
            var lstReviewFinalUser = subQuery.List().Select(x => x.REVIEW_ID).ToList();

            query = query.Where(x => lstReviewFinalUser.Contains(x.CODE));

            query = query.OrderByDescending(x => x.CREATE_DATE);
            return base.Paging(query, pageSize, pageIndex, out total);
        }

        public override T_EB_REVIEW_FINAL Get(object id, dynamic param = null)
        {
            var query = NHibernateSession.QueryOver<T_EB_REVIEW_FINAL>();

            query = query.Where(x => x.CODE == id);

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListContractor"))
            {
                query = query.Fetch(x => x.ListContractor).Eager
                    .Fetch(x => x.ListContractor.First().Contractor).Eager
                    .Fetch(x => x.ListContractor.First().ListDetail).Eager;
            }

            query = query.TransformUsing(new DistinctRootEntityResultTransformer());
            return query.List().FirstOrDefault();
        }

    }
}
