using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class PRSAPRepo : GenericRepository<T_PR_SAP_HEADER>, IPRSAPRepo
    {
        public PRSAPRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_PR_SAP_HEADER> Search(T_PR_SAP_HEADER objFilter, int pageSize, int pageIndex, out int total)
        {
            int startIndex = pageSize * (pageIndex - 1);
            var query = NHibernateSession.QueryOver<T_PR_SAP_HEADER>();
            var subQuery = QueryOver.Of<T_PR_SAP_HEADER>();
            if (!string.IsNullOrWhiteSpace(objFilter.CODE))
            {
                query = query.Where(x => x.CODE.IsLike(string.Format("%{0}%", objFilter.CODE)) 
                    || x.REQUIRE_NAME.IsLike(string.Format("%{0}%", objFilter.CODE))
                    || x.COMPANY_NAME.IsLike(string.Format("%{0}%", objFilter.CODE))
                    || x.DEPARTMENT_NAME.IsLike(string.Format("%{0}%", objFilter.CODE))
                    || x.REQUIRE_USER.IsLike(string.Format("%{0}%", objFilter.CODE)));

                subQuery = subQuery.Where(x => x.CODE.IsLike(string.Format("%{0}%", objFilter.CODE))
                    || x.REQUIRE_NAME.IsLike(string.Format("%{0}%", objFilter.CODE))
                    || x.COMPANY_NAME.IsLike(string.Format("%{0}%", objFilter.CODE))
                    || x.DEPARTMENT_NAME.IsLike(string.Format("%{0}%", objFilter.CODE))
                    || x.REQUIRE_USER.IsLike(string.Format("%{0}%", objFilter.CODE)));
            }

            if (objFilter.REQUIRE_DATE.HasValue)
            {
                query = query.Where(x => x.REQUIRE_DATE == objFilter.REQUIRE_DATE.Value);
                subQuery = subQuery.Where(x => x.REQUIRE_DATE == objFilter.REQUIRE_DATE.Value);
            }

            var copySubQuery = subQuery.Clone();

            var queryListDetail = NHibernateSession.QueryOver<T_PR_SAP_DETAIL>()
                .WithSubquery.WhereProperty(x => x.HEADER_CODE).In(
                    subQuery.Select(Projections.Property<T_PR_SAP_HEADER>(p => p.CODE ))
                    .OrderBy(p => p.REQUIRE_DATE).Desc
                    .Take(pageSize).Skip(startIndex))
                   .Future();

            var queryListBidRequirePR = NHibernateSession.QueryOver<T_EB_BID_REQUIRE_PR>()
                .WithSubquery.WhereProperty(x => x.PR_CODE).In(
                    copySubQuery.Select(Projections.Property<T_PR_SAP_HEADER>(p => p.CODE))
                    .OrderBy(p => p.REQUIRE_DATE).Desc
                    .Take(pageSize).Skip(startIndex))
                   .Future();

            query = query.OrderBy(x => x.REQUIRE_DATE).Desc;
            var result = base.Paging(query, pageSize, pageIndex, out total).ToList();
            var lstDetail = queryListDetail.ToList();
            var lstBidRequirePR = queryListBidRequirePR.ToList();
            foreach (var item in result)
            {
                item.ListDetail = lstDetail.Where(x => x.HEADER_CODE == item.CODE).ToList();
                item.ListBidRequirePR = lstBidRequirePR.Where(x => x.PR_CODE == item.CODE).ToList();
            }
            return result;
        }

        public override T_PR_SAP_HEADER Get(object id, dynamic param = null)
        {
            var query = NHibernateSession.QueryOver<T_PR_SAP_HEADER>();

            query = query.Where(x => x.CODE == id);

            query = query.Fetch(x => x.ListDetail).Eager;

            query = query.TransformUsing(new DistinctRootEntityResultTransformer());
            return query.List().FirstOrDefault();
        }
    }
}
