using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ReviewRepo : GenericRepository<T_EB_REVIEW>, IReviewRepo
    {
        public ReviewRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_REVIEW> Search(T_EB_REVIEW objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = Queryable();

            if (!string.IsNullOrWhiteSpace(objFilter.DESCRIPTION))
            {
                query = query.Where(x => x.DESCRIPTION.Contains(objFilter.DESCRIPTION));
            }

            if (!string.IsNullOrWhiteSpace(objFilter.ORG_CODE))
            {
                query = query.Where(x => x.ORG_CODE == objFilter.ORG_CODE);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.PROJECT_CODE))
            {
                query = query.Where(x => x.PROJECT_CODE == objFilter.PROJECT_CODE);
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

        public IList<T_EB_REVIEW> Search_ProcessByMe(T_EB_REVIEW objFilter, int pageSize, int pageIndex, out int total)
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

            if (!string.IsNullOrWhiteSpace(objFilter.ORG_CODE))
            {
                query = query.Where(x => x.ORG_CODE == objFilter.ORG_CODE);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.PROJECT_CODE))
            {
                query = query.Where(x => x.PROJECT_CODE == objFilter.PROJECT_CODE);
            }

            if (objFilter.FROM_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE >= objFilter.FROM_DATE.Value);
            }

            if (objFilter.TO_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE < objFilter.TO_DATE.Value.AddDays(1));
            }

            var subQuery = NHibernateSession.QueryOver<T_EB_REVIEW_COUNCIL>();

            subQuery = subQuery.Where(x => x.USER_NAME == objFilter.CREATE_BY);
            var lstReviewOfCouncil = subQuery.List().Select(x => x.REVIEW_ID).ToList();

            query = query.Where(x => lstReviewOfCouncil.Contains(x.CODE));

            query = query.OrderByDescending(x => x.CREATE_DATE);
            return base.Paging(query, pageSize, pageIndex, out total);
        }

        public override T_EB_REVIEW Get(object id, dynamic param = null)
        {
            var query = NHibernateSession.QueryOver<T_EB_REVIEW>();

            query = query.Where(x => x.CODE == id);

            //if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListCouncil"))
            //{
            //    query = query.Fetch(x => x.ListCouncil).Eager
            //        .Fetch(x => x.ListCouncil.First().User).Eager;
            //}

            //if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListContractor"))
            //{
            //    query = query.Fetch(x => x.ListContractor).Eager
            //        .Fetch(x => x.ListContractor.First().Contractor).Eager
            //        .Fetch(x => x.ListContractor.First().ListDetail).Eager;
            //}

            query = query.TransformUsing(Transformers.DistinctRootEntity);
            return query.List().FirstOrDefault();
        }

    }
}
