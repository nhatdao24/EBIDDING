using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.EB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.EB
{
    public class BidRepo : GenericRepository<T_EB_BID>, IBidRepo
    {
        public BidRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        public override IList<T_EB_BID> Search(T_EB_BID objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID>();

            if (!string.IsNullOrWhiteSpace(objFilter.TEN_GOI_THAU))
            {
                query = query.Where(x => x.TEN_GOI_THAU.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.PKID.IsLike($"%{objFilter.TEN_GOI_THAU}%"));
            }

            if (!string.IsNullOrEmpty(objFilter.COMPANY_CODE))
            {
                query = query.Where(x => x.COMPANY_CODE == objFilter.COMPANY_CODE);
            }

            //if (objFilter.CREATE_DATE.HasValue)
            //{
            //    query = query.Where(x => x.CREATE_DATE >= objFilter.CREATE_DATE.Value && x.CREATE_DATE < objFilter.CREATE_DATE.Value.AddDays(1));
            //}
            if (objFilter.FROM_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE >= objFilter.FROM_DATE.Value);
            }

            if (objFilter.TO_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE < objFilter.TO_DATE.Value.AddDays(1));
            }

            if (objFilter.FROM_DATE_NGAY_MO_THAU.HasValue)
            {
                query = query.Where(x => x.NGAY_MO_THAU >= objFilter.FROM_DATE_NGAY_MO_THAU.Value);
            }

            if (objFilter.TO_DATE_NGAY_MO_THAU.HasValue)
            {
                query = query.Where(x => x.NGAY_MO_THAU < objFilter.TO_DATE_NGAY_MO_THAU.Value.AddDays(1));
            }

            if (objFilter.FROM_DATE_NGAY_DONG_THAU.HasValue)
            {
                query = query.Where(x => x.NGAY_DONG_THAU >= objFilter.FROM_DATE_NGAY_DONG_THAU.Value);
            }

            if (objFilter.TO_DATE_NGAY_DONG_THAU.HasValue)
            {
                query = query.Where(x => x.NGAY_DONG_THAU < objFilter.TO_DATE_NGAY_DONG_THAU.Value.AddDays(1));
            }

            if (!string.IsNullOrEmpty(objFilter.USER_APPROVE))
            {
                query = query.Where(x => x.USER_APPROVE == objFilter.USER_APPROVE);
            }

            if (!string.IsNullOrEmpty(objFilter.CREATE_BY))
            {
                query = query.Where(x => x.CREATE_BY == objFilter.CREATE_BY);
            }

            //ConfigViettinBank: Xóa dự án

            //if (!string.IsNullOrEmpty(objFilter.PROJECT_CODE))
            //{
            //    query = query.Where(x => x.PROJECT_CODE == objFilter.PROJECT_CODE);
            //}

            if (!string.IsNullOrEmpty(objFilter.GIAI_DOAN))
            {
                if (objFilter.GIAI_DOAN == "02")
                {
                    //query = query.Where(x => x.GIAI_DOAN == "02" || (x.GIAI_DOAN == "03" && x.NGAY_DONG_THAU >= DateTime.Now) || x.GIAI_DOAN == "04" || x.GIAI_DOAN == "08");
                    query = query.Where(x => (x.GIAI_DOAN == "02" || x.GIAI_DOAN == "03" || x.GIAI_DOAN == "04" || x.GIAI_DOAN == "08") && x.NGAY_DONG_THAU >= DateTime.Now);
                }
                else if (objFilter.GIAI_DOAN == "05")
                {
                    query = query.Where(x => x.GIAI_DOAN == "05" || x.GIAI_DOAN == "07");
                }
                else if (objFilter.GIAI_DOAN == "01")
                {
                    query = query.Where(x => x.GIAI_DOAN == "01");
                }
                else if (objFilter.GIAI_DOAN == "06")
                {
                    query = query.Where(x => x.GIAI_DOAN == "06");
                }
                else if (objFilter.GIAI_DOAN == "08")
                {
                    query = query.Where(x => (x.GIAI_DOAN == "02" || x.GIAI_DOAN == "03" || x.GIAI_DOAN == "04" || x.GIAI_DOAN == "08") && x.NGAY_DONG_THAU < DateTime.Now);
                }
            }

            //ConfigViettinBank: Xóa dự án 

            //query = query.Where(x => x.PROJECT_CODE.IsIn(objFilter.ListFilterProject));

            query = query.OrderBy(x => x.CREATE_DATE).Desc;
            if (objFilter.IS_NOT_PAGEING)
            {
                total = 0;
                return query.List();
            }

            return base.Paging(query, pageSize, pageIndex, out total);
        }

        public IList<T_EB_BID> Search_ProcessByMe(T_EB_BID objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID>();
            var subQuery = NHibernateSession.QueryOver<T_EB_BID_COUNCIL>();

            subQuery = subQuery.Where(x => x.USER_NAME == objFilter.USER_APPROVE);
            var lstBidOfCouncil = subQuery.List().Select(x => x.BID_ID).ToList();

            if (!string.IsNullOrWhiteSpace(objFilter.TEN_GOI_THAU))
            {
                query = query.Where(x => x.TEN_GOI_THAU.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.PKID.IsLike($"%{objFilter.TEN_GOI_THAU}%"));
            }

            if (!string.IsNullOrEmpty(objFilter.COMPANY_CODE))
            {
                query = query.Where(x => x.COMPANY_CODE == objFilter.COMPANY_CODE);
            }

            if (!string.IsNullOrEmpty(objFilter.CREATE_BY))
            {
                query = query.Where(x => x.CREATE_BY == objFilter.CREATE_BY);
            }

            //if (objFilter.CREATE_DATE.HasValue)
            //{
            //    query = query.Where(x => x.CREATE_DATE >= objFilter.CREATE_DATE.Value && x.CREATE_DATE < objFilter.CREATE_DATE.Value.AddDays(1));
            //}
            if (objFilter.FROM_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE >= objFilter.FROM_DATE.Value);
            }

            if (objFilter.TO_DATE.HasValue)
            {
                query = query.Where(x => x.CREATE_DATE < objFilter.TO_DATE.Value.AddDays(1));
            }

            if (objFilter.FROM_DATE_NGAY_MO_THAU.HasValue)
            {
                query = query.Where(x => x.NGAY_MO_THAU >= objFilter.FROM_DATE_NGAY_MO_THAU.Value);
            }

            if (objFilter.TO_DATE_NGAY_MO_THAU.HasValue)
            {
                query = query.Where(x => x.NGAY_MO_THAU < objFilter.TO_DATE_NGAY_MO_THAU.Value.AddDays(1));
            }

            if (objFilter.FROM_DATE_NGAY_DONG_THAU.HasValue)
            {
                query = query.Where(x => x.NGAY_DONG_THAU >= objFilter.FROM_DATE_NGAY_DONG_THAU.Value);
            }

            if (objFilter.TO_DATE_NGAY_DONG_THAU.HasValue)
            {
                query = query.Where(x => x.NGAY_DONG_THAU < objFilter.TO_DATE_NGAY_DONG_THAU.Value.AddDays(1));
            }

            //ConfigViettinBank: Xóa dự án

            //if (!string.IsNullOrEmpty(objFilter.PROJECT_CODE))
            //{
            //    query = query.Where(x => x.PROJECT_CODE == objFilter.PROJECT_CODE);
            //}

            if (!string.IsNullOrEmpty(objFilter.GIAI_DOAN))
            {
                if (objFilter.GIAI_DOAN == "02")
                {
                    query = query.Where(x => (x.GIAI_DOAN == "02" || x.GIAI_DOAN == "03" || x.GIAI_DOAN == "04" || x.GIAI_DOAN == "08") && x.NGAY_DONG_THAU >= DateTime.Now);
                }
                else if (objFilter.GIAI_DOAN == "05")
                {
                    query = query.Where(x => x.GIAI_DOAN == "05");
                }
                else if (objFilter.GIAI_DOAN == "01")
                {
                    query = query.Where(x => x.GIAI_DOAN == "01");
                }
                else if (objFilter.GIAI_DOAN == "08")
                {
                    query = query.Where(x => (x.GIAI_DOAN == "02" || x.GIAI_DOAN == "03" || x.GIAI_DOAN == "04" || x.GIAI_DOAN == "08") && x.NGAY_DONG_THAU < DateTime.Now);
                }
            }

            //ConfigViettinBank: Xóa dự án

            //query = query.Where(x => x.PROJECT_CODE.IsIn(objFilter.ListFilterProject));

            query = query.Where(Restrictions.Or(
                    Restrictions.On<T_EB_BID>(x => x.USER_APPROVE).IsLike(objFilter.USER_APPROVE),
                    Restrictions.On<T_EB_BID>(x => x.PKID).IsIn(lstBidOfCouncil)
                ));

            query = query.OrderBy(x => x.CREATE_DATE).Desc;
            return base.Paging(query, pageSize, pageIndex, out total);
        }

        public IList<T_EB_BID> SearchPortal(T_EB_BID objFilter, string contractorId, List<string> lstId, bool isUserFecon, int pageSize, int pageIndex, out int total)
        {
            var lstBidOfContractor = new List<T_EB_BID_CONTRACTOR>();
            var queryBidContractor = NHibernateSession.QueryOver<T_EB_BID_CONTRACTOR>();

            if (!string.IsNullOrEmpty(contractorId))
            {
                queryBidContractor = queryBidContractor.Where(x => x.CONTRACTOR_ID == contractorId);
                lstBidOfContractor = queryBidContractor.List().ToList();
            }

            var query = NHibernateSession.QueryOver<T_EB_BID>();

            if (lstId.Count > 0)
            {
                query = query.Where(x => !x.PKID.IsIn(lstId));
            }

            if (!string.IsNullOrWhiteSpace(objFilter.TEN_GOI_THAU))
            {
                query = query.Where(x => x.TEN_GOI_THAU.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%"));
            }


            if (!string.IsNullOrEmpty(objFilter.SEARCH_TRANG_THAI) && objFilter.SEARCH_TRANG_THAI.Contains("DangThau") && objFilter.SEARCH_TRANG_THAI.Contains("KetThuc"))
            {

            }
            else if (!string.IsNullOrEmpty(objFilter.SEARCH_TRANG_THAI) && objFilter.SEARCH_TRANG_THAI.Contains("DangThau"))
            {
                query = query.Where(x => x.GIAI_DOAN == "02" || x.GIAI_DOAN == "03" || x.GIAI_DOAN == "04");
            }
            else if (!string.IsNullOrEmpty(objFilter.SEARCH_TRANG_THAI) && objFilter.SEARCH_TRANG_THAI.Contains("KetThuc"))
            {
                query = query.Where(x => x.GIAI_DOAN == "05");
            }

            if (!string.IsNullOrEmpty(objFilter.COMPANY_CODE))
            {
                query = query.Where(x => x.COMPANY_CODE == objFilter.COMPANY_CODE);
            }

            if (!string.IsNullOrEmpty(objFilter.TYPE))
            {
                var lstType = objFilter.TYPE.Split(',').ToList();
                query = query.Where(x => x.TYPE.IsIn(lstType));
            }

            if (!string.IsNullOrEmpty(objFilter.PROJECT_CODE))
            {
                query = query.Where(x => x.PROJECT_CODE == objFilter.PROJECT_CODE);
            }

            if (!string.IsNullOrEmpty(contractorId))
            {
                if (!string.IsNullOrEmpty(objFilter.SEARCH_TRANG_THAI) && objFilter.SEARCH_TRANG_THAI.Contains("DuocMoi"))
                {
                    query = query.Where(x => x.PKID.IsIn(lstBidOfContractor.Select(y => y.BID_ID).ToList()));
                }
                else
                {
                    query = query.Where(x => x.IS_PUBLIC == true ||
                    (x.IS_PUBLIC == false && x.PKID.IsIn(lstBidOfContractor.Select(y => y.BID_ID).ToList()))
                    );
                }
            }
            else if (!isUserFecon)
            {
                query = query.Where(x => x.IS_PUBLIC == true);
            }

            query = query.Where(x => x.STATUS == "05");
            query = query.Where(x => x.GIAI_DOAN != "01" && x.GIAI_DOAN != "07");
            query = query.Fetch(x => x.ListNew).Eager;
            query = query.Fetch(x => x.BidType).Eager;
            query = query.OrderBy(x => x.CREATE_DATE).Desc;
            query = query.TransformUsing(Transformers.DistinctRootEntity);

            return base.Paging(query, pageSize, pageIndex, out total);
        }

        public IList<T_EB_BID> SearchTopBid(string userName)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID>();
            var subQuery = NHibernateSession.QueryOver<T_EB_BID_COUNCIL>();

            subQuery = subQuery.Where(x => x.USER_NAME == userName);
            var lstBidOfCouncil = subQuery.List().Select(x => x.BID_ID).ToList();

            query = query.Where(x => x.GIAI_DOAN != "05");

            query = query.Where(Restrictions.Or(
                    Restrictions.On<T_EB_BID>(x => x.USER_APPROVE).IsLike(userName),
                    Restrictions.Or(
                        Restrictions.On<T_EB_BID>(x => x.CREATE_BY).IsLike(userName),
                        Restrictions.On<T_EB_BID>(x => x.PKID).IsIn(lstBidOfCouncil)
                    )
                ));

            return query.OrderBy(x => x.CREATE_DATE).Desc.List();
        }

        public IList<T_EB_BID> SearchBidOfContractor(T_EB_BID objFilter, string contractorId, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID>();
            var subQuery = QueryOver.Of<T_EB_BID_CONTRACTOR>();

            if (!string.IsNullOrWhiteSpace(objFilter.TEN_GOI_THAU))
            {
                query = query.Where(x => x.TEN_GOI_THAU.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.PKID.IsLike($"%{objFilter.TEN_GOI_THAU}%"));
            }

            subQuery = subQuery.Where(x => x.CONTRACTOR_ID == contractorId && x.STATUS == "AP");

            query = query.WithSubquery.WhereProperty(x => x.PKID).In(
                    subQuery.Select(Projections.Property<T_EB_BID_CONTRACTOR>(p => p.BID_ID))
                );

            query = query.Where(x => x.GIAI_DOAN != "01" && x.GIAI_DOAN != "05");

            query = query.OrderBy(x => x.CREATE_DATE).Desc;

            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }

        public IList<T_EB_BID> SearchBidOfContractorWait(T_EB_BID objFilter, string contractorId, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID>();
            var subQuery = QueryOver.Of<T_EB_BID_CONTRACTOR>();

            if (!string.IsNullOrWhiteSpace(objFilter.TEN_GOI_THAU))
            {
                query = query.Where(x => x.TEN_GOI_THAU.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.PKID.IsLike($"%{objFilter.TEN_GOI_THAU}%"));
            }

            subQuery = subQuery.Where(x => x.CONTRACTOR_ID == contractorId && (x.STATUS == "CR"));

            query = query.WithSubquery.WhereProperty(x => x.PKID).In(
                    subQuery.Select(Projections.Property<T_EB_BID_CONTRACTOR>(p => p.BID_ID))
                );

            query = query.OrderBy(x => x.CREATE_DATE).Desc;
            query = query.Where(x => x.GIAI_DOAN != "01");

            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }

        public IList<T_EB_BID> SearchBidOfContractorOpen(T_EB_BID objFilter, string contractorId, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID>();
            var subQuery = QueryOver.Of<T_EB_BID_CONTRACTOR>();

            if (!string.IsNullOrWhiteSpace(objFilter.TEN_GOI_THAU))
            {
                query = query.Where(x => x.TEN_GOI_THAU.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.PKID.IsLike($"%{objFilter.TEN_GOI_THAU}%"));
            }

            subQuery = subQuery.Where(x => x.CONTRACTOR_ID == contractorId);

            query = query.WithSubquery.WhereProperty(x => x.PKID).NotIn(
                    subQuery.Select(Projections.Property<T_EB_BID_CONTRACTOR>(p => p.BID_ID))
                );

            query = query.Where(x => x.GIAI_DOAN == "02" && x.IS_PUBLIC == true);

            query = query.OrderBy(x => x.CREATE_DATE).Desc;

            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }

        public IList<T_EB_BID> SearchBidOfContractorClose(T_EB_BID objFilter, string contractorId, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID>();
            var subQuery = QueryOver.Of<T_EB_BID_CONTRACTOR>();

            if (!string.IsNullOrWhiteSpace(objFilter.TEN_GOI_THAU))
            {
                query = query.Where(x => x.TEN_GOI_THAU.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.TEN_GOI_THAU_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_EN.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.MO_TA_KD.IsLike($"%{objFilter.TEN_GOI_THAU}%")
                || x.PKID.IsLike($"%{objFilter.TEN_GOI_THAU}%"));
            }

            subQuery = subQuery.Where(x => x.CONTRACTOR_ID == contractorId && x.STATUS == "AP");

            query = query.WithSubquery.WhereProperty(x => x.PKID).In(
                    subQuery.Select(Projections.Property<T_EB_BID_CONTRACTOR>(p => p.BID_ID))
                );

            query = query.Where(x => x.GIAI_DOAN == "05");
            query = query.Fetch(x => x.ListContractor).Eager;
            query = query.OrderBy(x => x.CREATE_DATE).Desc.TransformUsing(Transformers.DistinctRootEntity);

            return base.Paging(query, pageSize, pageIndex, out total).ToList();
        }

        public override T_EB_BID Get(object id, dynamic param = null)
        {
            var query = NHibernateSession.QueryOver<T_EB_BID>();

            query = query.Where(x => x.PKID == id);

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListFile"))
            {
                query = query.Fetch(x => x.ListFile).Eager
                    .Fetch(x => x.ListFile.First().FileUpload).Eager;
            }

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListNew"))
            {
                query = query.Fetch(x => x.ListNew).Eager;
            }

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListContractor"))
            {
                query = query.Fetch(x => x.ListContractor).Eager
                    .Fetch(x => x.ListContractor.First().Contractor).Eager;
            }

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListHistory"))
            {
                query = query.Fetch(x => x.ListHistory).Eager
                    .Fetch(x => x.ListHistory.First().ActionUser).Eager;
            }

            query = query.Fetch(x => x.BidType).Eager
                .Fetch(x => x.UserSent).Eager
                .Fetch(x => x.UserApprove).Eager;
            query = query.TransformUsing(Transformers.DistinctRootEntity);
            return query.List().FirstOrDefault();
        }

    }
}
