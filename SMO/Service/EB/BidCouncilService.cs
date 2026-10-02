using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Service.AD;
using SMO.Service.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class BidCouncilService : GenericService<T_EB_BID_COUNCIL, BidCouncilRepo>
    {
        public string BidId { get; set; }
        public BidCouncilService() : base()
        {

        }

        public List<NodeUser> BuildTreeUser()
        {
            var lstNode = new List<NodeUser>();
            var serviceOrganize = new OrganizeService();
            serviceOrganize.GetAll();
            foreach (var organize in serviceOrganize.ObjList.OrderBy(x => x.C_ORDER))
            {
                var nodeOrganize = new NodeUser()
                {
                    id = organize.PKID,
                    pId = organize.PARENT,
                    name = organize.NAME,
                    icon = "/Content/zTreeStyle/img/diy/donvi.gif",
                    type = "OG",
                    nocheck = "true"
                };
                lstNode.Add(nodeOrganize);
                foreach (var user in organize.ListUser.OrderBy(x => x.USER_NAME))
                {
                    var nodeUser = new NodeUser()
                    {
                        id = user.USER_NAME,
                        pId = organize.PKID,
                        name = user.FULL_NAME + "<span class='spMaOnTree'>" + user.USER_NAME + "</span>",
                        userName = user.USER_NAME,
                        fullName = user.FULL_NAME,
                        email = user.EMAIL,
                        phone = user.PHONE,
                        icon = "/Content/zTreeStyle/img/diy/user.gif",
                        type = "US"
                    };
                    if (this.ObjList.Count(x => x.USER_NAME == user.USER_NAME) > 0)
                    {
                        nodeUser.@checked = "true";
                    }
                    lstNode.Add(nodeUser);
                }
            }
            return lstNode;
        }

        public void AddCouncilToBid(string bidId, string lstCouncilID)
        {
            try
            {
                UnitOfWork.BeginTransaction();

                foreach (var userName in lstCouncilID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    var item = new T_EB_BID_COUNCIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        BID_ID = bidId,
                        USER_NAME = userName,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    };

                    UnitOfWork.Repository<BidCouncilRepo>().Create(item);
                }
                UnitOfWork.Commit();

                var lstCouncil = new List<T_AD_USER>();
                foreach (var council in lstCouncilID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    var find = UnitOfWork.Repository<UserRepo>().Get(council);
                    if (find != null)
                    {
                        lstCouncil.Add(find);
                    }
                }

                var serviceBid = new BidService();
                serviceBid.Get(bidId);
                if (serviceBid.ObjDetail.GIAI_DOAN == Phase_Status.NopHoSo || serviceBid.ObjDetail.GIAI_DOAN == Phase_Status.ChamThau)
                {
                    serviceBid.CreateNotifyInviteCouncil(lstCouncil.Select(x => x.USER_NAME).ToList());
                    serviceBid.CreateInviteEmailCouncil(lstCouncil);

                }

            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public override void Update()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                foreach (var item in this.ObjList)
                {
                    this.CurrentRepository.Update(item);
                }
                UnitOfWork.Commit();
                this.ObjList = new List<T_EB_BID_COUNCIL>();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }
    }
}
