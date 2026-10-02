using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class PREBRepo : GenericRepository<T_PR_EB_HEADER>, IPREBRepo
    {
        public PREBRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_PR_EB_HEADER> Search(T_PR_EB_HEADER objFilter, int pageSize, int pageIndex, out int total)
        {
            int startIndex = pageSize * (pageIndex - 1);
            var query = NHibernateSession.QueryOver<T_PR_EB_HEADER>();
            var subQuery = QueryOver.Of<T_PR_EB_HEADER>();
            if (!string.IsNullOrWhiteSpace(objFilter.CODE))
            {
                query = query.Where(x => x.CODE.IsLike(string.Format("%{0}%", objFilter.CODE)) || x.REQUIRE_NAME.IsLike(string.Format("%{0}%", objFilter.CODE)));
                subQuery = subQuery.Where(x => x.CODE.IsLike(string.Format("%{0}%", objFilter.CODE)) || x.REQUIRE_NAME.IsLike(string.Format("%{0}%", objFilter.CODE)));
            }

            if (!string.IsNullOrEmpty(objFilter.COMPANY_CODE))
            {
                query = query.Where(x => x.COMPANY_CODE == objFilter.COMPANY_CODE);
            }

            if (objFilter.REQUIRE_DATE.HasValue)
            {
                query = query.Where(x => x.REQUIRE_DATE == objFilter.REQUIRE_DATE.Value);
                subQuery = subQuery.Where(x => x.REQUIRE_DATE == objFilter.REQUIRE_DATE.Value);
            }

            if (!string.IsNullOrEmpty(objFilter.USER_APPROVE))
            {
                query = query.Where(x => x.USER_APPROVE == objFilter.USER_APPROVE);
                subQuery = subQuery.Where(x => x.USER_APPROVE == objFilter.USER_APPROVE);
            }
            
            if (!string.IsNullOrEmpty(objFilter.PROJECT_CODE))
            {
                query = query.Where(x => x.PROJECT_CODE == objFilter.PROJECT_CODE);
                subQuery = subQuery.Where(x => x.PROJECT_CODE == objFilter.PROJECT_CODE);
            }

            if (!string.IsNullOrEmpty(objFilter.PACKAGE_CODE))
            {
                query = query.Where(x => x.PACKAGE_CODE == objFilter.PACKAGE_CODE);
                subQuery = subQuery.Where(x => x.PACKAGE_CODE == objFilter.PACKAGE_CODE);
            }

            if (!string.IsNullOrEmpty(objFilter.SCOPE_TYPE_CODE))
            {
                query = query.Where(x => x.SCOPE_TYPE_CODE == objFilter.SCOPE_TYPE_CODE);
                subQuery = subQuery.Where(x => x.SCOPE_TYPE_CODE == objFilter.SCOPE_TYPE_CODE);
            }

            if (!string.IsNullOrEmpty(objFilter.BID_TYPE_CODE))
            {
                query = query.Where(x => x.BID_TYPE_CODE == objFilter.BID_TYPE_CODE);
                subQuery = subQuery.Where(x => x.BID_TYPE_CODE == objFilter.BID_TYPE_CODE);
            }

            if (!string.IsNullOrEmpty(objFilter.CREATE_BY))
            {
                query = query.Where(x => x.CREATE_BY == objFilter.CREATE_BY);
                subQuery = subQuery.Where(x => x.CREATE_BY == objFilter.CREATE_BY);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.STATUS))
            {
                query = query.Where(x => x.STATUS == objFilter.STATUS);
                subQuery = subQuery.Where(x => x.STATUS == objFilter.STATUS);
            }

            query = query.Where(x => x.PROJECT_CODE.IsIn(objFilter.ListFilterProject));
            subQuery = subQuery.Where(x => x.PROJECT_CODE.IsIn(objFilter.ListFilterProject));

            var copySubQuery = subQuery.Clone();


            var queryListDetail = NHibernateSession.QueryOver<T_PR_EB_DETAIL>()
                .WithSubquery.WhereProperty(x => x.HEADER_CODE).In(
                    subQuery.Select(Projections.Property<T_PR_EB_HEADER>(p => p.CODE))
                    .OrderBy(p => p.REQUIRE_DATE).Desc
                    .Take(pageSize).Skip(startIndex))
                   .Fetch(x => x.Material).Eager
                   .Fetch(x => x.Unit).Eager
                   .Future();

            var queryListBidRequirePR = NHibernateSession.QueryOver<T_EB_BID_REQUIRE_PR>()
                .WithSubquery.WhereProperty(x => x.PR_CODE).In(
                    copySubQuery.Select(Projections.Property<T_PR_EB_HEADER>(p => p.CODE))
                    .OrderBy(p => p.REQUIRE_DATE).Desc
                    .Take(pageSize).Skip(startIndex))
                   .Future();

            query = query.OrderBy(p => p.CREATE_DATE).Desc.TransformUsing(Transformers.DistinctRootEntity);
            var result = base.Paging(query, pageSize, pageIndex, out total).ToList();

            var lstDetail = queryListDetail.ToList();
            var lstBidRequirePR = queryListBidRequirePR.ToList();
            var listAddCodePR = objFilter.ListPrForAdd;

            if (listAddCodePR != null && listAddCodePR.Any())
            {
                result = result.Where(item => !listAddCodePR.Contains(item.CODE)).ToList();
            }

            foreach (var item in result)
            {
                item.ListDetail = lstDetail.Where(x => x.HEADER_CODE == item.CODE).ToList();
                item.ListBidRequirePR = lstBidRequirePR.Where(x => x.PR_CODE == item.CODE).ToList();
            }

            return result;
        }
         
        public override T_PR_EB_HEADER Get(object id, dynamic param = null)
        {
            var query = NHibernateSession.QueryOver<T_PR_EB_HEADER>();
            query = query.Where(x => x.CODE == id);
            query = query.Fetch(x => x.UserSent).Eager
                .Fetch(x => x.UserApprove).Eager
                .Fetch(x => x.Package).Eager
                .Fetch(x => x.Project).Eager
                .Fetch(x => x.Scope).Eager
                .Fetch(x => x.ScopeType).Eager
                .Fetch(x => x.BidType).Eager
                .Fetch(x => x.ListDetail).Eager
                .Fetch(x => x.ListDetail.First().Scope).Eager
                .Fetch(x => x.ListDetail.First().RentUnit).Eager
                .Fetch(x => x.ListDetail.First().Material).Eager;
            query = query.TransformUsing(Transformers.DistinctRootEntity);

            return query.SingleOrDefault();
        }
    }
}
