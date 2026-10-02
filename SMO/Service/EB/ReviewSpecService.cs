using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class ReviewSpecService : GenericService<T_EB_REVIEW_SPEC, ReviewSpecRepo>
    {
        public T_EB_REVIEW_SPEC ObjHeader { get; set; }
        public List<T_EB_REVIEW_SPEC_DETAIL> ObjListDetail { get; set; }

        public ReviewSpecService() : base()
        {
            ObjHeader = new T_EB_REVIEW_SPEC();
            ObjListDetail = new List<T_EB_REVIEW_SPEC_DETAIL>();
        }

        public List<NodeReviewSpec> BuildTreeSpec()
        {
            var lstNode = new List<NodeReviewSpec>();
            this.Search();
            foreach (var item in this.ObjList.OrderBy(x => x.C_ORDER))
            {
                var node = new NodeReviewSpec()
                {
                    id = item.PKID,
                    pId = item.PARENT_ID,
                    name = item.NAME,
                    icon = "/Content/zTreeStyle/img/diy/donvi.gif",
                };
                lstNode.Add(node);
            }
            return lstNode;
        }

        public void UpdateTree(List<NodeReviewSpec> lstNode)
        {
            try
            {
                var strSql = "";
                var order = 0;
                UnitOfWork.BeginTransaction();

                foreach (var item in lstNode)
                {
                    if (!string.IsNullOrWhiteSpace(item.pId))
                    {
                        strSql = "UPDATE T_EB_REVIEW_SPEC SET C_ORDER = :C_ORDER, PARENT_ID = :PARENT WHERE PKID = :PKID";
                        UnitOfWork.GetSession().CreateSQLQuery(strSql)
                            .SetParameter("C_ORDER", order)
                            .SetParameter("PARENT", item.pId)
                            .SetParameter("PKID", item.id)
                            .ExecuteUpdate();
                    }
                    else
                    {
                        strSql = "UPDATE T_EB_REVIEW_SPEC SET C_ORDER = :C_ORDER, PARENT_ID = '' WHERE PKID = :PKID";
                        UnitOfWork.GetSession().CreateSQLQuery(strSql)
                            .SetParameter("C_ORDER", order)
                            .SetParameter("PKID", item.id)
                            .ExecuteUpdate();
                    }
                    order++;
                }
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public override void Create()
        {
            try
            {
                this.ObjHeader.PKID = Guid.NewGuid().ToString();
                this.ObjHeader.CREATE_BY = ProfileUtilities.User.USER_NAME;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Create(this.ObjHeader);

                var order = 1;
                foreach (var detail in this.ObjListDetail)
                {
                    detail.PKID = Guid.NewGuid().ToString();
                    detail.HEADER_ID = this.ObjHeader.PKID;
                    detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                    detail.C_ORDER = order;
                    order++;
                }
                UnitOfWork.Repository<ReviewSpecDetailRepo>().Create(this.ObjListDetail);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                this.State = false;
            }
        }

        public override void Update()
        {
            try
            {
                // Kiểm tra hack thay đổi thông tin header
                var poCheck = this.CurrentRepository.Get(this.ObjHeader.PKID);
                UnitOfWork.GetSession().Evict(poCheck);
                if (poCheck == null)
                {
                    this.State = false;
                    this.ErrorMessage = "Thông tin không hợp lệ!";
                    return;
                }

                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);

                var lstDetail = UnitOfWork.Repository<ReviewSpecDetailRepo>().Queryable().Where(x => x.HEADER_ID == this.ObjHeader.PKID).ToList();

                //Tìm những detail bị xóa
                var lstDetailDelete = lstDetail.Where(x => !this.ObjListDetail.Select(y => y.PKID).Contains(x.PKID)).ToList();
                foreach (var detail in lstDetailDelete)
                {
                    UnitOfWork.Repository<ReviewSpecDetailRepo>().Delete(detail);
                }

                var number = 1;
                foreach (var detail in this.ObjListDetail)
                {
                    var find = lstDetail.FirstOrDefault(x => x.PKID == detail.PKID);
                    if (find != null)
                    {
                        find.C_ORDER = number;
                        find.FROM_VALUE = detail.FROM_VALUE;
                        find.TO_VALUE = detail.TO_VALUE;
                        find.VALUE = detail.VALUE;
                        find.VALUE_EN = detail.VALUE_EN;
                        find.SCORE = detail.SCORE;
                        find.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        UnitOfWork.Repository<ReviewSpecDetailRepo>().Update(find);
                    }
                    else
                    {
                        detail.C_ORDER = number;
                        detail.HEADER_ID = this.ObjHeader.PKID;
                        detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                        detail.PKID = Guid.NewGuid().ToString();
                        UnitOfWork.Repository<ReviewSpecDetailRepo>().Create(detail);
                    }
                    number++;
                }


                //var strSQL = "DELETE T_EB_BID_SPEC_DETAIL WHERE HEADER_ID = :HEADER_ID;";
                //UnitOfWork.GetSession().CreateSQLQuery(strSQL)
                //    .SetParameter("HEADER_ID", this.ObjHeader.PKID)
                //    .ExecuteUpdate();

                //foreach (var detail in this.ObjListDetail)
                //{
                //    detail.PKID = Guid.NewGuid().ToString();
                //    detail.HEADER_ID = this.ObjHeader.PKID;
                //    detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                //}

                //var order = 1;
                //foreach (var detail in this.ObjListDetail)
                //{
                //    detail.PKID = Guid.NewGuid().ToString();
                //    detail.HEADER_ID = this.ObjHeader.PKID;
                //    detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                //    detail.C_ORDER = order;
                //    order++;
                //}

                //UnitOfWork.Repository<BidSpecDetailRepo>().Create(this.ObjListDetail);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public override void Get(object id, dynamic param = null)
        {
            try
            {
                this.ObjHeader = this.CurrentRepository.Get(id, param);
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void AddSpecMDToBid(string lstSpec, string reviewId)
        {
            try
            {
                var lstEbSpec = UnitOfWork.Repository<ReviewSpecRepo>().Queryable().Where(x => x.REVIEW_ID == reviewId && lstSpec.Contains(x.MD_PKID)).ToList();
                if (lstEbSpec.Count > 0)
                {
                    this.State = false;
                    this.ErrorMessage = "Tiêu chí đã tồn tại trong bảng đánh giá.";
                    return;
                }
                var lstSpecCopy = new List<T_EB_REVIEW_SPEC>();
                var lstSpecDetailCopy = new List<T_EB_REVIEW_SPEC_DETAIL>();
                foreach (var specId in lstSpec.Split('|'))
                {
                    var specMd = UnitOfWork.Repository<MdReviewSpecRepo>().Get(specId);
                    var lstSpecDetailMd = UnitOfWork.Repository<MdReviewSpecDetailRepo>().Queryable().Where(x => x.HEADER_ID == specId).ToList();
                    if (specMd != null)
                    {
                        var parentId = "";
                        var find = lstSpecCopy.FirstOrDefault(x => x.MD_PKID == specMd.PARENT_ID);
                        if (find != null)
                        {
                            parentId = find.PKID;
                        }

                        var specCopy = new T_EB_REVIEW_SPEC()
                        {
                            MD_PKID = specMd.PKID,
                            PKID = Guid.NewGuid().ToString(),
                            REVIEW_ID = reviewId,
                            PARENT_ID = parentId,
                            C_ORDER = specMd.C_ORDER,
                            NAME = specMd.NAME,
                            NAME_EN = specMd.NAME_EN,
                            DESCRIPTION = specMd.DESCRIPTION,
                            DESCRIPTION_EN = specMd.DESCRIPTION_EN,
                            RATIO = specMd.RATIO,
                            MAX_SCORE = specMd.MAX_SCORE,
                            //IS_DEFAULT = specMd.IS_DEFAULT,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        };
                        lstSpecCopy.Add(specCopy);

                        foreach (var specDetail in lstSpecDetailMd)
                        {
                            lstSpecDetailCopy.Add(new T_EB_REVIEW_SPEC_DETAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                HEADER_ID = specCopy.PKID,
                                C_ORDER = specDetail.C_ORDER,
                                FROM_VALUE = specDetail.FROM_VALUE,
                                SCORE = specDetail.SCORE,
                                TO_VALUE = specDetail.SCORE,
                                VALUE = specDetail.VALUE,
                                VALUE_EN = specDetail.VALUE_EN,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });
                        }
                    }
                }

                UnitOfWork.BeginTransaction();

                foreach (var item in lstSpecCopy)
                {
                    UnitOfWork.Repository<ReviewSpecRepo>().Create(item);
                }

                foreach (var item in lstSpecDetailCopy)
                {
                    UnitOfWork.Repository<ReviewSpecDetailRepo>().Create(item);
                }

                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public override void Delete(string strLstSelected)
        {
            try
            {
                if (this.CurrentRepository.Queryable().Where(x => x.PARENT_ID == strLstSelected).Count() > 0)
                {
                    this.State = false;
                    this.ErrorMessage = "Tiêu chí này đang là cha của những tiêu chí khác!";
                    return;
                }
                //var lstSpecDetail = UnitOfWork.Repository<BidRequireDetailRepo>().Queryable().Where(x => x.HEADER_ID == strLstSelected).ToList();

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Delete(strLstSelected);
                //UnitOfWork.Repository<BidRequireDetailRepo>().Delete(lstSpecDetail);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

    }
}
