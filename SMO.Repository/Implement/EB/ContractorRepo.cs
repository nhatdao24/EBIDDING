using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class ContractorRepo : GenericRepository<T_EB_CONTRACTOR>, IContractorRepo
    {
        public ContractorRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_CONTRACTOR> Search(T_EB_CONTRACTOR objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_CONTRACTOR>();
            var subQueryMarket = QueryOver.Of<T_EB_CONTRACTOR_MARKET>();
            var subQueryProduct = QueryOver.Of<T_EB_CONTRACTOR_PRODUCT>();
            var subQueryScope = QueryOver.Of<T_EB_CONTRACTOR_SCOPE>();

            if (!string.IsNullOrWhiteSpace(objFilter.USER_NAME))
            {
                query = query.Where(x => x.USER_NAME.IsLike($"%{objFilter.USER_NAME}%")
                    || x.TEN_DAI.IsLike($"%{objFilter.USER_NAME}%")
                    || x.TEN_NGAN.IsLike($"%{objFilter.USER_NAME}%")
                    || x.MA_SO_THUE.IsLike($"%{objFilter.USER_NAME}%")
                    || x.DMS.IsLike($"%{objFilter.USER_NAME}%")
                    || x.EMAIL.IsLike($"%{objFilter.USER_NAME}%")
                );
            }

            if (!string.IsNullOrWhiteSpace(objFilter.CREATE_BY))
            {
                query = query.Where(x => x.CREATE_BY.IsLike($"%{objFilter.CREATE_BY}%"));
            }

            if (!string.IsNullOrWhiteSpace(objFilter.XEP_HANG))
            {
                query = query.Where(x => x.XEP_HANG == objFilter.XEP_HANG);
            }

            if (!string.IsNullOrWhiteSpace(objFilter.DIA_CHI))
            {
                query = query.Where(x => x.DIA_CHI.IsLike($"%{objFilter.DIA_CHI}%"));
            }

            if (objFilter.FROM_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE >= objFilter.FROM_DATE.Value);
            }

            if (objFilter.TO_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE < objFilter.TO_DATE.Value.AddDays(1));
            }

            if (!string.IsNullOrWhiteSpace(objFilter.PRODUCT_CODE))
            {
                subQueryProduct = subQueryProduct.Where(x => x.PRODUCT_CODE == objFilter.PRODUCT_CODE);
                query = query.WithSubquery.WhereProperty(x => x.PKID).In(
                    subQueryProduct.Select(Projections.Property<T_EB_CONTRACTOR_PRODUCT>(p => p.CONTRACTOR_ID))
                );
            }

            if (!string.IsNullOrWhiteSpace(objFilter.QUOC_GIA))
            {
                subQueryMarket = subQueryMarket.Where(x => x.COUNTRY_CODE == objFilter.QUOC_GIA);
                query = query.WithSubquery.WhereProperty(x => x.PKID).In(
                    subQueryMarket.Select(Projections.Property<T_EB_CONTRACTOR_MARKET>(p => p.CONTRACTOR_ID))
                );
            }

            if (!string.IsNullOrWhiteSpace(objFilter.LINH_VUC_CHINH))
            {
                subQueryScope = subQueryScope.Where(x => x.SCOPE_CODE == objFilter.LINH_VUC_CHINH);
                query = query.WithSubquery.WhereProperty(x => x.PKID).In(
                    subQueryScope.Select(Projections.Property<T_EB_CONTRACTOR_SCOPE>(p => p.CONTRACTOR_ID))
                );
            }

            query = query.Fetch(x => x.ListProduct).Eager.Fetch(x => x.ListProduct.First().Product).Eager;
            query = query.Fetch(x => x.ListMarket).Eager.Fetch(x => x.ListMarket.First().Country).Eager;
            query = query.Fetch(x => x.ListScope).Eager.Fetch(x => x.ListScope.First().Scope).Eager;

            query = query.Fetch(x => x.USER_CREATE).Eager.Fetch(x => x.USER_CREATE.Organize).Eager;
            query = query.TransformUsing(Transformers.DistinctRootEntity);
            if (objFilter.IS_NOT_PAGEING)
            {
                total = 0;
                return query.List();
            }

            return base.Paging(query, pageSize, pageIndex, out total);
        }

        public override T_EB_CONTRACTOR Get(object id, dynamic param = null)
        {
            var query = NHibernateSession.QueryOver<T_EB_CONTRACTOR>();

            query = query.Where(x => x.PKID == id);
            query = query.Fetch(x => x.FileGioiThieu).Eager;
            query = query.TransformUsing(new DistinctRootEntityResultTransformer());
            return query.List().FirstOrDefault();
        }
    }
}
