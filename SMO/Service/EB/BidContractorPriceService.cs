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
using System.IO;
using System.Linq;
using System.Web;
using System.Configuration;
namespace SMO.Service.EB
{
    public class BidContractorPriceService : GenericService<T_EB_BID_CONTRACTOR_PRICE, BidContractorPriceRepo>
    {
        public BidContractorPriceService() : base()
        {

        }

        public void Update(HttpRequestBase Request, string isTT)
        {
            try
            {
                UnitOfWork.BeginTransaction();
                var dictionary = new Dictionary<FILE_STREAM, string>();
                // Lưu file vào database
                var lstFileStream = new List<FILE_STREAM>();
                for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                {
                    var file = Request.Files[i];
                    var code = Guid.NewGuid().ToString();
                    var referenceCode = Guid.NewGuid().ToString();

                    var fileStream = new FILE_STREAM()
                    {
                        PKID = code,
                        FILE_OLD_NAME = file.FileName,
                        REFERENCE_ID = referenceCode,
                        FILE_EXT = Path.GetExtension(file.FileName),
                        FILE_SIZE = file.ContentLength,
                        FILESTREAM = Request.Files[i]
                    };
                    lstFileStream.Add(fileStream);
                    dictionary.Add(fileStream, Request.Files.GetKey(i));
                }
                FileStreamService.InsertFile(lstFileStream);

                // Xóa file
                if (!string.IsNullOrEmpty(this.ObjDetail.ListFileDelete))
                {
                    var strDelete = "";

                    foreach (var item in this.ObjDetail.ListFileDelete.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        strDelete += $"DELETE T_EB_BID_CONTRACTOR_PRICE_FILE WHERE FILE_ID = '{item}';";
                    }
                    UnitOfWork.GetSession().CreateSQLQuery(strDelete).ExecuteUpdate();
                }

                var lstContractorId = this.ObjList.Select(x => x.CONTRACTOR_ID).Distinct().ToList();
                foreach (var contractorId in lstContractorId)
                {
                    if (dictionary.Where(x => x.Value == contractorId).Count() > 0)
                    {
                        foreach (var item in dictionary.Where(x => x.Value == contractorId))
                        {
                            UnitOfWork.Repository<BidContractorPriceFileRepo>().Create(new T_EB_BID_CONTRACTOR_PRICE_FILE()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                BID_ID = this.ObjDetail.BID_ID,
                                CONTRACTOR_ID = contractorId,
                                FILE_ID = item.Key.REFERENCE_ID,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });
                        }
                    }
                }

                foreach (var item in this.ObjList.Where(x => x.TYPE == "2").OrderBy(x => x.C_ORDER))
                {
                    item.CURRENCY_CODE = "VND";
                    if (item.TYPE == "2")
                    {
                        if (string.IsNullOrWhiteSpace(item.PKID) && item.IS_DELETE == null)
                        {
                            item.PKID = Guid.NewGuid().ToString();
                            item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                            this.CurrentRepository.Create(item);
                        }
                        if (!string.IsNullOrWhiteSpace(item.PKID) && item.IS_DELETE == null)
                        {
                            item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                            this.CurrentRepository.Update(item);
                        }
                        if (!string.IsNullOrWhiteSpace(item.PKID) && item.IS_DELETE == "1")
                        {
                            this.CurrentRepository.Delete(item.PKID);
                        }
                    }
                    //else if (item.TYPE == "3")
                    //{
                    //    item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    //    this.CurrentRepository.Update(item);
                    //}

                }
                var bid = UnitOfWork.Repository<BidRepo>().Get(this.ObjDetail.BID_ID);
                if (bid != null)
                {
                    if (bid.GIAI_DOAN == "08" && isTT == "2")
                    {
                        bid.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        bid.GIAI_DOAN = Phase_Status.ChonNhaThau;
                        bid.STATUS = EB_Status.DaPheDuyet;
                        UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            ACTION = Action.KetThucThuongThao,
                            ACTION_USER = ProfileUtilities.User.USER_NAME,
                            ACTION_DATE = DateTime.Now,
                            BID_ID = bid.PKID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });

                        #region Tạo thông báo 

                        var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == bid.PKID && x.IS_QUYET_DINH == true).ToList();
                        var lstUserNotify = new List<string>();
                        var serviceNotify = new NotifyService();

                        foreach (var council in lstCouncil)
                        {
                            var newId = Guid.NewGuid().ToString();
                            lstUserNotify.Add(council.USER_NAME);

                            string strTemplate = @"
                                <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                                    <div class='icon-circle {2}'>
                                        <i class='material-icons'>format_list_numbered</i>
                                    </div>
                                    <div class='menu-info'>
                                        <span>Gói thầu [{4} - {5}] đã thương thảo lại xong !</span>
                                        <p>
                                            <i class='material-icons'>access_time</i> {6}
                                        </p>
                                    </div>
                                </a>
                            ";

                            string strTemplate_EN = @"
                                <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                                    <div class='icon-circle {2}'>
                                        <i class='material-icons'>format_list_numbered</i>
                                    </div>
                                    <div class='menu-info'>
                                        <span>Bid [{4} - {5}] negotiations have been completed !</span>
                                        <p>
                                            <i class='material-icons'>access_time</i> {6}
                                        </p>
                                    </div>
                                </a>
                            ";

                            string strContent = string.Format(strTemplate,
                                newId,
                                $"/EB/Bid/MenuDetailBidManager?id={bid.PKID}&idMenu=mnuChonNhaThau",
                                EB_Status.GetStatusColor(bid.STATUS),
                                "how_to_reg",
                                bid.PKID,
                                bid.TEN_GOI_THAU,
                                DateTime.Now.ToString(Global.DateTimeToStringFormat));
                            string strContent_EN = string.Format(strTemplate_EN,
                                newId,
                                $"/EB/Bid/MenuDetailBidManager?id={bid.PKID}&idMenu=mnuChonNhaThau",
                                EB_Status.GetStatusColor(bid.STATUS),
                                "how_to_reg",
                                bid.PKID,
                                bid.TEN_GOI_THAU,
                                DateTime.Now.ToString(Global.DateTimeToStringFormat));

                            string strRawContent = $"Gói thầu {bid.PKID} đã thương thảo lại xong đề nghị chọn lại nhà thầu";
                            string strRawContent_EN = $"Gói thầu {bid.PKID} After re-negotiating, we proposed to re-select the contractor";

                            serviceNotify.ObjDetail = new T_CM_NOTIFY()
                            {
                                PKID = newId,
                                CONTENTS = strContent,
                                RAW_CONTENTS = strRawContent,
                                CONTENTS_EN = strContent_EN,
                                RAW_CONTENTS_EN = strRawContent_EN,
                                USER_NAME = council.USER_NAME
                            };
                            serviceNotify.Create();
                        }

                        SMOUtilities.SendNotify(lstUserNotify);

                        #endregion
                        #region Gửi thông báo qua Mail
                        string LOAI_CONG_VIEC = "{LOAI_CONG_VIEC}";
                        string MA = "{MA}";
                        string TEN = "{TEN}";
                        string USER_NAME_NGUOI_DE_XUAT = "{USER_NAME_NGUOI_DE_XUAT}";
                        string HO_TEN_NGUOI_DE_XUAT = "{HO_TEN_NGUOI_DE_XUAT}";
                        string USER_NAME_NGUOI_PHE_DUYET = "{USER_NAME_NGUOI_PHE_DUYET}";
                        string HO_TEN_NGUOI_PHE_DUYET = "{HO_TEN_NGUOI_PHE_DUYET}";
                        string DU_AN = "{DU_AN}";
                        string DU_AN_EN = "{DU_AN_EN}";
                        string TRANG_THAI = "{TRANG_THAI}";
                        string LINK_CHI_TIET = "{LINK_CHI_TIET}";

                        var serviceTemplate = new ConfigTemplateNotifyService();
                        serviceTemplate.GetAll();
                        if (serviceTemplate.ObjList.Count() == 0)
                        {
                            return;
                        }

                        var serviceConfig = new SystemConfigService();
                        serviceConfig.GetConfig();

                        var template = serviceTemplate.ObjList.FirstOrDefault();

                        var serviceEmail = new EmailNotifyService();
                        var url = ConfigurationManager.AppSettings["URL_WEB"];

                        var subjectTemplate = template.CONG_VIEC_XU_LY_SUBJECT;
                        var bodyTemplate = template.CONG_VIEC_XU_LY_BODY;

                        var contentSubject = subjectTemplate
                                .Replace(LOAI_CONG_VIEC, "Gói thầu")
                                .Replace(MA, bid.PKID)
                                .Replace(TEN, bid.TEN_GOI_THAU)
                                .Replace(TRANG_THAI, "Thương thảo lại xong")
                                .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + bid.PKID + "&idMenu=mnuChonNhaThau")
                                .Replace(USER_NAME_NGUOI_DE_XUAT, bid.CREATE_BY)
                                .Replace(USER_NAME_NGUOI_PHE_DUYET, bid.USER_APPROVE)
                                .Replace(HO_TEN_NGUOI_DE_XUAT, bid.UserSent.FULL_NAME)
                                .Replace(DU_AN, bid.Project.NAME)
                                .Replace(DU_AN_EN, bid.Project.NAME_EN)
                                .Replace(HO_TEN_NGUOI_PHE_DUYET, bid.UserApprove.FULL_NAME);

                        if (bid.UserSent.Organize != null)
                        {
                            contentSubject = contentSubject.Replace(DU_AN, bid.Project.NAME);
                        }

                        var contentBody = bodyTemplate
                                .Replace(LOAI_CONG_VIEC, "Gói thầu")
                                .Replace(MA, bid.PKID)
                                .Replace(TEN, bid.TEN_GOI_THAU)
                                .Replace(TRANG_THAI, "Duyệt lại giá thương thảo")
                                .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + bid.PKID + "&idMenu=mnuChonNhaThau")
                                .Replace(USER_NAME_NGUOI_DE_XUAT, bid.CREATE_BY)
                                .Replace(USER_NAME_NGUOI_PHE_DUYET, bid.USER_APPROVE)
                                .Replace(HO_TEN_NGUOI_DE_XUAT, bid.UserSent.FULL_NAME)
                                .Replace(DU_AN, bid.Project.NAME)
                                .Replace(DU_AN_EN, bid.Project.NAME_EN)
                                .Replace(HO_TEN_NGUOI_PHE_DUYET, bid.UserApprove.FULL_NAME);

                        if (bid.UserSent.Organize != null)
                        {
                            contentBody = contentBody.Replace(DU_AN, bid.Project.NAME);
                        }

                        foreach (var userQuyetDinh in lstCouncil)
                        {
                            serviceEmail.ObjDetail = new T_CM_EMAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                EMAIL = userQuyetDinh.User.EMAIL,
                                SUBJECT = contentSubject,
                                CONTENTS = contentBody
                            };
                        }
                        serviceEmail.Create();
                        #endregion
                    }
                }

                // Tạo history
                //UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                //{
                //    PKID = Guid.NewGuid().ToString(),
                //    ACTION = Action.ChinhSua,
                //    ACTION_USER = ProfileUtilities.User.USER_NAME,
                //    ACTION_DATE = DateTime.Now,
                //    BID_ID = this.ObjList.Count > 0 ? this.ObjList.FirstOrDefault().BID_ID : "",
                //    NOTES = "Cập nhật bảng giá chào thầu",
                //    CREATE_BY = ProfileUtilities.User.USER_NAME
                //});
                UnitOfWork.Commit();
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
