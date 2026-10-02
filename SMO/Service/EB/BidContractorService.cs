using NHibernate.Linq;
using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.CM;
using SMO.Repository.Implement.EB;
using SMO.Service.AD;
using SMO.Service.CF;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class BidContractorService : GenericService<T_EB_BID_CONTRACTOR, BidContractorRepo>
    {
        public BidContractorService() : base()
        {

        }

        public void GetWinContractorOfBid(string bidId)
        {
            this.ObjList = this.CurrentRepository.Queryable().Fetch(x => x.Contractor).Where(x => x.BID_ID == bidId && x.IS_WINNING == true).ToList();
        }

        public void Comment(string bidId, string content)
        {
            try
            {
                UnitOfWork.BeginTransaction();
                UnitOfWork.Repository<BidLeadershipCommentsRepo>().Create(new T_EB_BID_LEADERSHIP_COMMENTS()
                {
                    PKID = Guid.NewGuid().ToString(),
                    BID_ID = bidId,
                    USER_NAME = ProfileUtilities.User.USER_NAME,
                    CONTENTS = content
                });
                UnitOfWork.Commit();
                this.CreateNotifyLeadershipComment(bidId);
                this.CreateEmailLeadershipComment(bidId);
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        /// <summary>
        /// Tạo thông báo ý kiến lãnh đạo
        /// </summary>
        public void CreateNotifyLeadershipComment(string bidId)
        {
            try
            {
                var bid = UnitOfWork.Repository<BidRepo>().Get(bidId);
                List<string> lstUserName = new List<string>();
                lstUserName.Add(bid.CREATE_BY);
                lstUserName.Add(bid.USER_MANAGE);
                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                foreach (var userName in lstUserName)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(userName);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] đã có ý kiến lãnh đạo</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={bid.PKID}&idMenu=mnuThongTin",
                        EB_Status.GetStatusColor(bid.STATUS),
                        "how_to_reg",
                        bid.PKID,
                        bid.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {bid.PKID} đã có ý kiến lãnh đạo";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent,
                        RAW_CONTENTS_EN = strRawContent,
                        USER_NAME = userName
                    };
                    serviceNotify.Create();
                }

                SMOUtilities.SendNotify(lstUserNotify);
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// Thông báo email ý kiến lãnh đạo
        /// </summary>
        /// <param name="lstContractor"></param>
        public void CreateEmailLeadershipComment(string bidId)
        {
            var bid = UnitOfWork.Repository<BidRepo>().Get(bidId);
            List<string> lstUserName = new List<string>();
            lstUserName.Add(bid.CREATE_BY);
            lstUserName.Add(bid.USER_MANAGE);
            var lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => lstUserName.Contains(x.USER_NAME)).ToList();
            try
            {
                string MA_GOI_THAU = "{MA_GOI_THAU}";
                string TEN_GOI_THAU = "{TEN_GOI_THAU}";
                string TEN_GOI_THAU_EN = "{TEN_GOI_THAU_EN}";
                string NGAY_MO_THAU = "{NGAY_MO_THAU}";
                string NGAY_DONG_THAU = "{NGAY_DONG_THAU}";
                string LINK_CHI_TIET = "{LINK_CHI_TIET}";
                string DU_AN = "{DU_AN}";
                string DU_AN_EN = "{DU_AN_EN}";
                string TRANG_THAI = "{TRANG_THAI}";

                var serviceTemplate = new ConfigTemplateNotifyService();
                serviceTemplate.GetAll();
                if (serviceTemplate.ObjList.Count() == 0)
                {
                    return;
                }


                var serviceConfig = new SystemConfigService();
                serviceConfig.GetConfig();

                var template = serviceTemplate.ObjList.FirstOrDefault();

                if (string.IsNullOrEmpty(template.Y_KIEN_LANH_DAO_SUBJECT) || string.IsNullOrEmpty(template.Y_KIEN_LANH_DAO_BODY))
                {
                    return;
                }

                var url = ConfigurationManager.AppSettings["URL_WEB"];

                foreach (var item in lstUser)
                {
                    if (!string.IsNullOrEmpty(item.EMAIL))
                    {

                        var contentSubject = template.Y_KIEN_LANH_DAO_SUBJECT;
                        contentSubject = contentSubject
                        .Replace(MA_GOI_THAU, bid.PKID)
                        .Replace(TEN_GOI_THAU, bid.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, bid.TEN_GOI_THAU_EN)
                        .Replace(DU_AN, bid.Project.NAME)
                        .Replace(DU_AN_EN, bid.Project.NAME_EN)
                        .Replace(NGAY_MO_THAU, bid.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_DONG_THAU, bid.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(TRANG_THAI, EB_Status.GetStatusText(bid.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Index/ChiTiet/" + bid.PKID);

                        var contentBody = template.Y_KIEN_LANH_DAO_BODY;
                        contentBody = contentBody
                            .Replace(MA_GOI_THAU, bid.PKID)
                            .Replace(TEN_GOI_THAU, bid.TEN_GOI_THAU)
                            .Replace(TEN_GOI_THAU_EN, bid.TEN_GOI_THAU_EN)
                            .Replace(DU_AN, bid.Project.NAME)
                            .Replace(DU_AN_EN, bid.Project.NAME_EN)
                            .Replace(NGAY_MO_THAU, bid.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                            .Replace(NGAY_DONG_THAU, bid.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                            .Replace(TRANG_THAI, EB_Status.GetStatusText(bid.STATUS))
                            .Replace(LINK_CHI_TIET, url + "Index/ChiTiet/" + bid.PKID);

                        try
                        {
                            UnitOfWork.BeginTransaction();

                            var email = new T_CM_EMAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                EMAIL = item.EMAIL,
                                SUBJECT = contentSubject,
                                CONTENTS = contentBody
                            };
                            UnitOfWork.Repository<EmailRepo>().Create(email);
                            UnitOfWork.Commit();
                        }
                        catch (Exception ex)
                        {
                            UnitOfWork.Rollback();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
