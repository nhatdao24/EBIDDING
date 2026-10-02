using Microsoft.Graph.Models.Security;
using Newtonsoft.Json;
using NHibernate.Linq;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using SMO.Core.Entities;
using SMO.Core.Entities.AD;
using SMO.Repository.Common;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.CM;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Service.AD;
using SMO.Service.CF;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.EnterpriseServices;
using System.IdentityModel.Protocols.WSTrust;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SMO.Service.EB
{
    public class BidService : GenericService<T_EB_BID, BidRepo>
    {
        public List<T_EB_BID_CONTRACTOR> ObjListBidContractor { get; set; }
        public IList<T_EB_BID_REQUIRE_FILE> ListRequireFile { get; set; }
        public List<T_EB_BID_CONTRACTOR_PRICE> ObjListPrice { get; set; }
        public virtual string ContractorId { get; set; }
        public BidService() : base()
        {
            ObjListBidContractor = new List<T_EB_BID_CONTRACTOR>();
            ObjListPrice = new List<T_EB_BID_CONTRACTOR_PRICE>();
            ListRequireFile = new List<T_EB_BID_REQUIRE_FILE>();
        }

        #region Quản lý
        public override void Search()
        {
            //ConfigViettinBank: Xóa dự án 

            //Tìm xem user thuộc dự án nào
            //var projectCode = this.ObjDetail.PROJECT_CODE;
            //if (projectCode == "" || projectCode == ",")
            //{
            //    this.ObjDetail.PROJECT_CODE = null;
            //}
            //this.ObjDetail.ListFilterProject = UnitOfWork.GetSession().Query<T_AD_USER_PROJECT>().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).Select(x => x.PROJECT_CODE).ToList();
            base.Search();
        }

        public void Search_ProcessByMe()
        {
            try
            {
                int iTotalRecord = 0;
                this.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;

                //ConfigViettinBank: Xóa dự án

                //var projectCode = this.ObjDetail.PROJECT_CODE;
                //if (projectCode == "" || projectCode == ",")
                //{
                //    this.ObjDetail.PROJECT_CODE = null;
                //}
                //this.ObjDetail.ListFilterProject = UnitOfWork.GetSession().Query<T_AD_USER_PROJECT>().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).Select(x => x.PROJECT_CODE).ToList();
                this.ObjList = UnitOfWork.Repository<BidRepo>().Search_ProcessByMe(this.ObjDetail, this.NumerRecordPerPage, this.Page, out iTotalRecord).ToList();
                this.TotalRecord = iTotalRecord;
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void AutoSendMail2DayEndBid()
        {
            try
            {
                List<string> lstBidId = new List<string>();
                //Lấy ra tất cả các review đang đánh giá
                var lstBid = this.CurrentRepository.Queryable().Where(x => x.STATUS == EB_Status.DaPheDuyet && x.GIAI_DOAN == Phase_Status.ChamThau).ToList();
                var dateNow = DateTime.Now;
                foreach (var item in lstBid)
                {
                    if (item.NGAY_KT_CHAM_THAU == dateNow.AddDays(2))
                    {
                        this.CreateNotiEmail2NgayKetThuc(item.PKID);
                    }
                }
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        /// <summary>
        /// Gửi email nhắc nhở còn 2 ngày kết thúc chấm thầu
        /// </summary>
        public void CreateNotiEmail2NgayKetThuc(string bidId)
        {
            try
            {
                var lstMailNotify = new List<string>();
                var bid = this.CurrentRepository.Get(bidId);
                var lstCouncilDone = UnitOfWork.Repository<BidSpecCouncilScoreRepo>().Queryable().Where(x => x.BID_ID == bidId)
                    .Select(x => x.USER_SCORE).Distinct()
                    .ToList();
                var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == bidId && !lstCouncilDone.Contains(x.USER_NAME))
                    .ToList();

                var lstCouncilUserName = lstCouncil.Select(x => x.USER_NAME).ToList();
                var lstCouncilEmail = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => lstCouncilUserName.Contains(x.USER_NAME)).Select(x => x.EMAIL).ToList();
                lstMailNotify.AddRange(lstCouncilEmail);

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
                        .Replace(DU_AN, bid.Project.NAME)
                        .Replace(DU_AN_EN, bid.Project.NAME_EN)
                        .Replace(TRANG_THAI, EB_Status.GetStatusText(bid.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + bid.PKID)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, bid.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, bid.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, bid.UserSent.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, bid.UserApprove.FULL_NAME);

                if (bid.UserSent.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DU_AN, bid.Project.NAME);
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Gói thầu")
                        .Replace(MA, bid.PKID)
                        .Replace(TEN, bid.TEN_GOI_THAU)
                        .Replace(DU_AN, bid.Project.NAME)
                        .Replace(DU_AN_EN, bid.Project.NAME_EN)
                        .Replace(TRANG_THAI, EB_Status.GetStatusText(bid.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + bid.PKID)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, bid.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, bid.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, bid.UserSent.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, bid.UserApprove.FULL_NAME);

                if (bid.UserSent.Organize != null)
                {
                    contentBody = contentBody.Replace(DU_AN, bid.Project.NAME);
                }
                UnitOfWork.BeginTransaction();
                foreach (var mail in lstMailNotify)
                {
                    serviceEmail.ObjDetail = new T_CM_EMAIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        EMAIL = mail,
                        SUBJECT = contentSubject,
                        CONTENTS = contentBody
                    };
                    serviceEmail.Create();
                }
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.ErrorMessage = "Có lỗi trong quá trình gửi email";
            }
        }

        /// <summary>
        /// Kiểm tra các ngày có theo đúng thứ tự hay ko
        /// </summary>
        /// <returns></returns>
        public void ValidateDate()
        {
            var check = true;

            if (this.ObjDetail.NGAY_DONG_THAU.Date <= this.ObjDetail.NGAY_MO_THAU.Date)
            {
                check = false;
            }
            if (this.ObjDetail.NGAY_KT_CHAM_THAU?.Date <= this.ObjDetail.NGAY_MO_THAU.Date)
            {
                check = false;
            }

            //if(this.ObjDetail.NGAY_KT_CHAM_THAU == null)
            //{
            //    check = false;
            //}

            if (this.ObjDetail.NGAY_KT_CHAM_THAU?.Date <= this.ObjDetail.NGAY_DONG_THAU.Date || this.ObjDetail.NGAY_KT_CHAM_THAU?.Date <= this.ObjDetail.NGAY_MO_THAU.Date)
            {
                check = false;
            }

            if (!check)
            {
                this.State = false;
                this.ErrorMessage = "Thứ tự các ngày chưa đúng. Ngày bắt đầu nộp hồ sơ < Ngày mở thầu < Ngày đóng thầu < Ngày bắt đầu chấm thầu < Ngày kết thúc chấm thầu" ;
            }
        }



        public void ValidateDateVtb()
        {
            var check = true;

            if (this.ObjDetail.NGAY_DONG_THAU.Date <= this.ObjDetail.NGAY_MO_THAU.Date)
            {
                check = false;
            }


            if (!check)
            {
                this.State = false;
                this.ErrorMessage = "Thứ tự các ngày chưa đúng. Ngày bắt đầu nộp hồ sơ < Ngày mở thầu < Ngày đóng thầu < Ngày bắt đầu chấm thầu";
            }
        }

        public async Task Update(HttpRequestBase Request)
        {
            try
            {
                this.ValidateDate();
                if (!this.State)
                {
                    return;
                }

                if (this.ObjDetail.NGAY_KT_CHAM_THAU == null)
                {
                    this.State = false;
                    this.ErrorMessage = "Ngày kết thúc chấm thầu không được để trống !";
                    return;
                }


                if (this.ObjDetail.GIAI_DOAN == "01")
                {
                    // Lưu file vào database
                    var lstFileStream = new List<FILE_STREAM>();
                    for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                    {
                        var file = Request.Files[i];
                        var code = Guid.NewGuid().ToString();
                        var referenceCode = Guid.NewGuid().ToString();

                        lstFileStream.Add(new FILE_STREAM()
                        {
                            PKID = code,
                            FILE_OLD_NAME = file.FileName,
                            REFERENCE_ID = referenceCode,
                            FILE_EXT = Path.GetExtension(file.FileName),
                            FILE_SIZE = file.ContentLength,
                            FILESTREAM = Request.Files[i]
                        });
                    }
                    await FileStreamService.InsertFile(lstFileStream);

                    // Xóa file database file
                    //FileStreamService.DeleteFile(this.ObjDetail.ListFileDelete);

                    // Lưu thông tin gói thầu
                    UnitOfWork.BeginTransaction();

                    // Xóa file
                    if (!string.IsNullOrEmpty(this.ObjDetail.ListFileDelete))
                    {
                        var strDelete = "";

                        foreach (var item in this.ObjDetail.ListFileDelete.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            strDelete += $"DELETE T_EB_BID_FILE WHERE FILE_ID = '{item}';";
                        }
                        UnitOfWork.GetSession().CreateSQLQuery(strDelete).ExecuteUpdate();
                    }

                    // Tạo file
                    foreach (var file in lstFileStream)
                    {
                        UnitOfWork.Repository<BidFileRepo>().Create(new T_EB_BID_FILE()
                        {
                            BID_ID = this.ObjDetail.PKID,
                            FILE_ID = file.REFERENCE_ID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }

                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    this.ObjDetail.TEN_GOI_THAU_KD = UtilsCore.ConvertToUnSign(this.ObjDetail.TEN_GOI_THAU);
                    this.ObjDetail.MO_TA_KD = UtilsCore.ConvertToUnSign(UtilsCore.StripHTML(this.ObjDetail.MO_TA));
                    this.CurrentRepository.Update(this.ObjDetail);

                    // Tạo history
                    UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        ACTION = Action.ChinhSua,
                        ACTION_USER = ProfileUtilities.User.USER_NAME,
                        ACTION_DATE = DateTime.Now,
                        BID_ID = this.ObjDetail.PKID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                    UnitOfWork.Commit();

                    var serviceUserApprove = new UserApproveService();
                    serviceUserApprove.Update(this.ObjDetail.USER_APPROVE, "BID");
                }
                else
                {
                    if (this.ObjDetail.NGAY_KT_CHAM_THAU != null)
                    {
                        UnitOfWork.BeginTransaction();
                        var bid = UnitOfWork.Repository<BidRepo>().Get(this.ObjDetail.PKID);

                        //ConfigViettinBank: Cho phép gia hạn thời gian đóng thầu



                        bid.NGAY_DONG_THAU = this.ObjDetail.NGAY_DONG_THAU;
                        bid.NGAY_KT_CHAM_THAU = this.ObjDetail.NGAY_KT_CHAM_THAU;
                        UnitOfWork.Repository<BidRepo>().Update(bid);
                        UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            ACTION = Action.ChinhSua,
                            ACTION_USER = ProfileUtilities.User.USER_NAME,
                            ACTION_DATE = DateTime.Now,
                            BID_ID = this.ObjDetail.PKID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                        UnitOfWork.Commit();
                    }
                    else
                    {
                        this.State = false;
                        this.ErrorMessage = "Giai đoạn này không thể cập nhật lại !";
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }


        public async Task UpdateVtb(HttpRequestBase Request)
        {
            try
            {
                this.ValidateDateVtb();
                if (!this.State)
                {
                    return;
                }

                var fileGroups = Request.Form.GetValues("fileGroups");

                if (this.ObjDetail.GIAI_DOAN == "01")
                {
                    // Lưu file vào database
                    var lstFileStream = new List<FILE_STREAM>();
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file.ContentLength == 0) continue;

                        var code = Guid.NewGuid().ToString();
                        var referenceCode = Guid.NewGuid().ToString();

                        lstFileStream.Add(new FILE_STREAM()
                        {
                            PKID = code,
                            FILE_OLD_NAME = file.FileName,
                            REFERENCE_ID = referenceCode,
                            FILE_EXT = Path.GetExtension(file.FileName),
                            FILE_SIZE = file.ContentLength,
                            FILESTREAM = file
                        });
                    }
                    FileStreamService.InsertFile(lstFileStream);

                    // Xóa file database file
                    //FileStreamService.DeleteFile(this.ObjDetail.ListFileDelete);

                    // Lưu thông tin gói thầu
                    UnitOfWork.BeginTransaction();

                    // Xóa file
                    if (!string.IsNullOrEmpty(this.ObjDetail.ListFileDelete))
                    {
                        var strDelete = "";

                        foreach (var item in this.ObjDetail.ListFileDelete.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            strDelete += $"DELETE T_EB_BID_FILE WHERE FILE_ID = '{item}';";
                        }
                        UnitOfWork.GetSession().CreateSQLQuery(strDelete).ExecuteUpdate();
                    }

                    // Tạo file
                    for (int i = 0; i < lstFileStream.Count; i++)
                    {
                        var fileData = lstFileStream[i];
                        string fileType = (fileGroups != null && fileGroups.Length > i) ? fileGroups[i] : "YC";

                        UnitOfWork.Repository<BidFileRepo>().Create(new T_EB_BID_FILE()
                        {
                            BID_ID = this.ObjDetail.PKID,
                            FILE_ID = fileData.REFERENCE_ID,
                            TYPE = fileType,
                            CREATE_BY = ProfileUtilities.User.USER_NAME,
                            CREATE_DATE = DateTime.Now
                        });
                    }

                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    this.ObjDetail.TEN_GOI_THAU_KD = UtilsCore.ConvertToUnSign(this.ObjDetail.TEN_GOI_THAU);
                    this.ObjDetail.MO_TA_KD = UtilsCore.ConvertToUnSign(UtilsCore.StripHTML(this.ObjDetail.MO_TA));
                    this.CurrentRepository.Update(this.ObjDetail);

                    // Tạo history
                    UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        ACTION = Action.ChinhSua,
                        ACTION_USER = ProfileUtilities.User.USER_NAME,
                        ACTION_DATE = DateTime.Now,
                        BID_ID = this.ObjDetail.PKID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                    UnitOfWork.Commit();

                    var serviceUserApprove = new UserApproveService();
                    serviceUserApprove.Update(this.ObjDetail.USER_APPROVE, "BID");
                }
                else
                {
                   
                        UnitOfWork.BeginTransaction();
                        var bid = UnitOfWork.Repository<BidRepo>().Get(this.ObjDetail.PKID);

                        //ConfigViettinBank: Cho phép gia hạn thời gian đóng thầu



                        bid.NGAY_DONG_THAU = this.ObjDetail.NGAY_DONG_THAU;
                        UnitOfWork.Repository<BidRepo>().Update(bid);
                        UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            ACTION = Action.ChinhSua,
                            ACTION_USER = ProfileUtilities.User.USER_NAME,
                            ACTION_DATE = DateTime.Now,
                            BID_ID = this.ObjDetail.PKID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                        UnitOfWork.Commit();
                 

                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }


        public void CreateVtb(HttpRequestBase Request, string lstContractorId, string hinhThucMuaSam)
        {
            try
            {
                this.ValidateDateVtb();
                if (!this.State) return;

                bool isPublicBool = (hinhThucMuaSam == "NHOM3");
                // 1. Lấy danh sách Type từ Form gửi lên
                var fileGroups = Request.Form.GetValues("fileGroups");

                var listFileBidJson = Request.Form["listFileBid"];
                var lstFileBid = JsonConvert.DeserializeObject<List<T_EB_BID_REQUIRE_FILE>>(listFileBidJson);

                var lstFileStream = new List<FILE_STREAM>();
                // Lưu trữ tạm mapping giữa ReferenceID và Type
                var dictFileType = new Dictionary<string, string>();

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    var referenceCode = Guid.NewGuid().ToString();
                    var groupType = (fileGroups != null && fileGroups.Length > i) ? fileGroups[i] : "";

                    lstFileStream.Add(new FILE_STREAM()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        FILE_OLD_NAME = file.FileName,
                        REFERENCE_ID = referenceCode,
                        FILE_EXT = Path.GetExtension(file.FileName),
                        FILE_SIZE = file.ContentLength,
                        FILESTREAM = file
                    });

                    // Lưu vết: File này thuộc nhóm nào
                    dictFileType.Add(referenceCode, groupType);
                }
                FileStreamService.InsertFile(lstFileStream);

                if (!this.ObjDetail.IS_NEW_ROUND)
                {
                    UnitOfWork.BeginTransaction();
                    this.ObjDetail.PKID = this.GetSequence("BID");
                    this.ObjDetail.PARENT_BID = this.ObjDetail.PKID;
                    this.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
                    this.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                    this.ObjDetail.STATUS = EB_Status.KhoiTao;
                    this.ObjDetail.COMMENT_ID = Guid.NewGuid().ToString();
                    this.ObjDetail.GIAI_DOAN = Phase_Status.KhoiTao;
                    this.ObjDetail.TEN_GOI_THAU_KD = UtilsCore.ConvertToUnSign(this.ObjDetail.TEN_GOI_THAU);
                    this.ObjDetail.MO_TA_KD = UtilsCore.ConvertToUnSign(UtilsCore.StripHTML(this.ObjDetail.MO_TA));
                    this.ObjDetail.ROUND = 1;
                    this.ObjDetail.IS_MULTIL_ROUND = false;
                    this.ObjDetail.BID_TYPE = hinhThucMuaSam;

                    // Tạo tin tức
                    //var content = "<p>&nbsp;</p><p> &nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><br/>HỒ SƠ THẦU BAO GỒM:</p><p>-Danh mục đề xuất vật tư sử dụng <br/>-Tiến độ thi công, giao hàng <br/>-Báo giá chào thầu</p>";
                    UnitOfWork.Repository<BidNewRepo>().Create(new T_EB_BID_NEW()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        IS_PUBLIC = isPublicBool,
                        BID_ID = this.ObjDetail.PKID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });

                    // Tạo file
                    foreach (var file in lstFileStream)
                    {
                        UnitOfWork.Repository<BidFileRepo>().Create(new T_EB_BID_FILE()
                        {
                            BID_ID = this.ObjDetail.PKID,
                            FILE_ID = file.REFERENCE_ID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME,
                            TYPE = dictFileType[file.REFERENCE_ID] 
                        });
                    }

                    foreach (var FileBid in lstFileBid)
                    {
                        UnitOfWork.Repository<BidFileRepo>().Create(new T_EB_BID_FILE()
                        {
                            BID_ID = this.ObjDetail.PKID,
                            FILE_ID = FileBid.FILE_ID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }



                    // Tạo header
                    this.CurrentRepository.Create(this.ObjDetail);



                    var strListDetail = Request.Form["strListDetail"];
                    if (!string.IsNullOrEmpty(strListDetail))
                    {
                        var listDetails = JsonConvert.DeserializeObject<List<T_EB_BID_HHDV>>(strListDetail);

                        if (listDetails != null && listDetails.Count > 0)
                        {
                            foreach (var item in listDetails)
                            {
                                item.PKID = Guid.NewGuid().ToString();
                                item.BID_ID = this.ObjDetail.PKID; 
                                item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                                item.CREATE_DATE = DateTime.Now;

                                if (string.IsNullOrEmpty(item.QUANTITY)) item.QUANTITY = "0";
                            }

                            UnitOfWork.Repository<BidHHDVRepo>().Create(listDetails);
                        }
                    }

                    //Tạo tiêu chí mặc định
                    var lstMdSpecDefault = UnitOfWork.Repository<MdBidSpecRepo>().GetAll();
                    var lstMdSpecDetail = UnitOfWork.Repository<MdBidSpecDetailRepo>().GetAll();
                    List<T_EB_BID_SPEC> lstBidSpec = new List<T_EB_BID_SPEC>();
                    List<T_EB_BID_SPEC_DETAIL> lstBidSpecDetail = new List<T_EB_BID_SPEC_DETAIL>();
                    foreach (var item in lstMdSpecDefault.Where(x => x.IS_DEFAULT == true && x.PKID != Global.IdSpecPrice).OrderBy(x => x.C_ORDER).ToList())
                    {
                        if (lstBidSpec.Count(x => x.MD_PKID == item.PKID) == 0)
                        {
                            var bidSpecId = Guid.NewGuid().ToString();
                            lstBidSpec.Add(new T_EB_BID_SPEC()
                            {
                                PKID = bidSpecId,
                                BID_ID = this.ObjDetail.PKID,
                                NAME = item.NAME,
                                NAME_EN = item.NAME_EN,
                                DESCRIPTION = item.DESCRIPTION,
                                DESCRIPTION_EN = item.DESCRIPTION_EN,
                                TYPE = item.TYPE,
                                C_ORDER = item.C_ORDER,
                                IS_SPEC_PRICE = item.IS_SPEC_PRICE,
                                IS_NOT_SHOW_CONTRACTOR = item.IS_NOT_SHOW_CONTRACTOR,
                                ROLE = item.ROLE,
                                MD_PKID = item.PKID,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });

                            if (lstMdSpecDefault.Count(x => x.PARENT_ID == item.PKID) > 0)
                            {
                                foreach (var child in lstMdSpecDefault.Where(x => x.PARENT_ID == item.PKID).OrderBy(x => x.C_ORDER).ToList())
                                {
                                    var bidSpecId2 = Guid.NewGuid().ToString();
                                    lstBidSpec.Add(new T_EB_BID_SPEC()
                                    {
                                        PKID = bidSpecId2,
                                        PARENT_ID = bidSpecId,
                                        BID_ID = this.ObjDetail.PKID,
                                        NAME = child.NAME,
                                        NAME_EN = child.NAME_EN,
                                        DESCRIPTION = child.DESCRIPTION,
                                        DESCRIPTION_EN = child.DESCRIPTION_EN,
                                        TYPE = child.TYPE,
                                        C_ORDER = child.C_ORDER,
                                        IS_SPEC_PRICE = child.IS_SPEC_PRICE,
                                        IS_NOT_SHOW_CONTRACTOR = child.IS_NOT_SHOW_CONTRACTOR,
                                        ROLE = child.ROLE,
                                        MD_PKID = child.PKID,
                                        CREATE_BY = ProfileUtilities.User.USER_NAME
                                    });

                                    foreach (var detail in lstMdSpecDetail.Where(x => x.HEADER_ID == child.PKID).ToList())
                                    {
                                        lstBidSpecDetail.Add(new T_EB_BID_SPEC_DETAIL()
                                        {
                                            PKID = Guid.NewGuid().ToString(),
                                            HEADER_ID = bidSpecId2,
                                            FROM_VALUE = detail.FROM_VALUE,
                                            TO_VALUE = detail.TO_VALUE,
                                            VALUE = detail.VALUE,
                                            VALUE_EN = detail.VALUE_EN,
                                            SCORE = detail.SCORE,
                                            C_ORDER = detail.C_ORDER,
                                            CREATE_BY = ProfileUtilities.User.USER_NAME
                                        });
                                    }
                                }
                            }

                            //Add Detail
                            foreach (var detail in lstMdSpecDetail.Where(x => x.HEADER_ID == item.PKID).ToList())
                            {
                                lstBidSpecDetail.Add(new T_EB_BID_SPEC_DETAIL()
                                {
                                    PKID = Guid.NewGuid().ToString(),
                                    HEADER_ID = bidSpecId,
                                    FROM_VALUE = detail.FROM_VALUE,
                                    TO_VALUE = detail.TO_VALUE,
                                    VALUE = detail.VALUE,
                                    VALUE_EN = detail.VALUE_EN,
                                    SCORE = detail.SCORE,
                                    C_ORDER = detail.C_ORDER
                                });
                            }
                        }
                    }
                    UnitOfWork.Repository<BidSpecRepo>().Create(lstBidSpec);
                    UnitOfWork.Repository<BidSpecDetailRepo>().Create(lstBidSpecDetail);
                    //Tạo tiêu chí giá tổng chào thầu
                    var MdBidSpecPrice = UnitOfWork.Repository<MdBidSpecRepo>().Queryable().FirstOrDefault(x => x.PKID == Global.IdSpecPrice);
                    var specVAT = "a90cb897-0e4a-40c9-8be9-f31005b51c18";
                    var ratioVat = 0.00m;
                    if (MdBidSpecPrice.PKID == specVAT)
                    {
                        ratioVat = 100.00m;
                    }
                    UnitOfWork.Repository<BidSpecRepo>().Create(new T_EB_BID_SPEC()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        BID_ID = this.ObjDetail.PKID,
                        NAME = MdBidSpecPrice.NAME,
                        NAME_EN = MdBidSpecPrice.NAME_EN,
                        DESCRIPTION = MdBidSpecPrice.DESCRIPTION,
                        DESCRIPTION_EN = MdBidSpecPrice.DESCRIPTION_EN,
                        TYPE = MdBidSpecPrice.TYPE,
                        RATIO = ratioVat,
                        C_ORDER = MdBidSpecPrice.C_ORDER,
                        IS_SPEC_PRICE = MdBidSpecPrice.IS_SPEC_PRICE,
                        IS_NOT_SHOW_CONTRACTOR = MdBidSpecPrice.IS_NOT_SHOW_CONTRACTOR,
                        ROLE = MdBidSpecPrice.ROLE,
                        MD_PKID = MdBidSpecPrice.PKID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });

                    var requireCode = this.GetSequence("BID_REQUIRED");
                    requireCode = this.ObjDetail.PKID;

                    // Tạo history
                    UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        ACTION = Action.TaoMoi,
                        ACTION_USER = ProfileUtilities.User.USER_NAME,
                        ACTION_DATE = DateTime.Now,
                        BID_ID = this.ObjDetail.PKID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                    UnitOfWork.Commit();

                    var serviceUserApprove = new UserApproveService();
                    serviceUserApprove.Update(this.ObjDetail.USER_APPROVE, "BID");
                }
                else
                {
                    var lstContractorIdToNotify = new List<string>();

                    var parentBid = this.CurrentRepository.Get(this.ObjDetail.PARENT_BID);
                    var latestRound = this.CurrentRepository.Queryable().Where(x => x.PARENT_BID == this.ObjDetail.PARENT_BID).OrderByDescending(x => x.ROUND).FirstOrDefault();
                    var latestRoundNews = UnitOfWork.Repository<BidNewRepo>().Queryable().FirstOrDefault(x => x.BID_ID == latestRound.PKID);

                    var listCouncil = new List<T_EB_BID_COUNCIL>();
                    var listSpec = new List<T_EB_BID_SPEC>();
                    var listBidFile = new List<T_EB_BID_FILE>();

                    // Lưu thông tin gói thầu
                    UnitOfWork.BeginTransaction();
                    this.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                    this.ObjDetail.STATUS = EB_Status.KhoiTao;
                    this.ObjDetail.COMMENT_ID = Guid.NewGuid().ToString();
                    this.ObjDetail.GIAI_DOAN = Phase_Status.KhoiTao;
                    this.ObjDetail.TEN_GOI_THAU_KD = UtilsCore.ConvertToUnSign(this.ObjDetail.TEN_GOI_THAU);
                    this.ObjDetail.MO_TA_KD = UtilsCore.ConvertToUnSign(UtilsCore.StripHTML(this.ObjDetail.MO_TA));
                    this.ObjDetail.IS_MULTIL_ROUND = true;
                    this.ObjDetail.BID_TYPE = hinhThucMuaSam;
                    if (parentBid.IS_MULTIL_ROUND == null || !parentBid.IS_MULTIL_ROUND)
                    {
                        this.ObjDetail.ROUND = 2;

                        //Lấy dữ liệu copy
                        listCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == parentBid.PKID).ToList();
                        listSpec = UnitOfWork.Repository<BidSpecRepo>().Queryable().Where(x => x.BID_ID == parentBid.PKID).ToList();
                        listBidFile = UnitOfWork.Repository<BidFileRepo>().Queryable().Where(x => x.BID_ID == parentBid.PKID).ToList();
                    }
                    else if (parentBid.IS_MULTIL_ROUND && (latestRound != null || latestRound.ROUND > 2))
                    {
                        this.ObjDetail.ROUND = latestRound.ROUND + 1;

                        //Lấy dữ liệu copy
                        listCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == latestRound.PKID).ToList();
                        listSpec = UnitOfWork.Repository<BidSpecRepo>().Queryable().Where(x => x.BID_ID == latestRound.PKID).ToList();
                        listBidFile = UnitOfWork.Repository<BidFileRepo>().Queryable().Where(x => x.BID_ID == latestRound.PKID).ToList();
                    }
                    this.ObjDetail.PKID = this.ObjDetail.PARENT_BID + "-" + this.ObjDetail.ROUND;

                    //Update parent Bid
                    if (!parentBid.IS_MULTIL_ROUND)
                    {
                        parentBid.GIAI_DOAN = Phase_Status.VongTiepTheo;
                        parentBid.IS_MULTIL_ROUND = true;
                        parentBid.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        parentBid.UPDATE_DATE = DateTime.Now;
                        this.CurrentRepository.Update(parentBid);
                    }

                    //Update latestRound
                    if (latestRound.GIAI_DOAN != Phase_Status.DongThau)
                    {
                        latestRound.GIAI_DOAN = Phase_Status.VongTiepTheo;
                        latestRound.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    }

                    // Tạo file
                    foreach (var file in lstFileStream)
                    {
                        UnitOfWork.Repository<BidFileRepo>().Create(new T_EB_BID_FILE()
                        {
                            BID_ID = this.ObjDetail.PKID,
                            FILE_ID = file.REFERENCE_ID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME,
                            TYPE = dictFileType[file.REFERENCE_ID]
                        });
                    }

                    // Copy file hồ sơ thầu
                    foreach (var item in listBidFile)
                    {
                        var fileID = Guid.NewGuid().ToString();
                        UnitOfWork.Repository<FileUploadRepo>().Create(new T_CM_FILE_UPLOAD()
                        {
                            PKID = fileID,
                            REFERENCE_ID = item.FileUpload.REFERENCE_ID,
                            FILE_EXT = item.FileUpload.FILE_EXT,
                            FILE_SIZE = item.FileUpload.FILE_SIZE,
                            FILE_OLD_NAME = item.FileUpload.FILE_OLD_NAME,
                            CONNECTION_ID = item.FileUpload.CONNECTION_ID,
                            DATABASE_NAME = item.FileUpload.DATABASE_NAME,
                            DIRECTORY_PATH = item.FileUpload.DIRECTORY_PATH,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                        UnitOfWork.Repository<BidFileRepo>().Create(new T_EB_BID_FILE()
                        {
                            BID_ID = this.ObjDetail.PKID,
                            FILE_ID = fileID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }

                    // Tạo Bid Contractor
                    if (!string.IsNullOrEmpty(lstContractorId))
                    {
                        foreach (var item in lstContractorId.Split('|'))
                        {
                            UnitOfWork.Repository<BidContractorRepo>().Create(new T_EB_BID_CONTRACTOR()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                BID_ID = this.ObjDetail.PKID,
                                CONTRACTOR_ID = item,
                                STATUS = "AP",
                                IS_SELECTED = false,
                                IS_WINNING = false,
                                IS_SELF_JOIN = true,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });
                            lstContractorIdToNotify.Add(item);
                        }
                    }


                    // Tạo tin tức
                    //var content = "<p>&nbsp;</p><p> &nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><br/>HỒ SƠ THẦU BAO GỒM:</p><p>-Danh mục đề xuất vật tư sử dụng <br/>-Tiến độ thi công, giao hàng <br/>-Báo giá chào thầu</p>";
                    UnitOfWork.Repository<BidNewRepo>().Create(new T_EB_BID_NEW()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        IS_PUBLIC = isPublicBool,
                        BID_ID = this.ObjDetail.PKID,
                        BAI_VIET = this.ObjDetail.MO_TA,
                        BAI_VIET_EN = this.ObjDetail.MO_TA_EN,
                        IMAGE_PATH = latestRoundNews.IMAGE_PATH,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });

                    // Tạo Bid Council
                    foreach (var item in listCouncil)
                    {
                        UnitOfWork.Repository<BidCouncilRepo>().Create(new T_EB_BID_COUNCIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            BID_ID = this.ObjDetail.PKID,
                            USER_NAME = item.USER_NAME,
                            VAI_TRO = item.VAI_TRO,
                            IS_QUYET_DINH = item.IS_QUYET_DINH,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }

                    //Tạo Bid Spec
                    var listHeaderID = listSpec.Select(x => x.PKID).ToList();
                    var listDetail = UnitOfWork.Repository<BidSpecDetailRepo>().Queryable().Where(x => listHeaderID.Contains(x.HEADER_ID)).ToList();
                    foreach (var item in listSpec)
                    {
                        var lstChild = listSpec.Where(x => x.PARENT_ID == item.PKID).ToList();
                        var specID = "";
                        var parentID = "";
                        if (this.ObjDetail.ROUND > 2)
                        {
                            var newID = item.PKID.Remove(item.PKID.Length - 2);
                            var newParentID = "";
                            specID = newID + "r" + this.ObjDetail.ROUND;
                            if (!string.IsNullOrEmpty(item.PARENT_ID))
                            {
                                newParentID = item.PARENT_ID.Remove(item.PARENT_ID.Length - 2);
                                parentID = newParentID + "r" + this.ObjDetail.ROUND;
                            }
                        }
                        else
                        {
                            specID = item.PKID + "-r" + this.ObjDetail.ROUND;
                            if (!string.IsNullOrEmpty(item.PARENT_ID))
                            {
                                parentID = item.PARENT_ID + "-r" + this.ObjDetail.ROUND;
                            }
                        }
                        UnitOfWork.Repository<BidSpecRepo>().Create(new T_EB_BID_SPEC()
                        {
                            PKID = specID,
                            PARENT_ID = parentID,
                            BID_ID = this.ObjDetail.PKID,
                            NAME = item.NAME,
                            NAME_EN = item.NAME_EN,
                            DESCRIPTION = item.DESCRIPTION,
                            DESCRIPTION_EN = item.DESCRIPTION_EN,
                            TYPE = item.TYPE,
                            C_ORDER = item.C_ORDER,
                            RATIO = item.RATIO,
                            IS_SPEC_PRICE = item.IS_SPEC_PRICE,
                            IS_NOT_SHOW_CONTRACTOR = item.IS_NOT_SHOW_CONTRACTOR,
                            ROLE = item.ROLE,
                            MD_PKID = item.MD_PKID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });

                        //Tạo Bid Spec Detail
                        var lstSpecDetail = listDetail.Where(x => x.HEADER_ID == item.PKID).ToList();
                        foreach (var detail in lstSpecDetail)
                        {
                            UnitOfWork.Repository<BidSpecDetailRepo>().Create(new T_EB_BID_SPEC_DETAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                HEADER_ID = specID,
                                FROM_VALUE = detail.FROM_VALUE,
                                TO_VALUE = detail.TO_VALUE,
                                VALUE = detail.VALUE,
                                VALUE_EN = detail.VALUE_EN,
                                SCORE = detail.SCORE,
                                C_ORDER = detail.C_ORDER,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });
                        }
                    }

                    // Tạo header
                    this.CurrentRepository.Create(this.ObjDetail);

                    // Tạo history
                    UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        ACTION = Action.TaoMoi,
                        ACTION_USER = ProfileUtilities.User.USER_NAME,
                        ACTION_DATE = DateTime.Now,
                        BID_ID = this.ObjDetail.PKID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                    UnitOfWork.Commit();

                    var serviceUserApprove = new UserApproveService();
                    serviceUserApprove.Update(this.ObjDetail.USER_APPROVE, "BID");

                    var lstContractor = UnitOfWork.Repository<ContractorRepo>().Queryable().Where(x => lstContractorIdToNotify.Contains(x.PKID)).ToList();
                    this.CreateNotifyNextRound(lstContractorIdToNotify);
                    this.CreateInviteEmailContractor(lstContractor);
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }


        public void Create(HttpRequestBase Request, string lstContractorId)
        {

            //ConfigViettinBank: Xoá phân quyền dự án

            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP302", this.ObjDetail.PROJECT_CODE);
            //if (!checkRight.Item1)
            //{
            //    this.State = false;
            //    this.ErrorMessage = checkRight.Item2;
            //    return;
            //}

            try
            {
                this.ValidateDate();
                if (!this.State)
                {
                    return;
                }

                // Lưu file vào database
                var listFileBidJson = Request.Form["listFileBid"];
                var lstFileBid = JsonConvert.DeserializeObject<List<T_EB_BID_REQUIRE_FILE>>(listFileBidJson);

                var lstFileStream = new List<FILE_STREAM>();
                for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                {
                    var file = Request.Files[i];
                    var code = Guid.NewGuid().ToString();
                    var referenceCode = Guid.NewGuid().ToString();

                    lstFileStream.Add(new FILE_STREAM()
                    {
                        PKID = code,
                        FILE_OLD_NAME = file.FileName,
                        REFERENCE_ID = referenceCode,
                        FILE_EXT = Path.GetExtension(file.FileName),
                        FILE_SIZE = file.ContentLength,
                        FILESTREAM = Request.Files[i]
                    });
                }
                FileStreamService.InsertFile(lstFileStream);
              


                //Gói thầu tạo mới từ đề nghị thầu
                if (!this.ObjDetail.IS_NEW_ROUND)
                {
                    // Lưu thông tin gói thầu
                    UnitOfWork.BeginTransaction();
                    this.ObjDetail.PKID = this.GetSequence("BID");
                    this.ObjDetail.PARENT_BID = this.ObjDetail.PKID;
                    this.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
                    this.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                    this.ObjDetail.STATUS = EB_Status.KhoiTao;
                    this.ObjDetail.COMMENT_ID = Guid.NewGuid().ToString();
                    this.ObjDetail.GIAI_DOAN = Phase_Status.KhoiTao;
                    this.ObjDetail.TEN_GOI_THAU_KD = UtilsCore.ConvertToUnSign(this.ObjDetail.TEN_GOI_THAU);
                    this.ObjDetail.MO_TA_KD = UtilsCore.ConvertToUnSign(UtilsCore.StripHTML(this.ObjDetail.MO_TA));
                    this.ObjDetail.ROUND = 1;
                    this.ObjDetail.IS_MULTIL_ROUND = false;

                    // Tạo tin tức
                    //var content = "<p>&nbsp;</p><p> &nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><br/>HỒ SƠ THẦU BAO GỒM:</p><p>-Danh mục đề xuất vật tư sử dụng <br/>-Tiến độ thi công, giao hàng <br/>-Báo giá chào thầu</p>";
                    UnitOfWork.Repository<BidNewRepo>().Create(new T_EB_BID_NEW()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        IS_PUBLIC = false,
                        BID_ID = this.ObjDetail.PKID,
                        BAI_VIET = this.ObjDetail.MO_TA,
                        BAI_VIET_EN = this.ObjDetail.MO_TA_EN,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });

                    // Tạo file
                    foreach (var file in lstFileStream)
                    {
                        UnitOfWork.Repository<BidFileRepo>().Create(new T_EB_BID_FILE()
                        {
                            BID_ID = this.ObjDetail.PKID,
                            FILE_ID = file.REFERENCE_ID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }

                    foreach (var FileBid in lstFileBid)
                    {
                        UnitOfWork.Repository<BidFileRepo>().Create(new T_EB_BID_FILE()
                        {
                            BID_ID = this.ObjDetail.PKID,
                            FILE_ID = FileBid.FILE_ID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }



                    // Tạo header
                    this.CurrentRepository.Create(this.ObjDetail);

                    //Tạo tiêu chí mặc định
                    var lstMdSpecDefault = UnitOfWork.Repository<MdBidSpecRepo>().GetAll();
                    var lstMdSpecDetail = UnitOfWork.Repository<MdBidSpecDetailRepo>().GetAll();
                    List<T_EB_BID_SPEC> lstBidSpec = new List<T_EB_BID_SPEC>();
                    List<T_EB_BID_SPEC_DETAIL> lstBidSpecDetail = new List<T_EB_BID_SPEC_DETAIL>();
                    foreach (var item in lstMdSpecDefault.Where(x => x.IS_DEFAULT == true && x.PKID != Global.IdSpecPrice).OrderBy(x => x.C_ORDER).ToList())
                    {
                        if (lstBidSpec.Count(x => x.MD_PKID == item.PKID) == 0)
                        {
                            var bidSpecId = Guid.NewGuid().ToString();
                            lstBidSpec.Add(new T_EB_BID_SPEC()
                            {
                                PKID = bidSpecId,
                                BID_ID = this.ObjDetail.PKID,
                                NAME = item.NAME,
                                NAME_EN = item.NAME_EN,
                                DESCRIPTION = item.DESCRIPTION,
                                DESCRIPTION_EN = item.DESCRIPTION_EN,
                                TYPE = item.TYPE,
                                C_ORDER = item.C_ORDER,
                                IS_SPEC_PRICE = item.IS_SPEC_PRICE,
                                IS_NOT_SHOW_CONTRACTOR = item.IS_NOT_SHOW_CONTRACTOR,
                                ROLE = item.ROLE,
                                MD_PKID = item.PKID,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });

                            if (lstMdSpecDefault.Count(x => x.PARENT_ID == item.PKID) > 0)
                            {
                                foreach (var child in lstMdSpecDefault.Where(x => x.PARENT_ID == item.PKID).OrderBy(x => x.C_ORDER).ToList())
                                {
                                    var bidSpecId2 = Guid.NewGuid().ToString();
                                    lstBidSpec.Add(new T_EB_BID_SPEC()
                                    {
                                        PKID = bidSpecId2,
                                        PARENT_ID = bidSpecId,
                                        BID_ID = this.ObjDetail.PKID,
                                        NAME = child.NAME,
                                        NAME_EN = child.NAME_EN,
                                        DESCRIPTION = child.DESCRIPTION,
                                        DESCRIPTION_EN = child.DESCRIPTION_EN,
                                        TYPE = child.TYPE,
                                        C_ORDER = child.C_ORDER,
                                        IS_SPEC_PRICE = child.IS_SPEC_PRICE,
                                        IS_NOT_SHOW_CONTRACTOR = child.IS_NOT_SHOW_CONTRACTOR,
                                        ROLE = child.ROLE,
                                        MD_PKID = child.PKID,
                                        CREATE_BY = ProfileUtilities.User.USER_NAME
                                    });

                                    foreach (var detail in lstMdSpecDetail.Where(x => x.HEADER_ID == child.PKID).ToList())
                                    {
                                        lstBidSpecDetail.Add(new T_EB_BID_SPEC_DETAIL()
                                        {
                                            PKID = Guid.NewGuid().ToString(),
                                            HEADER_ID = bidSpecId2,
                                            FROM_VALUE = detail.FROM_VALUE,
                                            TO_VALUE = detail.TO_VALUE,
                                            VALUE = detail.VALUE,
                                            VALUE_EN = detail.VALUE_EN,
                                            SCORE = detail.SCORE,
                                            C_ORDER = detail.C_ORDER,
                                            CREATE_BY = ProfileUtilities.User.USER_NAME
                                        });
                                    }
                                }
                            }

                            //Add Detail
                            foreach (var detail in lstMdSpecDetail.Where(x => x.HEADER_ID == item.PKID).ToList())
                            {
                                lstBidSpecDetail.Add(new T_EB_BID_SPEC_DETAIL()
                                {
                                    PKID = Guid.NewGuid().ToString(),
                                    HEADER_ID = bidSpecId,
                                    FROM_VALUE = detail.FROM_VALUE,
                                    TO_VALUE = detail.TO_VALUE,
                                    VALUE = detail.VALUE,
                                    VALUE_EN = detail.VALUE_EN,
                                    SCORE = detail.SCORE,
                                    C_ORDER = detail.C_ORDER
                                });
                            }
                        }
                    }
                    UnitOfWork.Repository<BidSpecRepo>().Create(lstBidSpec);
                    UnitOfWork.Repository<BidSpecDetailRepo>().Create(lstBidSpecDetail);
                    //Tạo tiêu chí giá tổng chào thầu
                    var MdBidSpecPrice = UnitOfWork.Repository<MdBidSpecRepo>().Queryable().FirstOrDefault(x => x.PKID == Global.IdSpecPrice);
                    var specVAT = "a90cb897-0e4a-40c9-8be9-f31005b51c18";
                    var ratioVat = 0.00m;
                    if (MdBidSpecPrice.PKID == specVAT)
                    {
                        ratioVat = 100.00m;
                    }
                    UnitOfWork.Repository<BidSpecRepo>().Create(new T_EB_BID_SPEC()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        BID_ID = this.ObjDetail.PKID,
                        NAME = MdBidSpecPrice.NAME,
                        NAME_EN = MdBidSpecPrice.NAME_EN,
                        DESCRIPTION = MdBidSpecPrice.DESCRIPTION,
                        DESCRIPTION_EN = MdBidSpecPrice.DESCRIPTION_EN,
                        TYPE = MdBidSpecPrice.TYPE,
                        RATIO = ratioVat,
                        C_ORDER = MdBidSpecPrice.C_ORDER,
                        IS_SPEC_PRICE = MdBidSpecPrice.IS_SPEC_PRICE,
                        IS_NOT_SHOW_CONTRACTOR = MdBidSpecPrice.IS_NOT_SHOW_CONTRACTOR,
                        ROLE = MdBidSpecPrice.ROLE,
                        MD_PKID = MdBidSpecPrice.PKID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });

                    var require = UnitOfWork.Repository<BidRequireRepo>().Get(this.ObjDetail.REQUIRED_CODE);
                    require.BID_ID = this.ObjDetail.PKID;
                    UnitOfWork.Repository<BidRequireRepo>().Update(require);

                    // Tạo history
                    UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        ACTION = Action.TaoMoi,
                        ACTION_USER = ProfileUtilities.User.USER_NAME,
                        ACTION_DATE = DateTime.Now,
                        BID_ID = this.ObjDetail.PKID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                    UnitOfWork.Commit();

                    var serviceUserApprove = new UserApproveService();
                    serviceUserApprove.Update(this.ObjDetail.USER_APPROVE, "BID");
                }
                else
                {
                    var lstContractorIdToNotify = new List<string>();

                    var parentBid = this.CurrentRepository.Get(this.ObjDetail.PARENT_BID);
                    var latestRound = this.CurrentRepository.Queryable().Where(x => x.PARENT_BID == this.ObjDetail.PARENT_BID).OrderByDescending(x => x.ROUND).FirstOrDefault();
                    var latestRoundNews = UnitOfWork.Repository<BidNewRepo>().Queryable().FirstOrDefault(x => x.BID_ID == latestRound.PKID);

                    var listCouncil = new List<T_EB_BID_COUNCIL>();
                    var listSpec = new List<T_EB_BID_SPEC>();
                    var listBidFile = new List<T_EB_BID_FILE>();

                    // Lưu thông tin gói thầu
                    UnitOfWork.BeginTransaction();
                    this.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                    this.ObjDetail.STATUS = EB_Status.KhoiTao;
                    this.ObjDetail.COMMENT_ID = Guid.NewGuid().ToString();
                    this.ObjDetail.GIAI_DOAN = Phase_Status.KhoiTao;
                    this.ObjDetail.TEN_GOI_THAU_KD = UtilsCore.ConvertToUnSign(this.ObjDetail.TEN_GOI_THAU);
                    this.ObjDetail.MO_TA_KD = UtilsCore.ConvertToUnSign(UtilsCore.StripHTML(this.ObjDetail.MO_TA));
                    this.ObjDetail.IS_MULTIL_ROUND = true;
                    if (parentBid.IS_MULTIL_ROUND == null || !parentBid.IS_MULTIL_ROUND)
                    {
                        this.ObjDetail.ROUND = 2;

                        //Lấy dữ liệu copy
                        listCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == parentBid.PKID).ToList();
                        listSpec = UnitOfWork.Repository<BidSpecRepo>().Queryable().Where(x => x.BID_ID == parentBid.PKID).ToList();
                        listBidFile = UnitOfWork.Repository<BidFileRepo>().Queryable().Where(x => x.BID_ID == parentBid.PKID).ToList();
                    }
                    else if (parentBid.IS_MULTIL_ROUND && (latestRound != null || latestRound.ROUND > 2))
                    {
                        this.ObjDetail.ROUND = latestRound.ROUND + 1;

                        //Lấy dữ liệu copy
                        listCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == latestRound.PKID).ToList();
                        listSpec = UnitOfWork.Repository<BidSpecRepo>().Queryable().Where(x => x.BID_ID == latestRound.PKID).ToList();
                        listBidFile = UnitOfWork.Repository<BidFileRepo>().Queryable().Where(x => x.BID_ID == latestRound.PKID).ToList();
                    }
                    this.ObjDetail.PKID = this.ObjDetail.PARENT_BID + "-" + this.ObjDetail.ROUND;

                    //Update parent Bid
                    if (!parentBid.IS_MULTIL_ROUND)
                    {
                        parentBid.GIAI_DOAN = Phase_Status.VongTiepTheo;
                        parentBid.IS_MULTIL_ROUND = true;
                        parentBid.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        parentBid.UPDATE_DATE = DateTime.Now;
                        this.CurrentRepository.Update(parentBid);
                    }

                    //Update latestRound
                    if (latestRound.GIAI_DOAN != Phase_Status.DongThau)
                    {
                        latestRound.GIAI_DOAN = Phase_Status.VongTiepTheo;
                        latestRound.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    }

                    // Tạo file
                    foreach (var file in lstFileStream)
                    {
                        UnitOfWork.Repository<BidFileRepo>().Create(new T_EB_BID_FILE()
                        {
                            BID_ID = this.ObjDetail.PKID,
                            FILE_ID = file.REFERENCE_ID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }

                    // Copy file hồ sơ thầu
                    foreach (var item in listBidFile)
                    {
                        var fileID = Guid.NewGuid().ToString();
                        UnitOfWork.Repository<FileUploadRepo>().Create(new T_CM_FILE_UPLOAD()
                        {
                            PKID = fileID,
                            REFERENCE_ID = item.FileUpload.REFERENCE_ID,
                            FILE_EXT = item.FileUpload.FILE_EXT,
                            FILE_SIZE = item.FileUpload.FILE_SIZE,
                            FILE_OLD_NAME = item.FileUpload.FILE_OLD_NAME,
                            CONNECTION_ID = item.FileUpload.CONNECTION_ID,
                            DATABASE_NAME = item.FileUpload.DATABASE_NAME,
                            DIRECTORY_PATH = item.FileUpload.DIRECTORY_PATH,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                        UnitOfWork.Repository<BidFileRepo>().Create(new T_EB_BID_FILE()
                        {
                            BID_ID = this.ObjDetail.PKID,
                            FILE_ID = fileID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }

                    // Tạo Bid Contractor
                    if (!string.IsNullOrEmpty(lstContractorId))
                    {
                        foreach (var item in lstContractorId.Split('|'))
                        {
                            UnitOfWork.Repository<BidContractorRepo>().Create(new T_EB_BID_CONTRACTOR()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                BID_ID = this.ObjDetail.PKID,
                                CONTRACTOR_ID = item,
                                STATUS = "AP",
                                IS_SELECTED = false,
                                IS_WINNING = false,
                                IS_SELF_JOIN = true,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });
                            lstContractorIdToNotify.Add(item);
                        }
                    }


                    // Tạo tin tức
                    //var content = "<p>&nbsp;</p><p> &nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><br/>HỒ SƠ THẦU BAO GỒM:</p><p>-Danh mục đề xuất vật tư sử dụng <br/>-Tiến độ thi công, giao hàng <br/>-Báo giá chào thầu</p>";
                    UnitOfWork.Repository<BidNewRepo>().Create(new T_EB_BID_NEW()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        IS_PUBLIC = false,
                        BID_ID = this.ObjDetail.PKID,
                        BAI_VIET = this.ObjDetail.MO_TA,
                        BAI_VIET_EN = this.ObjDetail.MO_TA_EN,
                        IMAGE_PATH = latestRoundNews.IMAGE_PATH,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });

                    // Tạo Bid Council
                    foreach (var item in listCouncil)
                    {
                        UnitOfWork.Repository<BidCouncilRepo>().Create(new T_EB_BID_COUNCIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            BID_ID = this.ObjDetail.PKID,
                            USER_NAME = item.USER_NAME,
                            VAI_TRO = item.VAI_TRO,
                            IS_QUYET_DINH = item.IS_QUYET_DINH,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }

                    //Tạo Bid Spec
                    var listHeaderID = listSpec.Select(x => x.PKID).ToList();
                    var listDetail = UnitOfWork.Repository<BidSpecDetailRepo>().Queryable().Where(x => listHeaderID.Contains(x.HEADER_ID)).ToList();
                    foreach (var item in listSpec)
                    {
                        var lstChild = listSpec.Where(x => x.PARENT_ID == item.PKID).ToList();
                        var specID = "";
                        var parentID = "";
                        if (this.ObjDetail.ROUND > 2)
                        {
                            var newID = item.PKID.Remove(item.PKID.Length - 2);
                            var newParentID = "";
                            specID = newID + "r" + this.ObjDetail.ROUND;
                            if (!string.IsNullOrEmpty(item.PARENT_ID))
                            {
                                newParentID = item.PARENT_ID.Remove(item.PARENT_ID.Length - 2);
                                parentID = newParentID + "r" + this.ObjDetail.ROUND;
                            }
                        }
                        else
                        {
                            specID = item.PKID + "-r" + this.ObjDetail.ROUND;
                            if (!string.IsNullOrEmpty(item.PARENT_ID))
                            {
                                parentID = item.PARENT_ID + "-r" + this.ObjDetail.ROUND;
                            }
                        }
                        UnitOfWork.Repository<BidSpecRepo>().Create(new T_EB_BID_SPEC()
                        {
                            PKID = specID,
                            PARENT_ID = parentID,
                            BID_ID = this.ObjDetail.PKID,
                            NAME = item.NAME,
                            NAME_EN = item.NAME_EN,
                            DESCRIPTION = item.DESCRIPTION,
                            DESCRIPTION_EN = item.DESCRIPTION_EN,
                            TYPE = item.TYPE,
                            C_ORDER = item.C_ORDER,
                            RATIO = item.RATIO,
                            IS_SPEC_PRICE = item.IS_SPEC_PRICE,
                            IS_NOT_SHOW_CONTRACTOR = item.IS_NOT_SHOW_CONTRACTOR,
                            ROLE = item.ROLE,
                            MD_PKID = item.MD_PKID,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });

                        //Tạo Bid Spec Detail
                        var lstSpecDetail = listDetail.Where(x => x.HEADER_ID == item.PKID).ToList();
                        foreach (var detail in lstSpecDetail)
                        {
                            UnitOfWork.Repository<BidSpecDetailRepo>().Create(new T_EB_BID_SPEC_DETAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                HEADER_ID = specID,
                                FROM_VALUE = detail.FROM_VALUE,
                                TO_VALUE = detail.TO_VALUE,
                                VALUE = detail.VALUE,
                                VALUE_EN = detail.VALUE_EN,
                                SCORE = detail.SCORE,
                                C_ORDER = detail.C_ORDER,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });
                        }
                    }

                    // Tạo header
                    this.CurrentRepository.Create(this.ObjDetail);

                    // Tạo history
                    UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        ACTION = Action.TaoMoi,
                        ACTION_USER = ProfileUtilities.User.USER_NAME,
                        ACTION_DATE = DateTime.Now,
                        BID_ID = this.ObjDetail.PKID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                    UnitOfWork.Commit();

                    var serviceUserApprove = new UserApproveService();
                    serviceUserApprove.Update(this.ObjDetail.USER_APPROVE, "BID");

                    var lstContractor = UnitOfWork.Repository<ContractorRepo>().Queryable().Where(x => lstContractorIdToNotify.Contains(x.PKID)).ToList();
                    this.CreateNotifyNextRound(lstContractorIdToNotify);
                    this.CreateInviteEmailContractor(lstContractor);
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }


        public void CreateNewRound(string bidID, string contractorID, bool isNewRound)
        {
            try
            {
                this.Get(bidID);

                UnitOfWork.BeginTransaction();
                this.ObjDetail.GIAI_DOAN = Phase_Status.VongTiepTheo;

                #region
                // Tạo thông báo
                var userSend = this.ObjDetail.UserSent;
                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();
                var newId = Guid.NewGuid().ToString();
                lstUserNotify.Add(userSend.USER_NAME);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] Yêu cầu đấu thầu vòng tiếp theo !</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                    ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/Create?id={this.ObjDetail.PKID}&bidID={bidID}&contractorID={contractorID}&isNewRound=true&userSend={this.ObjDetail.UserSent.USER_NAME}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "how_to_reg",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} yêu cầu đấu thầu vòng tiếp theo";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent,
                        RAW_CONTENTS_EN = strRawContent,
                        USER_NAME = userSend.USER_NAME
                    };
                    serviceNotify.Create();

                SMOUtilities.SendNotify(lstUserNotify);
                #endregion

            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }

        }
        public void Send(string code)
        {
            try
            {
                this.Get(code);

                if (string.IsNullOrWhiteSpace(this.ObjDetail.USER_APPROVE))
                {
                    this.ErrorMessage = "Bạn chưa chọn người phê duyệt cho gói thầu này!";
                    this.State = false;
                    return;
                }

                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.STATUS = EB_Status.ChoPheDuyet;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.TrinhDuyet,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                this.CreateNotify();
                this.CreateNotiEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        /// <summary>
        /// Gửi email nhắc nhở 
        /// </summary>
        public void CreateNotiEndScoreEmail()
        {
            try
            {
                var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID)
                    .Fetch(x => x.User).ToList();

                var lstUserQuyetDinh = lstCouncil.Where(x => x.IS_QUYET_DINH == true).ToList();
                foreach (var userQuyetDinh in lstUserQuyetDinh)
                {
                    if (userQuyetDinh == null || userQuyetDinh.User == null || string.IsNullOrWhiteSpace(userQuyetDinh.User.EMAIL))
                    {
                        return;
                    }
                }

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
                        .Replace(MA, this.ObjDetail.PKID)
                        .Replace(TEN, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TRANG_THAI, "Chọn nhà thầu")
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + this.ObjDetail.PKID + "&idMenu=mnuChonNhaThau")
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjDetail.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjDetail.USER_APPROVE)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjDetail.UserSent.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, this.ObjDetail.UserApprove.FULL_NAME);

                if (this.ObjDetail.UserSent.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DU_AN, this.ObjDetail.Project.NAME);
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Gói thầu")
                        .Replace(MA, this.ObjDetail.PKID)
                        .Replace(TEN, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TRANG_THAI, EB_Status.GetStatusText(this.ObjDetail.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + this.ObjDetail.PKID + "&idMenu=mnuChonNhaThau")
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjDetail.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjDetail.USER_APPROVE)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjDetail.UserSent.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, this.ObjDetail.UserApprove.FULL_NAME);

                if (this.ObjDetail.UserSent.Organize != null)
                {
                    contentBody = contentBody.Replace(DU_AN, this.ObjDetail.Project.NAME)
                                             .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN);
                }

                foreach (var userQuyetDinh in lstUserQuyetDinh)
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
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Gửi email nhắc nhở 
        /// </summary>
        public void CreateNotiEmail()
        {
            try
            {
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

                if (this.ObjDetail.STATUS == EB_Status.DaPheDuyet || this.ObjDetail.STATUS == EB_Status.TuChoi)
                {
                    subjectTemplate = template.CONG_VIEC_HOAN_THANH_SUBJECT;
                    bodyTemplate = template.CONG_VIEC_HOAN_THANH_BODY;
                }

                if (string.IsNullOrWhiteSpace(subjectTemplate) || string.IsNullOrWhiteSpace(bodyTemplate))
                {
                    return;
                }

                var contentSubject = subjectTemplate
                        .Replace(LOAI_CONG_VIEC, "Gói thầu")
                        .Replace(MA, this.ObjDetail.PKID)
                        .Replace(TEN, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TRANG_THAI, EB_Status.GetStatusText(this.ObjDetail.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + this.ObjDetail.PKID)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjDetail.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjDetail.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjDetail.UserSent.FULL_NAME)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, this.ObjDetail.UserApprove.FULL_NAME);

                if (this.ObjDetail.UserSent.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DU_AN, this.ObjDetail.Project.NAME);
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Gói thầu")
                        .Replace(MA, this.ObjDetail.PKID)
                        .Replace(TEN, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TRANG_THAI, EB_Status.GetStatusText(this.ObjDetail.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + this.ObjDetail.PKID)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjDetail.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjDetail.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjDetail.UserSent.FULL_NAME)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, this.ObjDetail.UserApprove.FULL_NAME);

                if (this.ObjDetail.UserSent.Organize != null)
                {
                    contentBody = contentBody.Replace(DU_AN, this.ObjDetail.Project.NAME);
                }

                if (this.ObjDetail.STATUS == EB_Status.ChoPheDuyet)
                {
                    serviceEmail.ObjDetail = new T_CM_EMAIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        EMAIL = this.ObjDetail.UserApprove.EMAIL,
                        SUBJECT = contentSubject,
                        CONTENTS = contentBody
                    };
                    serviceEmail.Create();
                }
                else
                {
                    serviceEmail.ObjDetail = new T_CM_EMAIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        EMAIL = this.ObjDetail.UserSent.EMAIL,
                        SUBJECT = contentSubject,
                        CONTENTS = contentBody
                    };
                    serviceEmail.Create();

                    if (this.ObjDetail.STATUS == EB_Status.DaPheDuyet)
                    {
                        serviceEmail.ObjDetail = new T_CM_EMAIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            EMAIL = this.ObjDetail.UserManage.EMAIL,
                            SUBJECT = contentSubject,
                            CONTENTS = contentBody
                        };
                        serviceEmail.Create();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Tạo thông báo theo trạng thái gói thầu
        /// </summary>
        public void CreateNotify()
        {
            try
            {
                var lstUserNotify = new List<string>();
                var newId = Guid.NewGuid().ToString();
                var serviceNotify = new NotifyService();

                if (this.ObjDetail.STATUS == EB_Status.ChoPheDuyet)
                {
                    lstUserNotify.Add(this.ObjDetail.USER_APPROVE);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] đang chờ phê duyệt!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strTemplate_EN = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Bid [{4} - {5}] waiting approve!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        EB_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        EB_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU_EN,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} đang chờ phê duyệt!";
                    string strRawContent_EN = $"Bid {this.ObjDetail.PKID} waiting approve!";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = this.ObjDetail.USER_APPROVE
                    };
                    serviceNotify.Create();
                }
                else if (this.ObjDetail.STATUS == EB_Status.DaPheDuyet)
                {
                    lstUserNotify.Add(this.ObjDetail.CREATE_BY);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] đã được phê duyệt!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strTemplate_EN = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Bid [{4} - {5}] approved!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        EB_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        EB_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU_EN,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} đã được phê duyệt!";
                    string strRawContent_EN = $"Bid {this.ObjDetail.PKID} approved!";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = this.ObjDetail.CREATE_BY
                    };
                    serviceNotify.Create();
                }
                else if (this.ObjDetail.STATUS == EB_Status.ChoCapNhat) 
                {
                    lstUserNotify.Add(this.ObjDetail.CREATE_BY);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] chưa được phê duyệt. Yêu cầu cập nhật thêm!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strTemplate_EN = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Bid [{4} - {5}] not approved. Required update more information!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        EB_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        EB_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU_EN,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} chưa được phê duyệt. Yêu cầu cập nhật thêm!";
                    string strRawContent_EN = $"Bid {this.ObjDetail.PKID} not approved. Required update more information!";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = this.ObjDetail.CREATE_BY
                    };
                    serviceNotify.Create();
                }
                else if (this.ObjDetail.STATUS == EB_Status.TuChoi)
                {
                    lstUserNotify.Add(this.ObjDetail.CREATE_BY);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] đã bị từ chối!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strTemplate_EN = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Bid [{4} - {5}] rejected!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        EB_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        EB_Status.GetStatusIcon(this.ObjDetail.STATUS),
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU_EN,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} đã bị từ chối!";
                    string strRawContent_EN = $"Bid {this.ObjDetail.PKID} rejected!";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = this.ObjDetail.CREATE_BY
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
        /// Tạo thông báo mời nhà thầu vào vòng đấu thầu tiếp theo
        /// </summary>
        /// <param name="code"></param>
        public void CreateNotifyNextRound(List<string> lstContractorID)
        {
            try
            {
                var lstContractor = UnitOfWork.Repository<ContractorRepo>().Queryable().Where(x => lstContractorID.Contains(x.PKID)).Select(x => x.USER_NAME).ToList();
                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                foreach (var contractor in lstContractor)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(contractor);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Công ty bạn đã được mời tham gia vòng đấu thầu tiếp theo [{4} - {5}]</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strTemplate_EN = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Your company was invited to participate in the next bidding round [{4} - {5}]!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/DetailBidContractorWait?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "email",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/DetailBidContractorWait?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "email",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU_EN,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Công ty bạn đã được mời tham gia vòng đấu thầu tiếp theo {this.ObjDetail.PKID}";
                    string strRawContent_EN = $"Your company was invited to participate in the next bidding round {this.ObjDetail.PKID}";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = contractor
                    };
                    serviceNotify.Create();
                }

                SMOUtilities.SendNotify(lstUserNotify);
            }
            catch (Exception ex)
            {
                this.State = false;
                this.ErrorMessage = "Lỗi gửi thông báo không thành công";
            }
        }

        public void RequireUpdate(string code, string reason)
        {
            try
            {
                this.Get(code);
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.STATUS = EB_Status.ChoCapNhat;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.YeuCauCapNhat,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    NOTES = reason,
                    BID_ID = this.ObjDetail.PKID,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                this.CreateNotify();
                this.CreateNotiEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void Cancel(string code)
        {
            try
            {
                this.Get(code);
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.STATUS = EB_Status.Huy;
                this.ObjDetail.STATUS = Phase_Status.HuyGoiThau;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.Huy,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void KetThuc(string code)
        {
            try
            {
                this.Get(code);
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.GIAI_DOAN = Phase_Status.DongThau;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.DongThau,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }
        public void Approve(string code, string reason)
        {
            try
            {
                this.Get(code);
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.STATUS = EB_Status.DaPheDuyet;
                this.ObjDetail.GIAI_DOAN = Phase_Status.NopHoSo;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.PheDuyet,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    NOTES = reason,
                    BID_ID = this.ObjDetail.PKID,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                this.CreateNotify();

                var lstContractor = UnitOfWork.Repository<BidContractorRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID)
                    .Fetch(x => x.Contractor).ToList();

                var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID)
                    .Fetch(x => x.User).ToList();

                this.CreateNotifyInviteContractor(lstContractor.Select(x => x.Contractor.USER_NAME).ToList());
                this.CreateNotifyInviteCouncil(lstCouncil.Select(x => x.USER_NAME).ToList());

                this.CreateInviteEmailContractor(lstContractor.Select(x => x.Contractor).ToList());
                this.CreateInviteEmailCouncil(lstCouncil.Select(x => x.User).ToList());
                this.CreateNotiEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        /// <summary>
        /// Tạo thông báo mời nhà thầu khi gói thầu được phê duyệt
        /// </summary>
        public void CreateNotifyInviteContractor(List<string> lstContractor)
        {
            try
            {
                //var lstContractor = UnitOfWork.Repository<BidContractorRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID)
                //    .Fetch(x => x.Contractor).ToList();

                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                foreach (var contractor in lstContractor)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(contractor);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Công ty bạn đã được mời tham gia gói thầu [{4} - {5}]</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strTemplate_EN = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Your company has been invited to bid [{4} - {5}]!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/DetailBidContractorWait?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "email",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/DetailBidContractorWait?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "email",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU_EN,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Công ty bạn đã được mời tham gia gói thầu {this.ObjDetail.PKID}";
                    string strRawContent_EN = $"Your company has been invited to bid {this.ObjDetail.PKID}";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = contractor
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
        /// Tạo thông báo mời hội đồng thầu khi gói thầu được phê duyệt
        /// </summary>
        public void CreateNotifyInviteCouncil(List<string> lstCouncil)
        {
            try
            {
                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                foreach (var council in lstCouncil)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(council);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Bạn đã được mời tham gia vào hội đồng chấm thầu của gói thầu [{4} - {5}]</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strTemplate_EN = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>You have been invited to join the bid's brass committee [{4} - {5}]!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "people",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "people",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Bạn đã được mời tham gia vào hội đồng chấm thầu của gói thầu {this.ObjDetail.PKID}";
                    string strRawContent_EN = $"You have been invited to join the bid's brass committee {this.ObjDetail.PKID}";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = council
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
        /// Tạo thông báo chấm thầu cho hội đồng thầu
        /// </summary>
        public void CreateNotifyChooseContractor()
        {
            try
            {
                var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID).ToList();

                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                string strTemplate = @"
                        <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                            <div class='icon-circle {2}'>
                                <i class='material-icons'>{3}</i>
                            </div>
                            <div class='menu-info'>
                                <span>Gói thầu [{4} - {5}] đã lựa chọn được nhà thầu</span>
                                <p>
                                    <i class='material-icons'>access_time</i> {6}
                                </p>
                            </div>
                        </a>
                    ";

                foreach (var council in lstCouncil)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(council.USER_NAME);

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "person_pin",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} đã lựa chọn được nhà thầu";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent,
                        RAW_CONTENTS_EN = strRawContent,
                        USER_NAME = council.USER_NAME
                    };
                    serviceNotify.Create();
                }

                if (lstCouncil.Count(x => x.USER_NAME == this.ObjDetail.CREATE_BY) == 0)
                {
                    lstUserNotify.Add(this.ObjDetail.CREATE_BY);
                    var newId = Guid.NewGuid().ToString();

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "person_pin",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} đã lựa chọn được nhà thầu";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent,
                        RAW_CONTENTS_EN = strRawContent,
                        USER_NAME = this.ObjDetail.CREATE_BY
                    };
                    serviceNotify.Create();
                }

                if (lstCouncil.Count(x => x.USER_NAME == this.ObjDetail.USER_APPROVE) == 0)
                {
                    lstUserNotify.Add(this.ObjDetail.USER_APPROVE);
                    var newId = Guid.NewGuid().ToString();

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "person_pin",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} đã lựa chọn được nhà thầu";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent,
                        RAW_CONTENTS_EN = strRawContent,
                        USER_NAME = this.ObjDetail.USER_APPROVE
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
        /// Tạo thông báo chấm thầu cho hội đồng thầu
        /// </summary>
        public void CreateNotifyScoreCouncil(List<string> lstCouncil)
        {
            try
            {
                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                foreach (var council in lstCouncil)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(council);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] đã chuyển sang giai đoạn chấm thầu. Thời hạn chấm thầu đến ""{6}""</span>
                            <p>
                                <i class='material-icons'>access_time</i> {7}
                            </p>
                        </div>
                    </a>
                ";

                    string strTemplate_EN = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] đã chuyển sang giai đoạn chấm thầu. Thời hạn chấm thầu đến ""{6}""</span>
                            <p>
                                <i class='material-icons'>access_time</i> {7}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}&idMenu=mnuChamThau",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "format_list_numbered",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        this.ObjDetail.NGAY_KT_CHAM_THAU.HasValue ? this.ObjDetail.NGAY_KT_CHAM_THAU.Value.ToString(Global.DateTimeToStringFormat) : "",
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}&idMenu=mnuChamThau",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "format_list_numbered",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        this.ObjDetail.NGAY_KT_CHAM_THAU.HasValue ? this.ObjDetail.NGAY_KT_CHAM_THAU.Value.ToString(Global.DateTimeToStringFormat) : "",
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} đã chuyển sang giai đoạn chấm thầu";
                    string strRawContent_EN = $"Bid {this.ObjDetail.PKID} has moved to the bidding stage";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = council
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
        /// Tạo thông báo chọn nhà thầu cho người quyết định
        /// </summary>
        public void CreateNotifyEndScoreCouncil()
        {
            try
            {
                var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID).ToList();

                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                foreach (var council in lstCouncil.Where(x => x.IS_QUYET_DINH == true))
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(council.USER_NAME);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] đã chuyển sang giai đoạn chọn nhà thầu</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}&idMenu=mnuChonNhaThau",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "how_to_reg",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} đã chuyển sang giai đoạn chọn nhà thầu";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent,
                        RAW_CONTENTS_EN = strRawContent,
                        USER_NAME = council.USER_NAME
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
        /// Thông báo email cho nhà thầu khi gói thầu được phê duyệt / vào vòng đấu thầu tiếp theo
        /// </summary>
        /// <param name="lstContractor"></param>
        public void CreateInviteEmailContractor(List<T_EB_CONTRACTOR> lstContractor)
        {
            try
            {
                //var lstContractor = UnitOfWork.Repository<BidContractorRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID)
                //    .Fetch(x => x.Contractor).ToList();
                string MA_GOI_THAU = "{MA_GOI_THAU}";
                string TEN_GOI_THAU = "{TEN_GOI_THAU}";
                string TEN_GOI_THAU_EN = "{TEN_GOI_THAU_EN}";
                string NGAY_MO_THAU = "{NGAY_MO_THAU}";
                string NGAY_DONG_THAU = "{NGAY_DONG_THAU}";
                string LINK_CHI_TIET = "{LINK_CHI_TIET}";
                string TEN_NHA_THAU_DAI = "{TEN_NHA_THAU_DAI}";
                string TEN_NHA_THAU_NGAN = "{TEN_NHA_THAU_NGAN}";
                string DU_AN = "{DU_AN}";
                string DU_AN_EN = "{DU_AN_EN}";

                var serviceTemplate = new ConfigTemplateNotifyService();
                serviceTemplate.GetAll();
                if (serviceTemplate.ObjList.Count() == 0)
                {
                    return;
                }


                var serviceConfig = new SystemConfigService();
                serviceConfig.GetConfig();

                var template = serviceTemplate.ObjList.FirstOrDefault();

                if ((!this.ObjDetail.IS_MULTIL_ROUND && (string.IsNullOrEmpty(template.INVITE_SUBJECT) || string.IsNullOrEmpty(template.INVITE_BODY))) || (this.ObjDetail.IS_MULTIL_ROUND && (string.IsNullOrEmpty(template.NEXT_ROUND_SUBJECT) || string.IsNullOrEmpty(template.NEXT_ROUND_BODY))))
                {
                    return;
                }

                var url = ConfigurationManager.AppSettings["URL_WEB"];

                foreach (var contractor in lstContractor)
                {
                    var bidContractor = UnitOfWork.Repository<BidContractorRepo>().Queryable().FirstOrDefault(x => x.BID_ID == this.ObjDetail.PKID && x.CONTRACTOR_ID == contractor.PKID);

                    if (!string.IsNullOrEmpty(contractor.EMAIL))
                    {

                        var contentSubject = template.INVITE_SUBJECT;
                        if (this.ObjDetail.IS_MULTIL_ROUND)
                        {
                            contentSubject = template.NEXT_ROUND_SUBJECT;
                        }
                        contentSubject = contentSubject
                        .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                        .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                        .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(LINK_CHI_TIET, url + "Index/ChiTiet/" + this.ObjDetail.PKID)
                        .Replace(TEN_NHA_THAU_NGAN, contractor.TEN_NGAN)
                        .Replace(TEN_NHA_THAU_DAI, contractor.TEN_DAI);

                        var contentBody = template.INVITE_BODY;
                        if (this.ObjDetail.IS_MULTIL_ROUND)
                        {
                            contentBody = template.NEXT_ROUND_BODY;
                        }
                        contentBody = contentBody
                            .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                            .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                            .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                            .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                            .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                            .Replace(LINK_CHI_TIET, url + "Index/ChiTiet/" + this.ObjDetail.PKID)
                            .Replace(DU_AN, this.ObjDetail.Project.NAME)
                            .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                            .Replace(TEN_NHA_THAU_NGAN, contractor.TEN_NGAN)
                            .Replace(TEN_NHA_THAU_DAI, contractor.TEN_DAI);

                        try
                        {
                            UnitOfWork.BeginTransaction();
                            var lstCoctractorEmail = contractor.EMAIL.Split(';').ToList();
                            foreach (var item in lstCoctractorEmail)
                            {
                                var email = new T_CM_EMAIL()
                                {
                                    PKID = Guid.NewGuid().ToString(),
                                    EMAIL = item,
                                    SUBJECT = contentSubject,
                                    CONTENTS = contentBody
                                };
                                UnitOfWork.Repository<EmailRepo>().Create(email);
                                if (bidContractor != null)
                                {
                                    bidContractor.EMAIL_SENT_ID = email.PKID;
                                    UnitOfWork.Repository<BidContractorRepo>().Update(bidContractor);
                                }
                            }
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

        /// <summary>
        /// Thông báo email chấm thầu cho hội đồng thầu
        /// </summary>
        /// <param name="lstCouncil"></param>
        public void CreateScoreEmailCouncil(List<T_AD_USER> lstCouncil)
        {
            try
            {
                string MA_GOI_THAU = "{MA_GOI_THAU}";
                string TEN_GOI_THAU = "{TEN_GOI_THAU}";
                string TEN_GOI_THAU_EN = "{TEN_GOI_THAU_EN}";
                string NGAY_MO_THAU = "{NGAY_MO_THAU}";
                string NGAY_DONG_THAU = "{NGAY_DONG_THAU}";
                string NGAY_KET_THUC_CHAM_THAU = "{NGAY_KET_THUC_CHAM_THAU}";
                string LINK_CHI_TIET = "{LINK_CHI_TIET}";
                string USER_NAME = "{USER_NAME}";
                string HO_TEN = "{HO_TEN}";
                string DU_AN = "{DU_AN}";
                string DU_AN_EN = "{DU_AN_EN}";


                var serviceTemplate = new ConfigTemplateNotifyService();
                serviceTemplate.GetAll();
                if (serviceTemplate.ObjList.Count() == 0)
                {
                    return;
                }

                var serviceConfig = new SystemConfigService();
                serviceConfig.GetConfig();

                var template = serviceTemplate.ObjList.FirstOrDefault();

                if (string.IsNullOrEmpty(template.SCORE_SUBJECT) || string.IsNullOrEmpty(template.SCORE_BODY))
                {
                    return;
                }

                var serviceEmail = new EmailNotifyService();
                var url = ConfigurationManager.AppSettings["URL_WEB"];

                foreach (var council in lstCouncil)
                {
                    if (!string.IsNullOrEmpty(council.EMAIL))
                    {
                        var contentSubject = template.SCORE_SUBJECT
                        .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                        .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                        .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_KET_THUC_CHAM_THAU, this.ObjDetail.NGAY_KT_CHAM_THAU.HasValue ? this.ObjDetail.NGAY_KT_CHAM_THAU.Value.ToString(Global.DateTimeToStringFormat) : "")
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + this.ObjDetail.PKID)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(USER_NAME, council.USER_NAME)
                        .Replace(HO_TEN, council.FULL_NAME);

                        var contentBody = template.SCORE_BODY
                        .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                        .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                        .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_KET_THUC_CHAM_THAU, this.ObjDetail.NGAY_KT_CHAM_THAU.HasValue ? this.ObjDetail.NGAY_KT_CHAM_THAU.Value.ToString(Global.DateTimeToStringFormat) : "")
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + this.ObjDetail.PKID)
                        .Replace(USER_NAME, council.USER_NAME)
                        .Replace(HO_TEN, council.FULL_NAME);

                        serviceEmail.ObjDetail = new T_CM_EMAIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            EMAIL = council.EMAIL,
                            SUBJECT = contentSubject,
                            CONTENTS = contentBody
                        };
                        serviceEmail.Create();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Thông báo email cho hội đồng thầu gói khi thầu được phê duyệt
        /// </summary>
        /// <param name="lstCouncil"></param>
        public void CreateInviteEmailCouncil(List<T_AD_USER> lstCouncil)
        {
            try
            {
                string MA_GOI_THAU = "{MA_GOI_THAU}";
                string TEN_GOI_THAU = "{TEN_GOI_THAU}";
                string TEN_GOI_THAU_EN = "{TEN_GOI_THAU_EN}";
                string NGAY_MO_THAU = "{NGAY_MO_THAU}";
                string NGAY_DONG_THAU = "{NGAY_DONG_THAU}";
                string LINK_CHI_TIET = "{LINK_CHI_TIET}";
                string USER_NAME = "{USER_NAME}";
                string HO_TEN = "{HO_TEN}";
                string DU_AN = "{DU_AN}";
                string DU_AN_EN = "{DU_AN_EN}";

                var serviceTemplate = new ConfigTemplateNotifyService();
                serviceTemplate.GetAll();
                if (serviceTemplate.ObjList.Count() == 0)
                {
                    return;
                }

                var serviceConfig = new SystemConfigService();
                serviceConfig.GetConfig();

                var template = serviceTemplate.ObjList.FirstOrDefault();

                if (string.IsNullOrEmpty(template.INVITE_COUNCIL_SUBJECT) || string.IsNullOrEmpty(template.INVITE_COUNCIL_BODY))
                {
                    return;
                }

                var serviceEmail = new EmailNotifyService();
                var url = ConfigurationManager.AppSettings["URL_WEB"];

                foreach (var council in lstCouncil)
                {
                    if (!string.IsNullOrEmpty(council.EMAIL))
                    {
                        var contentSubject = template.INVITE_COUNCIL_SUBJECT
                        .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                        .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                        .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + this.ObjDetail.PKID)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(USER_NAME, council.USER_NAME)
                        .Replace(HO_TEN, council.FULL_NAME);

                        var contentBody = template.INVITE_COUNCIL_BODY
                        .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                        .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                        .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + this.ObjDetail.PKID)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(USER_NAME, council.USER_NAME)
                        .Replace(HO_TEN, council.FULL_NAME);

                        serviceEmail.ObjDetail = new T_CM_EMAIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            EMAIL = council.EMAIL,
                            SUBJECT = contentSubject,
                            CONTENTS = contentBody
                        };
                        serviceEmail.Create();
                    }
                }
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// Thông báo email kết quả gói thầu
        /// </summary>
        public void CreateResultEmail()
        {
            try
            {
                var lstContractor = UnitOfWork.Repository<BidContractorRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID || x.BID_ID == this.ObjDetail.PARENT_BID)
                    .Fetch(x => x.Contractor).ToList();


                string MA_GOI_THAU = "{MA_GOI_THAU}";
                string TEN_GOI_THAU = "{TEN_GOI_THAU}";
                string TEN_GOI_THAU_EN = "{TEN_GOI_THAU_EN}";
                string NGAY_MO_THAU = "{NGAY_MO_THAU}";
                string NGAY_DONG_THAU = "{NGAY_DONG_THAU}";
                string LINK_CHI_TIET = "{LINK_CHI_TIET}";
                string TEN_NHA_THAU_DAI = "{TEN_NHA_THAU_DAI}";
                string TEN_NHA_THAU_NGAN = "{TEN_NHA_THAU_NGAN}";
                string DU_AN = "{DU_AN}";
                string DU_AN_EN = "{DU_AN_EN}";

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

                foreach (var contractor in lstContractor.Where(x => x.IS_WINNING == true))
                {
                    if (!string.IsNullOrEmpty(contractor.Contractor.EMAIL) && !string.IsNullOrEmpty(template.WIN_SUBJECT) && !string.IsNullOrEmpty(template.WIN_BODY))
                    {
                        var contentSubject = template.WIN_SUBJECT
                        .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                        .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                        .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(LINK_CHI_TIET, url + "Index/ChiTiet/" + this.ObjDetail.PKID)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(TEN_NHA_THAU_NGAN, contractor.Contractor.TEN_NGAN)
                        .Replace(TEN_NHA_THAU_DAI, contractor.Contractor.TEN_DAI);

                        var contentBody = template.WIN_BODY
                            .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                        .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                        .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(LINK_CHI_TIET, url + "Index/ChiTiet/" + this.ObjDetail.PKID)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(TEN_NHA_THAU_NGAN, contractor.Contractor.TEN_NGAN)
                        .Replace(TEN_NHA_THAU_DAI, contractor.Contractor.TEN_DAI);

                        var lstContractorEmail = contractor.Contractor.EMAIL.Split(';').ToList();
                        foreach (var item in lstContractorEmail)
                        {
                            serviceEmail.ObjDetail = new T_CM_EMAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                EMAIL = item,
                                SUBJECT = contentSubject,
                                CONTENTS = contentBody
                            };
                            serviceEmail.Create();
                        }
                    }
                }

                foreach (var contractor in lstContractor.Where(x => x.IS_WINNING == false && x.STATUS == "AP"))
                {
                    if (!string.IsNullOrEmpty(contractor.Contractor.EMAIL) && !string.IsNullOrEmpty(template.FAIL_SUBJECT) && !string.IsNullOrEmpty(template.FAIL_BODY))
                    {
                        var contentSubject = template.FAIL_SUBJECT
                        .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                        .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                        .Replace(TEN_NHA_THAU_NGAN, contractor.Contractor.TEN_NGAN)
                        .Replace(TEN_NHA_THAU_DAI, contractor.Contractor.TEN_DAI)
                        .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(LINK_CHI_TIET, url + "Index/ChiTiet/" + this.ObjDetail.PKID)
                        .Replace(TEN_NHA_THAU_NGAN, contractor.Contractor.TEN_NGAN)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)

                        .Replace(TEN_NHA_THAU_DAI, contractor.Contractor.TEN_DAI);

                        var contentBody = template.FAIL_BODY
                        .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                        .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                        .Replace(TEN_NHA_THAU_NGAN, contractor.Contractor.TEN_NGAN)
                        .Replace(TEN_NHA_THAU_DAI, contractor.Contractor.TEN_DAI)
                        .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(LINK_CHI_TIET, url + "Index/ChiTiet/" + this.ObjDetail.PKID)
                        .Replace(TEN_NHA_THAU_NGAN, contractor.Contractor.TEN_NGAN)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(TEN_NHA_THAU_DAI, contractor.Contractor.TEN_DAI);

                        var lstContractorEmail = contractor.Contractor.EMAIL.Split(';').ToList();
                        foreach (var item in lstContractorEmail)
                        {
                            serviceEmail.ObjDetail = new T_CM_EMAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                EMAIL = item,
                                SUBJECT = contentSubject,
                                CONTENTS = contentBody
                            };
                            serviceEmail.Create();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

        }

        public void Reject(string code, string reason)
        {
            try
            {
                if (string.IsNullOrEmpty(reason))
                {
                    this.State = false;
                    this.ErrorMessage = "Chưa nhập lý do từ chối phê duyệt gói thầu!";
                    return;
                }
                this.Get(code);
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.STATUS = EB_Status.TuChoi;
                this.ObjDetail.GIAI_DOAN = Phase_Status.DongThau;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);

                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.TuChoi,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    NOTES = reason,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                this.CreateNotify();
                this.CreateNotiEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void ScoreBid(string code)
        {
            try
            {
                this.Get(code);

                if (!this.ObjDetail.NGAY_KT_CHAM_THAU.HasValue)
                {
                    this.ErrorMessage = "Gói thầu chưa có thông tin [NGÀY KẾT THÚC CHẤM THẦU]!";
                    this.State = false;
                    return;
                }

                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.GIAI_DOAN = Phase_Status.ChamThau;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                UnitOfWork.Commit();

                var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID)
                  .Fetch(x => x.User).ToList();

                this.CreateNotifyScoreCouncil(lstCouncil.Select(x => x.USER_NAME).ToList());
                this.CreateScoreEmailCouncil(lstCouncil.Select(x => x.User).ToList());
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CancelBid(string code, string reason)
        {
            try
            {
                if (string.IsNullOrEmpty(reason))
                {
                    this.State = false;
                    this.ErrorMessage = "Chưa nhập lý do hủy gói thầu!";
                    return;
                }
                this.Get(code);
                this.ObjDetail.REASON = reason;
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.GIAI_DOAN = Phase_Status.HuyGoiThau;

                UnitOfWork.BeginTransaction();
                if (this.ObjDetail.IS_MULTIL_ROUND)
                {
                    var lstBid = this.CurrentRepository.Queryable().Where(x => x.PARENT_BID == this.ObjDetail.PARENT_BID).ToList();
                    foreach (var item in lstBid)
                    {
                        item.GIAI_DOAN = Phase_Status.HuyGoiThau;
                        item.REASON = reason;
                        item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        this.CurrentRepository.Update(item);
                    }
                }
                else
                {
                    this.CurrentRepository.Update(this.ObjDetail);
                }

                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.Huy,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    NOTES = reason,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();

                this.CreateResultEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CloseBid(HttpRequestBase Request)
        {
            try
            {
                this.Get(this.ObjDetail.PKID);
                UnitOfWork.BeginTransaction();
                //Chọn nhà thầu trúng thầu
                var lstBidContractor = UnitOfWork.Repository<BidContractorRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID).ToList();
                if (lstBidContractor.Count > 0)
                {
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

                    var lstBidContractorWin = lstBidContractor.Where(x => ContractorId.Contains(x.CONTRACTOR_ID)).ToList();
                    var lstBidContractorFail = lstBidContractor.Where(x => !ContractorId.Contains(x.CONTRACTOR_ID)).ToList();

                    if (lstBidContractorWin.Count > 0)
                    {
                        foreach (var item in lstBidContractorWin)
                        {
                            item.IS_WINNING = true;
                            item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                            UnitOfWork.Repository<BidContractorRepo>().Update(item);

                            var price = this.ObjListPrice.FirstOrDefault(x => x.CONTRACTOR_ID == item.CONTRACTOR_ID);
                            if (price.PRICE == 0)
                            {
                                this.State = false;
                                this.ErrorMessage = "Chưa nhập giá trúng thầu!";
                                return;
                            }
                            UnitOfWork.Repository<BidContractorPriceRepo>().Create(new T_EB_BID_CONTRACTOR_PRICE()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                BID_ID = this.ObjDetail.PKID,
                                CONTRACTOR_ID = price.CONTRACTOR_ID,
                                PRICE = price.PRICE,
                                TYPE = "3",
                                NOTE = price.NOTE,
                                C_ORDER = 0,
                                CREATE_BY = ProfileUtilities.User.USER_NAME
                            });

                            if (dictionary.Where(x => x.Value == item.CONTRACTOR_ID).Count() > 0)
                            {
                                foreach (var detail in dictionary.Where(x => x.Value == item.CONTRACTOR_ID))
                                {
                                    UnitOfWork.Repository<BidContractorPriceFileRepo>().Create(new T_EB_BID_CONTRACTOR_PRICE_FILE()
                                    {
                                        PKID = Guid.NewGuid().ToString(),
                                        BID_ID = this.ObjDetail.PKID,
                                        CONTRACTOR_ID = item.CONTRACTOR_ID,
                                        FILE_ID = detail.Key.REFERENCE_ID,
                                        TYPE = "3",
                                        CREATE_BY = ProfileUtilities.User.USER_NAME
                                    });
                                }
                            }
                        }
                    }

                    if (lstBidContractorFail.Count > 0)
                    {
                        foreach (var item in lstBidContractorFail)
                        {
                            item.IS_WINNING = false;
                            item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                            UnitOfWork.Repository<BidContractorRepo>().Update(item);
                        }
                    }



                }
                else
                {
                    this.State = false;
                    this.ErrorMessage = "Không tìm thấy thông tin gói thầu!";
                    return;
                }

                //Update gói thầu
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.GIAI_DOAN = Phase_Status.DongThau;
                this.ObjDetail.CLOSE_DATE = DateTime.Now;

                if (this.ObjDetail.IS_MULTIL_ROUND)
                {
                    var lstBid = this.CurrentRepository.Queryable().Where(x => x.PARENT_BID == this.ObjDetail.PARENT_BID).ToList();
                    foreach (var item in lstBid)
                    {
                        item.GIAI_DOAN = Phase_Status.DongThau;
                        item.CLOSE_DATE = DateTime.Now;
                        item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                        this.CurrentRepository.Update(item);
                    }
                }
                else
                {
                    this.CurrentRepository.Update(this.ObjDetail);
                }

                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.DongThau,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();

                this.CreateResultEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void EndScoreBid(string code)
        {
            try
            {
                this.Get(code);
                if (this.ObjDetail.ListContractor.Count(x => x.IS_SELECTED == true) == 0) 
                {
                    this.State = false;
                    this.ErrorMessage = "Chưa chọn nhà thầu vào danh sách sơ bộ!";
                    return;
                }
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.GIAI_DOAN = Phase_Status.ChonNhaThau;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.KetThucChamThau,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                this.CreateNotifyEndScoreCouncil();
                this.CreateNotiEndScoreEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void DebutContractor()
        {
            try
            { 
                var lstBidContractor = this.ObjDetail.ListContractor.ToList();
                var lstContractorId = this.ObjDetail.ListContractor.Select(x => x.CONTRACTOR_ID).ToList();
                var lstContractor = UnitOfWork.Repository<ContractorRepo>().Queryable().Where(x => lstContractorId.Contains(x.PKID)).ToList();
                UnitOfWork.BeginTransaction();
                foreach (var item in this.ObjDetail.ListContractor)
                {
                    if (item.PERCENT_WINNING.HasValue && item.PERCENT_WINNING > 0)
                    {
                        item.PERCENT_WINNING = item.PERCENT_DEBUT;
                    }
                    item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    UnitOfWork.Repository<BidContractorRepo>().Update(item);
                }

                var note = "user: " + ProfileUtilities.User.USER_NAME + " - " + ProfileUtilities.User.FULL_NAME + " chọn danh sách sơ bộ: <br/>";
                foreach (var item in this.ObjDetail.ListContractor.Where(x => x.IS_SELECTED == true).ToList())
                {
                    var contractor = lstContractor.FirstOrDefault(x => x.PKID == item.CONTRACTOR_ID);
                    note += $"&nbsp&nbsp- Nhà thầu: [{contractor.TEN_NGAN}] <br/>";
                }
                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.ChonDSSoBo,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    NOTES = note,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void ChoiceContractorWin(string code, List<T_EB_BID_CONTRACTOR> lstContractorWin)
        {
            try
            {
                this.Get(code);
                var lstBidContractor = UnitOfWork.Repository<BidContractorRepo>().Queryable().Where(x => x.BID_ID == code).ToList();
                foreach (var item in lstBidContractor)
                {
                    item.IS_WINNING = false;
                    item.PERCENT_WINNING = 0;
                }
                var note = "user: " + ProfileUtilities.User.USER_NAME + " - " + ProfileUtilities.User.FULL_NAME + " chọn nhà thầu trúng thầu: <br/>";
                UnitOfWork.BeginTransaction();
                foreach (var item in lstContractorWin)
                {
                    var contractor = lstBidContractor.FirstOrDefault(x => x.CONTRACTOR_ID == item.CONTRACTOR_ID);
                    contractor.PERCENT_WINNING = item.PERCENT_WINNING;
                    contractor.PERCENT_DEBUT = item.PERCENT_WINNING;
                    contractor.IS_WINNING = true;
                    contractor.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    UnitOfWork.Repository<BidContractorRepo>().Update(contractor);

                    note += $"&nbsp&nbsp- Nhà thầu: [{contractor.Contractor.TEN_NGAN}]<br/>";
                }

                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.ChonNhaThau,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    NOTES = note,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CancelContractorWin(string code, string contractorId)
        {
            var bidContractor = UnitOfWork.Repository<BidContractorRepo>().Queryable().FirstOrDefault(x => x.BID_ID == code && x.CONTRACTOR_ID == contractorId);
            if (bidContractor != null)
            {
                bidContractor.IS_WINNING = false;
                bidContractor.UPDATE_BY = ProfileUtilities.User.USER_NAME;

                UnitOfWork.BeginTransaction();
                UnitOfWork.Repository<BidContractorRepo>().Update(bidContractor);
                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.HuyChonNhaThau,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
            }
            else
            {
                this.State = false;
                this.ErrorMessage = "Nhà thầu không có trong danh sách được mời của gói thầu!";
            }
        }


        /// <summary>
        ///  giai đoạn thương thảo
        /// </summary>
        /// <param name="code"></param>
        public void ThuongThaoLai(string id)
        {
            try
            {
                this.Get(id);
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.GIAI_DOAN = Phase_Status.ThuongThaoLai;
                this.ObjDetail.STATUS = EB_Status.ThuongThaoLai;
                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.ThuongThaoLai,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                this.CreateNotifyThuongThaoLai();
                this.CreateEmailThuongThaoLai();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        /// <summary>
        /// Kết thúc giai đoạn thương thảo
        /// </summary>
        /// <param name="code"></param>
        public void KetThucThuongThao(string code)
        {
            try
            {
                this.Get(code);

                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjDetail.GIAI_DOAN = Phase_Status.ChonNhaThau;
                this.ObjDetail.STATUS = EB_Status.DaPheDuyet;


                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjDetail);
                // Tạo history
                UnitOfWork.Repository<BidHistoryRepo>().Create(new T_EB_BID_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.KetThucThuongThao,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    BID_ID = this.ObjDetail.PKID,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                UnitOfWork.Commit();
                CreateNotifyKTThuongThao();
                CreateNotiEmail();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        /// <summary>
        /// Tạo thông báo kết thúc thương thảo
        /// </summary>
        public void CreateNotifyKTThuongThao()
        {
            try
            {
                var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID && x.IS_QUYET_DINH == true).ToList();
                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                foreach (var council in lstCouncil)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(council.USER_NAME);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] đã thương thảo lại xong !</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}&idMenu=mnuThongTin",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "how_to_reg",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} đã thương thảo lại xong đề nghị chọn lại nhà thầu";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent,
                        RAW_CONTENTS_EN = strRawContent,
                        USER_NAME = council.USER_NAME
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
        /// Thông báo email kết thúc thương thảo cho người quyết định
        /// </summary>
        /// <param name="lstContractor"></param>
        public void CreateEmailKTThuongThao()
        {
            var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID && x.IS_QUYET_DINH == true).ToList();
            try
            {
                string MA_GOI_THAU = "{MA_GOI_THAU}";
                string TEN_GOI_THAU = "{TEN_GOI_THAU}";
                string TEN_GOI_THAU_EN = "{TEN_GOI_THAU_EN}";
                string NGAY_MO_THAU = "{NGAY_MO_THAU}";
                string NGAY_DONG_THAU = "{NGAY_DONG_THAU}";
                string LINK_CHI_TIET = "{LINK_CHI_TIET}";
                string DU_AN = "{DU_AN}";

                var serviceTemplate = new ConfigTemplateNotifyService();
                serviceTemplate.GetAll();
                if (serviceTemplate.ObjList.Count() == 0)
                {
                    return;
                }


                var serviceConfig = new SystemConfigService();
                serviceConfig.GetConfig();

                var template = serviceTemplate.ObjList.FirstOrDefault();

                if (string.IsNullOrEmpty(template.KET_THUC_THUONG_THAO_SUBJECT) || string.IsNullOrEmpty(template.KET_THUC_THUONG_THAO_BODY))
                {
                    return;
                }

                var url = ConfigurationManager.AppSettings["URL_WEB"];

                foreach (var item in lstCouncil)
                {
                    if (!string.IsNullOrEmpty(item.User.EMAIL))
                    {

                        var contentSubject = template.KET_THUC_THUONG_THAO_SUBJECT;
                        contentSubject = contentSubject
                        .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                        .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                        .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                        .Replace(LINK_CHI_TIET, url + "Index/ChiTiet/" + this.ObjDetail.PKID);

                        var contentBody = template.KET_THUC_THUONG_THAO_BODY;
                        contentBody = contentBody
                            .Replace(MA_GOI_THAU, this.ObjDetail.PKID)
                            .Replace(TEN_GOI_THAU, this.ObjDetail.TEN_GOI_THAU)
                            .Replace(TEN_GOI_THAU_EN, this.ObjDetail.TEN_GOI_THAU_EN)
                            .Replace(DU_AN, this.ObjDetail.Project.NAME)
                            .Replace(NGAY_MO_THAU, this.ObjDetail.NGAY_MO_THAU.ToString(Global.DateTimeToStringFormat))
                            .Replace(NGAY_DONG_THAU, this.ObjDetail.NGAY_DONG_THAU.ToString(Global.DateTimeToStringFormat))
                            .Replace(LINK_CHI_TIET, url + "Index/ChiTiet/" + this.ObjDetail.PKID);

                        try
                        {
                            UnitOfWork.BeginTransaction();

                            var email = new T_CM_EMAIL()
                            {
                                PKID = Guid.NewGuid().ToString(),
                                EMAIL = item.User.EMAIL,
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

        /// <summary>
        /// Tạo thông báo thương thảo lại
        /// </summary>
        public void CreateNotifyThuongThaoLai()
        {
            try
            {
                var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID && x.VAI_TRO.Contains("TDG")).ToList();
                var lstUserNotify = new List<string>();
                var serviceNotify = new NotifyService();

                foreach (var council in lstCouncil)
                {
                    var newId = Guid.NewGuid().ToString();
                    lstUserNotify.Add(council.USER_NAME);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                       <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Gói thầu [{4} - {5}] Yêu cầu thương thảo lại!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strTemplate_EN = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                       <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Bid [{4} - {5}] Request to renegotiate!!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}&idMenu=mnuBidContractorPrice",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "how_to_reg",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/Bid/MenuDetailBidManager?id={this.ObjDetail.PKID}&idMenu=mnuBidContractorPrice",
                        EB_Status.GetStatusColor(this.ObjDetail.STATUS),
                        "how_to_reg",
                        this.ObjDetail.PKID,
                        this.ObjDetail.TEN_GOI_THAU,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"Gói thầu {this.ObjDetail.PKID} đã yêu cầu thương thảo lại";
                    string strRawContent_EN = $"Gói thầu {this.ObjDetail.PKID} requested to renegotiate";

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
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// Thông báo email thương thảo lại cho người tạo và người quản lý hợp đồng
        /// </summary>
        /// <param name="lstContractor"></param>
        public void CreateEmailThuongThaoLai()
        {
            var lstCouncil = UnitOfWork.Repository<BidCouncilRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID && x.VAI_TRO.Contains("TDG")).ToList();
            try
            {

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
                        .Replace(MA, this.ObjDetail.PKID)
                        .Replace(TEN, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TRANG_THAI, "Thương thảo lại")
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + this.ObjDetail.PKID + "&idMenu=mnuBidContractorPrice")
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjDetail.CREATE_BY)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjDetail.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjDetail.UserSent.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, this.ObjDetail.UserApprove.FULL_NAME);

                if (this.ObjDetail.UserSent.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DU_AN, this.ObjDetail.Project.NAME);
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Gói thầu")
                        .Replace(MA, this.ObjDetail.PKID)
                        .Replace(TEN, this.ObjDetail.TEN_GOI_THAU)
                        .Replace(TRANG_THAI, EB_Status.GetStatusText(this.ObjDetail.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBid?bidId=" + this.ObjDetail.PKID + "&idMenu=mnuBidContractorPrice")
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjDetail.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjDetail.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjDetail.UserSent.FULL_NAME)
                        .Replace(DU_AN, this.ObjDetail.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjDetail.Project.NAME_EN)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, this.ObjDetail.UserApprove.FULL_NAME);

                if (this.ObjDetail.UserSent.Organize != null)
                {
                    contentBody = contentBody.Replace(DU_AN, this.ObjDetail.Project.NAME);
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
            }
            catch (Exception)
            {
            }
        }

        public void ExportExcel(ref MemoryStream outFileStream)
        {
            this.ObjDetail.IS_NOT_PAGEING = true;
            this.Search();
            try
            {
                //Ghi file Excel
                XSSFWorkbook wb = new XSSFWorkbook();
                ISheet sheet = wb.CreateSheet();

                //Tạo row
                var row0 = sheet.CreateRow(0);
                //Merge lại row đầu các cột
                row0.CreateCell(0); //Tạo ra cell trc khi merge
                CellRangeAddress cellMerge = new CellRangeAddress(0, 0, 0, 12);
                sheet.AddMergedRegion(cellMerge);
                row0.GetCell(0).SetCellValue("DANH SÁCH GÓI THẦU");

                //Ghi tên cột ở row1
                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("STT");
                row1.CreateCell(1).SetCellValue("MÃ GÓI THẦU");
                row1.CreateCell(2).SetCellValue("TÊN GÓI THẦU");
                row1.CreateCell(3).SetCellValue("TRẠNG THÁI");
                row1.CreateCell(4).SetCellValue("GIAI ĐOẠN");
                row1.CreateCell(5).SetCellValue("NGÀY MỜI THẦU");
                row1.CreateCell(6).SetCellValue("NGÀY ĐÓNG THẦU");
                row1.CreateCell(7).SetCellValue("NGÀY KẾT THÚC");
                row1.CreateCell(8).SetCellValue("NGÀY TẠO");
                row1.CreateCell(9).SetCellValue("NGƯỜI TẠO");
                row1.CreateCell(10).SetCellValue("NGƯỜI TRÌNH DUYỆT");
                row1.CreateCell(11).SetCellValue("NGƯỜI PHÊ DUYỆT");
                row1.CreateCell(12).SetCellValue("MÃ ĐỀ NGHỊ MỞ THẦU");

                //bắt đầu duyệt mảng và ghi tiếp tục
                int rowIndex = 2;
                int stt = 1;
                foreach (var item in this.ObjList)
                {
                    //tạo row mới
                    var newRow = sheet.CreateRow(rowIndex);

                    //set giá trị
                    newRow.CreateCell(0).SetCellValue(stt);
                    newRow.CreateCell(1).SetCellValue(item.PKID);
                    newRow.CreateCell(2).SetCellValue(item.TEN_GOI_THAU);
                    newRow.CreateCell(3).SetCellValue(EB_Status.GetStatusText(item.STATUS));
                    newRow.CreateCell(4).SetCellValue(Phase_Status.GetStatusText(item.GIAI_DOAN));
                    newRow.CreateCell(5).SetCellValue(item.NGAY_MO_THAU.ToString(Global.DateToStringFormat));
                    newRow.CreateCell(6).SetCellValue(item.NGAY_DONG_THAU.ToString(Global.DateToStringFormat));
                    newRow.CreateCell(7).SetCellValue(item.CLOSE_DATE.HasValue ? item.CLOSE_DATE.Value.ToString(Global.DateToStringFormat) : "");
                    newRow.CreateCell(8).SetCellValue(item.CREATE_DATE.Value.ToString("dd/MM/yyyy HH:mm"));
                    newRow.CreateCell(9).SetCellValue(item.CREATE_BY);
                    newRow.CreateCell(10).SetCellValue((item.UserSent != null) ? item.UserSent.FULL_NAME : "");
                    newRow.CreateCell(11).SetCellValue((item.UserApprove != null) ? item.UserApprove.FULL_NAME : "");
                    newRow.CreateCell(12).SetCellValue(item.REQUIRED_CODE);

                    rowIndex++;
                    stt++;
                }
                wb.Write(outFileStream);
            }
            catch (Exception ex)
            {
                this.State = false;
                this.ErrorMessage = "Export file excel bị lỗi!<br/>" + ex;
            }
        }
        #endregion

        #region Nhà thầu

        public void SearchTopBid()
        {
            try
            {
                this.ObjList = UnitOfWork.Repository<BidRepo>().SearchTopBid(ProfileUtilities.User.USER_NAME).ToList();
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void SearchBidOfContractor()
        {
            try
            {
                int iTotalRecord = 0;
                this.ObjList = UnitOfWork.Repository<BidRepo>().SearchBidOfContractor(this.ObjDetail, ProfileUtilities.User.CONTRACTOR_ID, this.NumerRecordPerPage, this.Page, out iTotalRecord).ToList();
                this.TotalRecord = iTotalRecord;
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void SearchBidOfContractorWait()
        {
            try
            {
                int iTotalRecord = 0;
                this.ObjList = UnitOfWork.Repository<BidRepo>().SearchBidOfContractorWait(this.ObjDetail, ProfileUtilities.User.CONTRACTOR_ID, this.NumerRecordPerPage, this.Page, out iTotalRecord).ToList();
                this.TotalRecord = iTotalRecord;
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void SearchBidOfContractorOpen()
        {
            try
            {
                int iTotalRecord = 0;
                this.ObjList = UnitOfWork.Repository<BidRepo>().SearchBidOfContractorOpen(this.ObjDetail, ProfileUtilities.User.CONTRACTOR_ID, this.NumerRecordPerPage, this.Page, out iTotalRecord).ToList();
                this.TotalRecord = iTotalRecord;
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void SearchBidOfContractorClose()
        {
            try
            {
                int iTotalRecord = 0;
                this.ObjList = UnitOfWork.Repository<BidRepo>().SearchBidOfContractorClose(this.ObjDetail, ProfileUtilities.User.CONTRACTOR_ID, this.NumerRecordPerPage, this.Page, out iTotalRecord).ToList();
                this.TotalRecord = iTotalRecord;
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void JoinBid(string bidId, string status)
        {
            try
            {
                this.Get(bidId);
                if (this.ObjDetail == null || this.ObjDetail.GIAI_DOAN != Phase_Status.NopHoSo)
                {
                    this.State = false;
                    this.ErrorMessage = "Gói thầu không ở giai đoạn nộp hồ sơ!";
                    return;
                }

                var bidContractor = UnitOfWork.Repository<BidContractorRepo>().Queryable().FirstOrDefault(x => x.BID_ID == bidId && x.CONTRACTOR_ID == ProfileUtilities.User.CONTRACTOR_ID);
                if (bidContractor != null)
                {
                    bidContractor.STATUS = status;
                    bidContractor.UPDATE_BY = ProfileUtilities.User.USER_NAME;

                    UnitOfWork.BeginTransaction();
                    UnitOfWork.Repository<BidContractorRepo>().Update(bidContractor);
                    UnitOfWork.Commit();
                }

                else if (status == "AP")
                {
                    if (!this.ObjDetail.IS_PUBLIC)
                    {
                        this.State = false;
                        this.ErrorMessage = "Gói thầu không public ra ngoài!";
                        return;
                    }

                    if (ProfileUtilities.User.USER_TYPE == UserType.NB)
                    {
                        this.State = false;
                        this.ErrorMessage = "Tài khoản của bạn không phải là tài khoản nhà thầu!";
                        return;
                    }

                    UnitOfWork.BeginTransaction();
                    UnitOfWork.Repository<BidContractorRepo>().Create(new T_EB_BID_CONTRACTOR()
                    {
                        BID_ID = bidId,
                        CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME,
                        PKID = Guid.NewGuid().ToString(),
                        STATUS = status,
                        IS_SELF_JOIN = true
                    });
                    UnitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void CheckBidSpec()
        {
            try
            {
                var lstBidSpec = UnitOfWork.Repository<BidSpecRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID).ToList();
                var lstBidSpecContractor = UnitOfWork.Repository<BidSpecContractorRepo>().Queryable().Where(x => x.BID_ID == this.ObjDetail.PKID && x.CONTRACTOR_ID == ProfileUtilities.User.CONTRACTOR_ID).ToList();
                if (lstBidSpec.Count > 0 && (lstBidSpecContractor == null || lstBidSpecContractor.Count < 1))
                {
                    this.ObjDetail.CHECK_BID_SPEC = true;
                    return;
                }
                //else
                //{
                //    lstBidSpecContractor = lstBidSpecContractor.Where(x => x.VALUE == null || x.VALUE == "").ToList();
                //    if (lstBidSpecContractor == null || lstBidSpecContractor.Count > 0)
                //    {
                //        this.ObjDetail.CHECK_BID_SPEC = true;
                //        return;
                //    }
                //}
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        #endregion

        #region Portal
        public void SearchPortal()
        {
            var lstBid = this.CurrentRepository.GetAll();
            List<string> lstBidId = new List<string>();
            foreach (var item in lstBid.Where(x => x.GIAI_DOAN == Phase_Status.VongTiepTheo).ToList())
            {
                var newRound = lstBid.Where(x => x.PARENT_BID == item.PARENT_BID).OrderByDescending(x => x.ROUND).FirstOrDefault();
                var lstId = lstBid.Where(x => x.PARENT_BID == item.PARENT_BID && x.PKID != newRound.PKID).Select(x => x.PKID).ToList();
                lstBidId.AddRange(lstId);
            }
            foreach(var item in lstBid.Where(x => x.GIAI_DOAN == Phase_Status.DongThau && x.IS_MULTIL_ROUND).ToList())
            {
                var newRound = lstBid.Where(x => x.PARENT_BID == item.PARENT_BID).OrderByDescending(x => x.ROUND).FirstOrDefault();
                var lstId = lstBid.Where(x => x.PARENT_BID == item.PARENT_BID && x.PKID != newRound.PKID).Select(x => x.PKID).ToList();
                lstBidId.AddRange(lstId);
            }
            try
            {
                var contractorId = "";
                var isUserFecon = false;
                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    if (ProfileUtilities.User.USER_TYPE == UserType.NhaThau)
                    {
                        contractorId = ProfileUtilities.User.CONTRACTOR_ID;
                    }
                    else
                    {
                        isUserFecon = true;
                    }
                }
                int iTotalRecord = 0;
                this.NumerRecordPerPage = 5;
                this.ObjList = UnitOfWork.Repository<BidRepo>().SearchPortal(this.ObjDetail, contractorId, lstBidId, isUserFecon, this.NumerRecordPerPage, this.Page, out iTotalRecord).ToList();
                this.TotalRecord = iTotalRecord;
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }
        #endregion
    }
}
