using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidRequireRepo : GenericRepository<T_EB_BID_REQUIRE>, IBidRequireRepo
    {
        public BidRequireRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID_REQUIRE> Search(T_EB_BID_REQUIRE objFilter, int pageSize, int pageIndex, out int total)
        {
            int startIndex = pageSize * (pageIndex - 1);
            var query = NHibernateSession.QueryOver<T_EB_BID_REQUIRE>();
            var subQuery = QueryOver.Of<T_EB_BID_REQUIRE>();
            var subQueryUserApprove = QueryOver.Of<T_EB_BID_REQUIRE_USER>();
            if (!string.IsNullOrWhiteSpace(objFilter.NAME))
            {
                query = query.Where(x => x.CODE.IsLike(string.Format("%{0}%", objFilter.NAME)) || x.NAME.IsLike(string.Format("%{0}%", objFilter.NAME)));
                subQuery = subQuery.Where(x => x.CODE.IsLike(string.Format("%{0}%", objFilter.NAME)) || x.NAME.IsLike(string.Format("%{0}%", objFilter.NAME)));
            }

            if (!string.IsNullOrEmpty(objFilter.COMPANY_CODE))
            {
                query = query.Where(x => x.COMPANY_CODE == objFilter.COMPANY_CODE);
            }

            if (objFilter.CREATE_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE >= objFilter.CREATE_DATE.Value && x.CREATE_DATE < objFilter.CREATE_DATE.Value.AddDays(1));
                subQuery = subQuery.Where(x => x.CREATE_DATE >= objFilter.CREATE_DATE.Value && x.CREATE_DATE < objFilter.CREATE_DATE.Value.AddDays(1));
            }

            if (!string.IsNullOrEmpty(objFilter.USER_APPROVE))
            {
                var lstUserApprove = objFilter.USER_APPROVE.Split(',').ToList();
                subQueryUserApprove = subQueryUserApprove.Where(x => x.USER_NAME.IsIn(lstUserApprove));
                query = query.WithSubquery.WhereProperty(x => x.CODE).In(
                    subQueryUserApprove.Select(Projections.Property<T_EB_BID_REQUIRE_USER>(p => p.HEADER_CODE))
                );
                subQuery = subQuery.WithSubquery.WhereProperty(x => x.CODE).In(
                    subQueryUserApprove.Select(Projections.Property<T_EB_BID_REQUIRE_USER>(p => p.HEADER_CODE))
                );
            }

            if (!string.IsNullOrEmpty(objFilter.CREATE_BY))
            {
                query = query.Where(x => x.CREATE_BY == objFilter.CREATE_BY);
                subQuery = subQuery.Where(x => x.CREATE_BY == objFilter.CREATE_BY);
            }

            query = query.Where(x => x.PROJECT_CODE.IsIn(objFilter.ListFilterProject));
            subQuery = subQuery.Where(x => x.PROJECT_CODE.IsIn(objFilter.ListFilterProject));

            var queryListDetail = NHibernateSession.QueryOver<T_EB_BID_REQUIRE_DETAIL>()
                .WithSubquery.WhereProperty(x => x.HEADER_CODE).In(
                    subQuery.Select(Projections.Property<T_EB_BID_REQUIRE>(p => p.CODE))
                    .OrderBy(p => p.CREATE_DATE).Desc
                    .Take(pageSize).Skip(startIndex))
                   .Fetch(x => x.Material).Eager
                   .Fetch(x => x.Unit).Eager
                   .Future();

            query = query.Fetch(x => x.UserApprove).Eager.OrderBy(p => p.CREATE_DATE).Desc.TransformUsing(new DistinctRootEntityResultTransformer());
            var result = base.Paging(query, pageSize, pageIndex, out total).ToList();
            var lstDetail = queryListDetail.ToList();
            foreach (var item in result)
            {
                item.ListDetail = lstDetail.Where(x => x.HEADER_CODE == item.CODE).ToList();
            }
            return result;
        }

        public IList<T_EB_BID_REQUIRE> Search_ProcessByMe(T_EB_BID_REQUIRE objFilter, int pageSize, int pageIndex, out int total)
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

            var subQuery = NHibernateSession.QueryOver<T_EB_BID_REQUIRE_USER>();

            subQuery = subQuery.Where(x => x.USER_NAME == objFilter.CREATE_BY);
            var lstReviewFinalUser = subQuery.List().Select(x => x.HEADER_CODE).ToList();

            query = query.Where(x => lstReviewFinalUser.Contains(x.CODE));
            query = query.Where(x => objFilter.ListFilterProject.Contains(x.PROJECT_CODE));

            query = query.OrderByDescending(x => x.CREATE_DATE);
            return base.Paging(query, pageSize, pageIndex, out total);
        }

        public override T_EB_BID_REQUIRE Get(object id, dynamic param = null)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID_REQUIRE>();

            query = query.Where(x => x.CODE == id);

            if (UtilsRepo.IsPropertyExist(param, "IsFetch"))
            {

                var queryListDetail = NHibernateSession.QueryOver<T_EB_BID_REQUIRE_DETAIL>()
                   .Where(x => x.HEADER_CODE == id)
                   .Fetch(x => x.Material).Eager
                   .Fetch(x => x.Scope).Eager
                   .Fetch(x => x.RentUnit).Eager
                   .Fetch(x => x.Unit).Eager
                   .Future();

                var queryListPR = NHibernateSession.QueryOver<T_EB_BID_REQUIRE_PR>()
                   .Where(x => x.HEADER_CODE == id)
                   .Future();

                var resultFetch = query.Fetch(x => x.UserSent).Eager
                    .Fetch(x => x.UserApprove).Eager
                    .Future().FirstOrDefault();
                resultFetch.ListDetail = queryListDetail.ToList();
                resultFetch.ListPR = queryListPR.ToList();
                return resultFetch;
            }
            return query.Fetch(x => x.UserApprove).Eager.SingleOrDefault();
        }

    }
}
