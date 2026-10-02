using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Service.MD
{
    public class MdReviewSpecService : GenericService<T_MD_REVIEW_SPEC, MdReviewSpecRepo>
    {
        public T_MD_REVIEW_SPEC ObjHeader { get; set; }
        public List<T_MD_REVIEW_SPEC_DETAIL> ObjListDetail { get; set; }
        public MdReviewSpecService() : base()
        {
            ObjHeader = new T_MD_REVIEW_SPEC();
            ObjListDetail = new List<T_MD_REVIEW_SPEC_DETAIL>();
        }

        public List<NodeReviewSpec> BuildTreeSpec(string bidId = "")
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
                    icon = "/Content/zTreeStyle/img/diy/donvi.gif"
                };
                lstNode.Add(node);
            }
            return lstNode;
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
                UnitOfWork.Repository<MdReviewSpecDetailRepo>().Create(this.ObjListDetail);
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

                var lstDetail = UnitOfWork.Repository<MdReviewSpecDetailRepo>().Queryable().Where(x => x.HEADER_ID == this.ObjHeader.PKID).ToList();

                //Tìm những detail bị xóa
                var lstDetailDelete = lstDetail.Where(x => !this.ObjListDetail.Select(y => y.PKID).Contains(x.PKID)).ToList();
                foreach (var detail in lstDetailDelete)
                {
                    UnitOfWork.Repository<MdReviewSpecDetailRepo>().Delete(detail);
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
                        find.SCORE = detail.SCORE;
                        find.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        UnitOfWork.Repository<MdReviewSpecDetailRepo>().Update(find);
                    }
                    else
                    {
                        detail.C_ORDER = number;
                        detail.HEADER_ID = this.ObjHeader.PKID;
                        detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                        detail.PKID = Guid.NewGuid().ToString();
                        UnitOfWork.Repository<MdReviewSpecDetailRepo>().Create(detail);
                    }
                    number++;
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
                        strSql = "UPDATE T_MD_REVIEW_SPEC SET C_ORDER = :C_ORDER, PARENT_ID = :PARENT WHERE PKID = :PKID";
                        UnitOfWork.GetSession().CreateSQLQuery(strSql)
                            .SetParameter("C_ORDER", order)
                            .SetParameter("PARENT", item.pId)
                            .SetParameter("PKID", item.id)
                            .ExecuteUpdate();
                    }
                    else
                    {
                        strSql = "UPDATE T_MD_REVIEW_SPEC SET C_ORDER = :C_ORDER, PARENT_ID = '' WHERE PKID = :PKID";
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
                var lstSpecDetail = UnitOfWork.Repository<MdReviewSpecDetailRepo>().Queryable().Where(x => x.HEADER_ID == strLstSelected).ToList();

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Delete(strLstSelected);
                UnitOfWork.Repository<MdReviewSpecDetailRepo>().Delete(lstSpecDetail);
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
