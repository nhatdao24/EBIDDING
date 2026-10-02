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

namespace SMO.Service.MD
{
    public class MdBidSpecService : GenericService<T_MD_BID_SPEC, MdBidSpecRepo>
    {
        public T_MD_BID_SPEC ObjHeader { get; set; }
        public List<T_MD_BID_SPEC_DETAIL> ObjListDetail { get; set; }

        public MdBidSpecService() : base()
        {
            ObjHeader = new T_MD_BID_SPEC();
            ObjListDetail = new List<T_MD_BID_SPEC_DETAIL>();
        }

        public List<NodeSpec> BuildTreeSpec(string bidId = "") 
        {
            var lstSpecOfBid = UnitOfWork.Repository<BidSpecRepo>().Queryable().Where(x => x.BID_ID == bidId).ToList();

            var lstNode = new List<NodeSpec>();
            this.Search(); 
            foreach (var i in this.ObjList)
            {
                i.Dictionary = UnitOfWork.Repository<DictionaryRepo>().Queryable().FirstOrDefault(x => x.CODE == i.ROLE);
            }
            foreach (var item in this.ObjList.OrderBy(x => x.C_ORDER))
            {
                var node = new NodeSpec()
                {
                    id = item.PKID,
                    pId = item.PARENT_ID,
                    name = item.NAME + $" <span style='color:#f24b3f;'> -{item.Dictionary?.C_VALUE}- </span>",
                    icon = "/Content/zTreeStyle/img/diy/donvi.gif",
                    type = item.TYPE
                };

                if (lstSpecOfBid.Count(x => x.MD_PKID == item.PKID) > 0)
                {
                    node.nocheck = "true";
                }

                if (item.TYPE == "NUMBER")
                {
                    node.name = node.name + "<span class='spMaOnTree'>Nhập số</span>";
                }else if (item.TYPE == "TEXT")
                {
                    node.name = node.name + "<span class='spMaOnTree'>Nhập chữ</span>";
                }
                else if (item.TYPE == "SELECT")
                {
                    node.name = node.name + "<span class='spMaOnTree'>Danh sách chọn</span>";
                }
                else if (item.TYPE == "GROUP")
                {
                    node.name = node.name + "<span class='spMaOnTree'>Nhóm</span>";
                }
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
                UnitOfWork.Repository<MdBidSpecDetailRepo>().Create(this.ObjListDetail);
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

                var lstDetail = UnitOfWork.Repository<MdBidSpecDetailRepo>().Queryable().Where(x => x.HEADER_ID == this.ObjHeader.PKID).ToList();

                //Tìm những detail bị xóa
                var lstDetailDelete = lstDetail.Where(x => !this.ObjListDetail.Select(y => y.PKID).Contains(x.PKID)).ToList();
                foreach (var detail in lstDetailDelete)
                {
                    UnitOfWork.Repository<MdBidSpecDetailRepo>().Delete(detail);
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
                        UnitOfWork.Repository<MdBidSpecDetailRepo>().Update(find);
                    }
                    else
                    {
                        detail.C_ORDER = number;
                        detail.HEADER_ID = this.ObjHeader.PKID;
                        detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                        detail.PKID = Guid.NewGuid().ToString();
                        UnitOfWork.Repository<MdBidSpecDetailRepo>().Create(detail);
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

        public void UpdateTree(List<NodeSpec> lstNode)
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
                        strSql = "UPDATE T_MD_BID_SPEC SET C_ORDER = :C_ORDER, PARENT_ID = :PARENT WHERE PKID = :PKID";
                        UnitOfWork.GetSession().CreateSQLQuery(strSql)
                            .SetParameter("C_ORDER", order)
                            .SetParameter("PARENT", item.pId)
                            .SetParameter("PKID", item.id)
                            .ExecuteUpdate();
                    }
                    else
                    {
                        strSql = "UPDATE T_MD_BID_SPEC SET C_ORDER = :C_ORDER, PARENT_ID = '' WHERE PKID = :PKID";
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
                var lstSpecDetail = UnitOfWork.Repository<MdBidSpecDetailRepo>().Queryable().Where(x => x.HEADER_ID == strLstSelected).ToList();

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Delete(strLstSelected);
                UnitOfWork.Repository<MdBidSpecDetailRepo>().Delete(lstSpecDetail);
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
