using NHibernate.Linq;
using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Service.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class ReviewContractorService : GenericService<T_EB_REVIEW_CONTRACTOR, ReviewContractorRepo>
    {
        public T_EB_REVIEW_CONTRACTOR_DETAIL ObjReviewContractorDetail { get; set; }
        public List<T_EB_BID> ObjListBid { get; set; }
        public List<T_EB_BID_CONTRACTOR_PRICE> ObjListBidPrice { get; set; }
        public List<T_MD_MATERIAL_TYPE> ObjListMaterialType { get; set; }
        public string SearchName { get; set; }
        public string ProjectCode { get; set; }
        public bool SearchIsCheckThamGia { get; set; }
        public bool SearchIsCheckTrungThau { get; set; }

        public ReviewContractorService() : base()
        {
            this.ObjReviewContractorDetail = new T_EB_REVIEW_CONTRACTOR_DETAIL();
            this.ObjListBid = new List<T_EB_BID>();
            this.ObjListBidPrice = new List<T_EB_BID_CONTRACTOR_PRICE>();
            this.ObjListMaterialType = new List<T_MD_MATERIAL_TYPE>();
        }

        public void AddDetailReviewContractor()
        {
            try
            {
                this.ObjReviewContractorDetail.PKID = Guid.NewGuid().ToString();
                this.ObjReviewContractorDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                this.UnitOfWork.BeginTransaction();
                this.UnitOfWork.Repository<ReviewContractorDetailRepo>().Create(this.ObjReviewContractorDetail);
                this.UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                this.UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void UpdateDetailReviewContractor()
        {
            try
            {
                this.ObjReviewContractorDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.UnitOfWork.BeginTransaction();
                this.UnitOfWork.Repository<ReviewContractorDetailRepo>().Update(this.ObjReviewContractorDetail);
                this.UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                this.UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void DeleteDetailReviewContractor(string id)
        {
            try
            {
                this.UnitOfWork.BeginTransaction();
                this.UnitOfWork.Repository<ReviewContractorDetailRepo>().Delete(id);
                this.UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                this.UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void GetDetailReviewContractor(string id)
        {
            this.ObjReviewContractorDetail = UnitOfWork.Repository<ReviewContractorDetailRepo>().Get(id);
        }

        public void SearchMaterialType()
        {
            var queryMaterialType = this.UnitOfWork.Repository<MaterialTypeRepo>().Queryable();
            if (!string.IsNullOrWhiteSpace(this.SearchName))
            {
                queryMaterialType.Where(x => x.TEXT.Contains(this.SearchName));
            }
            this.ObjListMaterialType = queryMaterialType.ToList();
        }
        public void SearchBid()
        {
            var review = UnitOfWork.Repository<ReviewRepo>().Queryable().FirstOrDefault(x => x.CODE == this.ObjDetail.REVIEW_ID);
            var lstContractorDetailBidId = "";
            foreach (var item in review.ListContractor)
            {
                foreach (var detail in item.ListDetail)
                {
                    lstContractorDetailBidId += (detail.INFO_BID_EBID + ",");
                }
            }
            var queryBid = this.UnitOfWork.Repository<BidRepo>().Queryable();
            if (!string.IsNullOrWhiteSpace(this.SearchName))
            {
                queryBid = queryBid.Where(
                    x => x.TEN_GOI_THAU.Contains(this.SearchName) ||
                    x.TEN_GOI_THAU_EN.Contains(this.SearchName) ||
                    x.MO_TA.Contains(this.SearchName) ||
                    x.MO_TA_EN.Contains(this.SearchName)
                    );
            }
            //if (!string.IsNullOrEmpty(this.ProjectCode))
            //{
            //    queryBid = queryBid.Where(x => x.PROJECT_CODE == this.ProjectCode);
            //}

            //if (this.SearchIsCheckThamGia || this.SearchIsCheckTrungThau)
            //{
            //    var queryThamGia = this.UnitOfWork.Repository<BidContractorRepo>().Queryable();
            //    queryThamGia = queryThamGia.Where(
            //            x => x.CONTRACTOR_ID == this.ObjDetail.CONTRACTOR_ID
            //        );
            //    if (this.SearchIsCheckTrungThau)
            //    {
            //        queryThamGia = queryThamGia.Where(x => x.IS_WINNING == true);
            //    }

            //    var lstBidThamGia = queryThamGia.ToList().Select(x => x.BID_ID);

            //    queryBid = queryBid.Where(x => lstBidThamGia.Contains(x.PKID));
            //}

            queryBid = queryBid.Where(x => x.PROJECT_CODE == review.PROJECT_CODE);
            var queryThamGia = this.UnitOfWork.Repository<BidContractorRepo>().Queryable();
            queryThamGia = queryThamGia.Where(
                    x => x.CONTRACTOR_ID == this.ObjDetail.CONTRACTOR_ID
                );

            queryThamGia = queryThamGia.Where(x => x.IS_WINNING == true);
            var lstBidThamGia = queryThamGia.ToList().Select(x => x.BID_ID);
            queryBid = queryBid.Where(x => lstBidThamGia.Contains(x.PKID) && !lstContractorDetailBidId.Contains(x.PKID));

            var queryBidPrice = this.UnitOfWork.Repository<BidContractorPriceRepo>().Queryable();
            queryBidPrice = queryBidPrice.Where(x => lstBidThamGia.Contains(x.BID_ID) && !lstContractorDetailBidId.Contains(x.BID_ID) && x.TYPE == "3");

            this.ObjListBid = queryBid.ToList();
            this.ObjListBidPrice = queryBidPrice.ToList();
        }
    }
}
