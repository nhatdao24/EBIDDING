using NHibernate.Linq;
using SMO.Core.Entities;
using SMO.Core.Entities.AD;
using SMO.Repository.Common;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.CF;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Service.AD;
using SMO.Service.CF;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Claims;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class BidDescriptionDto
    {
        public string BidDescription { get; set; }
        public string BidDescriptionEN { get; set; }
    }
    public class BidRequireService : GenericService<T_EB_BID_REQUIRE, BidRequireRepo>
    {
        public T_EB_BID_REQUIRE ObjHeader { get; set; }
        public string LIST_PR_CODE { get; set; }

        public T_EB_BID_REQUIRE_PR ObjSearchPR { get; set; }
        public T_AD_USER ObjSearchUser { get; set; }
        public List<T_EB_BID_REQUIRE_DETAIL> ObjListDetail { get; set; }
        public List<T_EB_BID_REQUIRE_USER> ObjListUser { get; set; }
        public List<T_EB_BID_REQUIRE_PR> ObjListPR { get; set; }
        public List<T_AD_USER> ObjListSearchUser { get; set; }
        public BidRequireService() : base()
        {
            ObjHeader = new T_EB_BID_REQUIRE();
            ObjSearchUser = new T_AD_USER();
            ObjListDetail = new List<T_EB_BID_REQUIRE_DETAIL>();
            ObjListPR = new List<T_EB_BID_REQUIRE_PR>();
            ObjSearchPR = new T_EB_BID_REQUIRE_PR();
            ObjListUser = new List<T_EB_BID_REQUIRE_USER>();
            ObjListSearchUser = new List<T_AD_USER>();
        }

        public BidDescriptionDto GetBidDescription()
        {
            try
            {
                var bidDescription = UnitOfWork.Repository<ConfigTemplateNotifyRepo>()
                    .Queryable()
                    .Select(t => new BidDescriptionDto
                    {
                        BidDescription = t.BID_DESCRIPTION,
                        BidDescriptionEN = t.BID_DESCRIPTION_EN
                    })
                    .FirstOrDefault();
                return bidDescription; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null; 
            }

        }

        public override void Search()
        {
            //Tìm xem user thuộc dự án nào
            this.ObjDetail.ListFilterProject = UnitOfWork.GetSession().Query<T_AD_USER_PROJECT>().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).Select(x => x.PROJECT_CODE).ToList();
            base.Search();
        }

        public void Search_ProcessByMe()
        {
            try
            {
                int iTotalRecord = 0;
                this.ObjDetail.ListFilterProject = UnitOfWork.GetSession().Query<T_AD_USER_PROJECT>().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).Select(x => x.PROJECT_CODE).ToList();
                this.ObjList = UnitOfWork.Repository<BidRequireRepo>().Search_ProcessByMe(this.ObjDetail, this.NumerRecordPerPage, this.Page, out iTotalRecord).ToList();
                this.TotalRecord = iTotalRecord;
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void SearchUser()
        {
            try
            {
                //Tìm trong T_AD_USER_RIGHT
                var lstUserName = new List<string>();
                var findUserRight = UnitOfWork.GetSession().Query<T_AD_RIGHT_PROJECT_USER>().Where(x => x.RIGHT_CODE == "RP207" && x.PROJECT_CODE == this.ObjSearchPR.PROJECT_CODE).ToList();
                lstUserName.AddRange(findUserRight.Select(x => x.USER_NAME));

                this.ObjListSearchUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => lstUserName.Contains(x.USER_NAME)).OrderBy(x => x.USER_NAME).ToList();

                if (!string.IsNullOrEmpty(this.ObjSearchUser.USER_NAME))
                {
                    this.ObjListSearchUser = this.ObjListSearchUser.Where(x => x.USER_NAME.ToLower().Contains(this.ObjSearchUser.USER_NAME.ToLower()) || x.FULL_NAME.ToLower().Contains(this.ObjSearchUser.USER_NAME.ToLower())).ToList();
                }
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void Create(HttpRequestBase Request)
        {

            //ConfigViettinBank: Xoá phân quyền dự án

            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP202", this.ObjHeader.PROJECT_CODE);
            //if (!checkRight.Item1)
            //{
            //    this.State = false;
            //    this.ErrorMessage = checkRight.Item2;
            //    return;
            //}

            try
            {
                if (this.ObjHeader.ListUser.Count(x => x.C_LEVEL == 0) > 0)
                {
                    this.State = false;
                    this.ErrorMessage = "Thứ tự phê duyệt phải lớn hơn 0!";
                    return;
                }

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
                FileStreamService.InsertFile(lstFileStream);

                this.ObjHeader.CODE = this.GetSequence("BID_REQUIRED");
                this.ObjHeader.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
                this.ObjHeader.STATUS = EB_Status.KhoiTao;
                this.ObjHeader.ACTIVE = true;
                this.ObjHeader.COMMENT_ID = Guid.NewGuid().ToString();
                this.ObjHeader.CREATE_BY = ProfileUtilities.User.USER_NAME;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Create(this.ObjHeader);
                int numberItem = 1;
                foreach (var detail in this.ObjListDetail)
                {
                    detail.PKID = Guid.NewGuid().ToString();
                    detail.HEADER_CODE = this.ObjHeader.CODE;
                    detail.NOTE = detail.NOTE;
                    detail.ITEM_NUMBER = numberItem++;
                    detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                }

                foreach (var item in this.ObjListPR)
                {
                    item.PKID = Guid.NewGuid().ToString();
                    item.HEADER_CODE = this.ObjHeader.CODE;
                    item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                }

                foreach (var item in this.ObjListUser)
                {
                    item.PKID = Guid.NewGuid().ToString();
                    item.HEADER_CODE = this.ObjHeader.CODE;
                    item.IS_APPROVED = false;
                    item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                }
                UnitOfWork.Repository<BidRequirePRRepo>().Create(this.ObjListPR);
                UnitOfWork.Repository<BidRequireDetailRepo>().Create(this.ObjListDetail);
                UnitOfWork.Repository<BidRequireUserRepo>().Create(this.ObjListUser);
                // Tạo history
                UnitOfWork.Repository<BidRequiredHistoryRepo>().Create(new T_EB_BID_REQUIRE_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.TaoMoi,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    HEADER_CODE = this.ObjHeader.CODE,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });
                // Tạo file
                foreach (var file in lstFileStream)
                {
                    UnitOfWork.Repository<BidRequiredFileRepo>().Create(new T_EB_BID_REQUIRE_FILE()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        HEADER_CODE = this.ObjHeader.CODE,
                        FILE_ID = file.REFERENCE_ID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }

                UnitOfWork.Commit();

                var serviceUserApprove = new UserApproveService();
                serviceUserApprove.Update(this.ObjHeader.USER_APPROVE, "BID_REQUIRED");
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void Update(HttpRequestBase Request)
        {

            //ConfigViettinBank: Xoá phân quyền dự án

            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP203", this.ObjHeader.PROJECT_CODE);
            //if (!checkRight.Item1)
            //{
            //    this.State = false;
            //    this.ErrorMessage = checkRight.Item2;
            //    return;
            //}

            try
            {
                if (this.ObjHeader.ListUser.Count(x => x.C_LEVEL == 0) > 0)
                {
                    this.State = false;
                    this.ErrorMessage = "Thứ tự phê duyệt phải lớn hơn 0!";
                    return;
                }
                // Kiểm tra hack thay đổi thông tin header
                var poCheck = this.CurrentRepository.Get(this.ObjHeader.CODE);
                UnitOfWork.GetSession().Evict(poCheck);
                if (poCheck == null)
                {
                    this.State = false;
                    this.ErrorMessage = "Thông tin không hợp lệ!";
                    return;
                }

                //if (poCheck.STATUS != EB_Status.KhoiTao)
                //{
                //    this.ErrorMessage = "Thông tin không hợp lệ!";
                //    this.State = false;
                //    return;
                //}

                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;

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
                FileStreamService.InsertFile(lstFileStream);

                UnitOfWork.BeginTransaction();
                if (this.ObjHeader.ListFileDelete != null) 
                {
                    var strDeleteFile = string.Empty;
                    foreach (var item in this.ObjHeader.ListFileDelete)
                    {
                        strDeleteFile += $"DELETE T_EB_BID_REQUIRE_FILE WHERE FILE_ID = '{item}';";
                    }

                    if (!string.IsNullOrEmpty(strDeleteFile))
                    {
                        UnitOfWork.GetSession().CreateSQLQuery(strDeleteFile).ExecuteUpdate();
                    }
                }
                this.CurrentRepository.Update(this.ObjHeader);

                var strSQL = "DELETE T_EB_BID_REQUIRE_DETAIL WHERE HEADER_CODE = :HEADER_CODE; DELETE T_EB_BID_REQUIRE_PR WHERE HEADER_CODE = :HEADER_CODE; DELETE T_EB_BID_REQUIRE_USER WHERE HEADER_CODE = :HEADER_CODE";
                UnitOfWork.GetSession().CreateSQLQuery(strSQL)
                    .SetParameter("HEADER_CODE", this.ObjHeader.CODE)
                    .ExecuteUpdate();
                int numberItem = 0;
                foreach (var detail in this.ObjListDetail)
                {
                    detail.PKID = Guid.NewGuid().ToString();
                    detail.HEADER_CODE = this.ObjHeader.CODE;
                    detail.NOTE = detail.NOTE;
                    detail.ITEM_NUMBER = numberItem++;
                    detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                }

                foreach (var item in this.ObjListPR)
                {
                    item.PKID = Guid.NewGuid().ToString();
                    item.HEADER_CODE = this.ObjHeader.CODE;
                    item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                }

                foreach (var item in this.ObjListUser)
                {
                    item.PKID = Guid.NewGuid().ToString();
                    item.HEADER_CODE = this.ObjHeader.CODE;
                    item.IS_APPROVED = false;
                    item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                }
                UnitOfWork.Repository<BidRequirePRRepo>().Create(this.ObjListPR);
                UnitOfWork.Repository<BidRequireDetailRepo>().Create(this.ObjListDetail);
                UnitOfWork.Repository<BidRequireUserRepo>().Create(this.ObjListUser);
                // Tạo history
                UnitOfWork.Repository<BidRequiredHistoryRepo>().Create(new T_EB_BID_REQUIRE_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.ChinhSua,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    HEADER_CODE = this.ObjHeader.CODE,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });

                // Tạo file
                foreach (var file in lstFileStream)
                {
                    UnitOfWork.Repository<BidRequiredFileRepo>().Create(new T_EB_BID_REQUIRE_FILE()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        HEADER_CODE = this.ObjHeader.CODE,
                        FILE_ID = file.REFERENCE_ID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }
                UnitOfWork.Commit();

                var serviceUserApprove = new UserApproveService();
                serviceUserApprove.Update(this.ObjHeader.USER_APPROVE, "BID_REQUIRED");
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }


        public void Send(string code)
        {
            try
            {
                this.Get(code);


                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP205", this.ObjHeader.PROJECT_CODE);
                //if (!checkRight.Item1)
                //{
                //    this.State = false;
                //    this.ErrorMessage = checkRight.Item2;
                //    return;
                //}

                if (this.ObjHeader.ListUser.Count == 0)
                {
                    this.ErrorMessage = "Bạn chưa chọn người phê duyệt cho đề nghị thầu này!";
                    this.State = false;
                    return;
                }
                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjHeader.STATUS = EB_Status.ChoPheDuyet;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);
                // Tạo history
                UnitOfWork.Repository<BidRequiredHistoryRepo>().Create(new T_EB_BID_REQUIRE_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.TrinhDuyet,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    HEADER_CODE = this.ObjHeader.CODE,
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

                if (this.ObjHeader.STATUS == EB_Status.DaPheDuyet || this.ObjHeader.STATUS == EB_Status.TuChoi)
                {
                    subjectTemplate = template.CONG_VIEC_HOAN_THANH_SUBJECT;
                    bodyTemplate = template.CONG_VIEC_HOAN_THANH_BODY;
                }

                if (string.IsNullOrWhiteSpace(subjectTemplate) || string.IsNullOrWhiteSpace(bodyTemplate))
                {
                    return;
                }

                var contentSubject = subjectTemplate
                        .Replace(LOAI_CONG_VIEC, "Đề nghị mở thầu")
                        .Replace(MA, this.ObjHeader.CODE)
                        .Replace(TEN, this.ObjHeader.NAME)
                        .Replace(DU_AN, this.ObjHeader.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjHeader.Project.NAME_EN)
                        .Replace(TRANG_THAI, EB_Status.GetStatusText(this.ObjHeader.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBidRequire?id=" + this.ObjHeader.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjHeader.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjHeader.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjHeader.UserSent.FULL_NAME);

                if (this.ObjHeader.ListUser.Count > 0)
                {
                    var fullName = "";
                    foreach (var user in this.ObjHeader.ListUser)
                    {
                        fullName += user.User.FULL_NAME + ", ";
                    }
                    contentSubject = contentSubject.Replace(HO_TEN_NGUOI_PHE_DUYET, fullName);
                }
                if (this.ObjHeader.UserSent.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DU_AN, this.ObjHeader.Project.NAME);
                }

                var history = UnitOfWork.Repository<BidRequiredHistoryRepo>().Queryable().Where(x => x.HEADER_CODE == this.ObjHeader.CODE).OrderByDescending(x => x.CREATE_DATE).Select(x => x.ACTION).FirstOrDefault();
                var lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_TYPE == UserType.NB).ToList();
                var userCreate = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_NAME == this.ObjHeader.CREATE_BY).FirstOrDefault();

                var lstMailNotify = new List<string>();
                var bidCurrentUser = this.ObjHeader.ListUser.Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.IS_APPROVED == true).FirstOrDefault();
                var lstCurrentLevel = this.ObjHeader.ListUser.Where(x => x.IS_APPROVED == false).ToList();
                if (bidCurrentUser != null)
                {
                    lstCurrentLevel = lstCurrentLevel.Where(x => x.C_LEVEL == bidCurrentUser.C_LEVEL && x.USER_NAME != bidCurrentUser.USER_NAME && x.IS_APPROVED == false).ToList();
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Đề nghị mở thầu")
                        .Replace(MA, this.ObjHeader.CODE)
                        .Replace(TEN, this.ObjHeader.NAME)
                        .Replace(DU_AN, this.ObjHeader.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjHeader.Project.NAME_EN)
                        .Replace(TRANG_THAI, EB_Status.GetStatusText(this.ObjHeader.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenBidRequire?id=" + this.ObjHeader.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjHeader.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjHeader.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjHeader.UserSent.FULL_NAME);

                if (this.ObjHeader.ListUser.Count > 0)
                {
                    var fullName = "";
                    foreach (var user in this.ObjHeader.ListUser)
                    {
                        fullName += user.User.FULL_NAME + ", ";
                    }
                    contentBody = contentBody.Replace(HO_TEN_NGUOI_PHE_DUYET, fullName);
                }
                if (this.ObjHeader.UserSent.Organize != null)
                {
                    contentBody = contentBody.Replace(DU_AN, this.ObjHeader.Project.NAME);
                }

                if ((this.ObjHeader.STATUS == EB_Status.ChoPheDuyet && history != Action.HuyPheDuyet) || (this.ObjHeader.STATUS == EB_Status.DangPheDuyet && lstCurrentLevel.Count < 1) || (this.ObjHeader.STATUS == EB_Status.KhoiTao && history == Action.HuyTrinhDuỵet))
                {
                    var lstUserApprove = this.ObjHeader.ListUser;
                    if (this.ObjHeader.STATUS == EB_Status.DangPheDuyet && bidCurrentUser != null)
                    {
                        lstUserApprove = lstUserApprove.Where(x => x.IS_APPROVED == false && x.C_LEVEL > bidCurrentUser.C_LEVEL).ToList();
                    }
                    else
                    {
                        lstUserApprove = lstUserApprove.Where(x => x.IS_APPROVED == false).ToList();
                    }
                    var minLevel = lstUserApprove.Min(x => x.C_LEVEL);
                    var lstUserNameApprove = lstUserApprove.Where(x => x.C_LEVEL == minLevel).Select(x => x.USER_NAME).ToList();

                    foreach (var userName in lstUserNameApprove)
                    {
                        var userMail = lstUser.Where(x => x.USER_NAME == userName).FirstOrDefault();
                        serviceEmail.ObjDetail = new T_CM_EMAIL()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            EMAIL = userMail.EMAIL,
                            SUBJECT = contentSubject,
                            CONTENTS = contentBody
                        };
                        serviceEmail.Create();
                    }

                }
                else if (this.ObjHeader.STATUS == EB_Status.DaPheDuyet)
                {
                    var userSendPackage = UnitOfWork.Repository<BidRequiredHistoryRepo>().Queryable().Where(x => x.HEADER_CODE == this.ObjHeader.CODE && x.ACTION == Action.TrinhDuyet).OrderByDescending(x => x.CREATE_DATE).Select(x => x.ACTION_USER).FirstOrDefault();
                    if (userSendPackage != this.ObjHeader.CREATE_BY)
                    {
                        var mailUserSendPackage = lstUser.Where(x => x.USER_NAME == userSendPackage).Select(x => x.EMAIL).FirstOrDefault();
                        lstMailNotify.Add(mailUserSendPackage);
                    }
                    lstMailNotify.Add(userCreate.EMAIL);

                    var userSend = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).FirstOrDefault();

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
                }
                else if (this.ObjHeader.STATUS == EB_Status.TuChoi)
                {
                    var mailUserSendPackage = lstUser.Where(x => x.USER_NAME == this.ObjHeader.CREATE_BY).Select(x => x.EMAIL).FirstOrDefault();
                    lstMailNotify.Add(mailUserSendPackage);

                    var lstUserApprove = this.ObjHeader.ListUser.Where(x => x.IS_APPROVED == true).Select(x => x.USER_NAME).ToList();
                    var lstMailUserApprove = lstUser.Where(x => lstUserApprove.Contains(x.USER_NAME)).Select(x => x.EMAIL).ToList();
                    lstMailNotify.AddRange(lstMailUserApprove);

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
                }
                else if (this.ObjHeader.STATUS == EB_Status.ChoCapNhat)
                {
                    var userSendPackage = UnitOfWork.Repository<BidRequiredHistoryRepo>().Queryable().Where(x => x.HEADER_CODE == this.ObjHeader.CODE && x.ACTION == Action.TrinhDuyet).OrderByDescending(x => x.CREATE_DATE).Select(x => x.ACTION_USER).FirstOrDefault();
                    if (userSendPackage != this.ObjHeader.CREATE_BY)
                    {
                        var mailUserSendPackage = lstUser.Where(x => x.USER_NAME == userSendPackage).Select(x => x.EMAIL).FirstOrDefault();
                        lstMailNotify.Add(mailUserSendPackage);
                    }
                    lstMailNotify.Add(userCreate.EMAIL);

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
                }
                else if ((this.ObjHeader.STATUS == EB_Status.ChoPheDuyet || this.ObjHeader.STATUS == EB_Status.DangPheDuyet) && history == Action.HuyPheDuyet)
                {

                    var lstUserApprove = this.ObjHeader.ListUser.Where(x => x.USER_NAME != ProfileUtilities.User.USER_NAME).Select(x => x.USER_NAME).ToList();
                    var lstMailUserApprove = lstUser.Where(x => lstUserApprove.Contains(x.USER_NAME)).Select(x => x.EMAIL).ToList();
                    lstMailNotify.AddRange(lstMailUserApprove);
                    lstMailNotify.Add(userCreate.EMAIL);

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
                }
            }
            catch (Exception)
            {

            }
        }


        public void CreateNotify()
        {
            try
            {
                var lstUserNotify = new List<string>();
                var newId = Guid.NewGuid().ToString();
                var serviceNotify = new NotifyService();
                var lstUser = new List<string>();
                lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_TYPE == UserType.NB).Select(x => x.USER_NAME).ToList();

                var history = UnitOfWork.Repository<BidRequiredHistoryRepo>().Queryable().Where(x => x.HEADER_CODE == this.ObjHeader.CODE).OrderByDescending(x => x.CREATE_DATE).Select(x => x.ACTION).FirstOrDefault();
                var bidRequireCurrentUser = this.ObjHeader.ListUser.Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.IS_APPROVED == true).FirstOrDefault();
                var lstCurrentLevel = this.ObjHeader.ListUser.Where(x => x.IS_APPROVED == false).ToList();
                if (bidRequireCurrentUser != null)
                {
                    lstCurrentLevel = lstCurrentLevel.Where(x => x.C_LEVEL == bidRequireCurrentUser.C_LEVEL && x.USER_NAME != bidRequireCurrentUser.USER_NAME && x.IS_APPROVED == false).ToList();
                }

                if ((this.ObjHeader.STATUS == EB_Status.ChoPheDuyet && history != Action.HuyPheDuyet) || (this.ObjHeader.STATUS == EB_Status.DangPheDuyet && lstCurrentLevel.Count < 1))
                {
                    var lstUserApprove = this.ObjHeader.ListUser;
                    if (this.ObjHeader.STATUS == EB_Status.DangPheDuyet && bidRequireCurrentUser != null)
                    {
                        lstUserApprove = lstUserApprove.Where(x => x.IS_APPROVED == false && x.C_LEVEL > bidRequireCurrentUser.C_LEVEL).ToList();
                    }
                    else
                    {
                        lstUserApprove = lstUserApprove.Where(x => x.IS_APPROVED == false).ToList();
                    }

                    var minLevel = lstUserApprove.Min(x => x.C_LEVEL);
                    var lstUserNameApprove = lstUserApprove.Where(x => x.C_LEVEL == minLevel).Select(x => x.USER_NAME).ToList();

                    lstUserNotify.AddRange(lstUserNameApprove);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Đề nghị thầu [{4} - {5}] đang chờ phê duyệt!</span>
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
                            <span>Bid proposal [{4} - {5}] is awaiting approval!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    foreach (var userName in lstUserNotify)
                    {
                        string strContent = string.Format(strTemplate,
                        Guid.NewGuid().ToString(),
                        $"/EB/BidRequire/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                       string strContent_EN = string.Format(strTemplate_EN,
                       Guid.NewGuid().ToString(),
                       $"/EB/BidRequire/Detail?id={this.ObjHeader.CODE}",
                       EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                       EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                       this.ObjHeader.CODE,
                       this.ObjHeader.NAME,
                       DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Đề nghị thầu {this.ObjHeader.CODE} đang chờ phê duyệt!";
                        string strRawContent_EN = $"Bid proposal{this.ObjHeader.CODE} is awaiting approval!";


                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent_EN,
                            RAW_CONTENTS_EN = strRawContent_EN,
                            USER_NAME = userName
                        };
                        serviceNotify.Create();
                    }
                }
                else if (this.ObjHeader.STATUS == EB_Status.KhoiTao && history == Action.HuyTrinhDuỵet)
                {
                    var lstUserApprove = this.ObjHeader.ListUser;
                    var minLevel = lstUserApprove.Min(x => x.C_LEVEL);
                    var lstUserNameApprove = lstUserApprove.Where(x => x.C_LEVEL == minLevel).Select(x => x.USER_NAME).ToList();

                    lstUserNotify.AddRange(lstUserNameApprove);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Đề nghị thầu [{4} - {5}] đã hủy trình duyệt!</span>
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
                            <span>Bid proposal [{4} - {5}] has been canceled!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    foreach (var userName in lstUserNameApprove)
                    {

                        string strContent = string.Format(strTemplate,
                        Guid.NewGuid().ToString(),
                        $"/EB/BidRequire/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strContent_EN = string.Format(strTemplate_EN,
                        Guid.NewGuid().ToString(),
                        $"/EB/BidRequire/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Đề nghị thầu {this.ObjHeader.CODE} đã hủy trình duyệt!";
                        string strRawContent_EN = $"Bid proposal {this.ObjHeader.CODE} has been canceled!!";


                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent_EN,
                            RAW_CONTENTS_EN = strRawContent_EN,
                            USER_NAME = userName
                        };
                        serviceNotify.Create();
                    }
                }
                else if (this.ObjHeader.STATUS == EB_Status.DaPheDuyet)
                {
                    var userSendPackage = UnitOfWork.Repository<BidRequiredHistoryRepo>().Queryable().Where(x => x.HEADER_CODE == this.ObjHeader.CODE && x.ACTION == Action.TrinhDuyet).OrderByDescending(x => x.CREATE_DATE).Select(x => x.ACTION_USER).FirstOrDefault();
                    if (userSendPackage != this.ObjHeader.CREATE_BY)
                    {
                        lstUserNotify.Add(userSendPackage);
                    }
                    lstUserNotify.Add(this.ObjHeader.CREATE_BY);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Đề nghị thầu [{4} - {5}] đã được phê duyệt!</span>
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
                            <span>Bid proposal [{4} - {5}] has been approved!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    foreach (var userName in lstUserNotify)
                    {
                        string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/BidRequire/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Đề nghị thầu {this.ObjHeader.CODE} đã được phê duyệt!";

                        string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/BidRequire/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent_EN = $"Bid proposal {this.ObjHeader.CODE} has been approved!";

                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = newId,
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent_EN,
                            RAW_CONTENTS_EN = strRawContent_EN,
                            USER_NAME = userName
                        };
                        serviceNotify.Create();
                    }
                }
                else if (this.ObjHeader.STATUS == EB_Status.ChoCapNhat)
                {
                    var userSendPackage = UnitOfWork.Repository<BidRequiredHistoryRepo>().Queryable().Where(x => x.HEADER_CODE == this.ObjHeader.CODE && x.ACTION == Action.TrinhDuyet).OrderByDescending(x => x.CREATE_DATE).Select(x => x.ACTION_USER).FirstOrDefault();
                    if (userSendPackage != this.ObjHeader.CREATE_BY)
                    {
                        lstUserNotify.Add(userSendPackage);
                    }
                    lstUserNotify.Add(this.ObjHeader.CREATE_BY);

                    var lstUserApproved = UnitOfWork.Repository<BidRequireUserRepo>().Queryable().Where(x => x.HEADER_CODE == this.ObjHeader.CODE && x.IS_APPROVED == true).Select(x => x.USER_NAME).ToList();
                    if (lstUserApproved.Count > 0)
                    {
                        lstUserNotify.AddRange(lstUserApproved);
                    }

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Đề nghị thầu [{4} - {5}] chưa được phê duyệt. Yêu cầu cập nhật thêm thông tin!</span>
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
                            <span>Bid proposals [{4} - {5}] have not been approved. Request to update more information!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    foreach (var userName in lstUserNotify)
                    {
                        string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/BidRequire/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Đề nghị thầu {this.ObjHeader.CODE} chưa được phê duyệt. Yêu cầu cập nhật thêm thông tin!";

                        string strContent_EN = string.Format(strTemplate_EN,
                       newId,
                       $"/EB/BidRequire/Detail?id={this.ObjHeader.CODE}",
                       EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                       EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                       this.ObjHeader.CODE,
                       this.ObjHeader.NAME,
                       DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent_EN = $"Bid proposals {this.ObjHeader.CODE} have not been approved. Request to update more information!";

                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = newId,
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent_EN,
                            RAW_CONTENTS_EN = strRawContent_EN,
                            USER_NAME = userName
                        };
                        serviceNotify.Create();
                    }
                }
                else if (this.ObjHeader.STATUS == EB_Status.TuChoi)
                {
                    lstUserNotify.Add(this.ObjHeader.CREATE_BY);
                    var query = UnitOfWork.Repository<BidRequiredHistoryRepo>().Queryable();
                    var userSendPackage = query.Where(x => x.HEADER_CODE == this.ObjHeader.CODE && x.ACTION == Action.TrinhDuyet).OrderByDescending(x => x.CREATE_DATE).Select(x => x.ACTION_USER).FirstOrDefault();
                    if (userSendPackage != this.ObjHeader.CREATE_BY)
                    {
                        lstUserNotify.Add(userSendPackage);
                    }
                    var lstUserApproval = query.Where(x => x.HEADER_CODE == this.ObjHeader.CODE && x.ACTION == Action.PheDuyet).OrderByDescending(x => x.CREATE_DATE).Select(x => x.ACTION_USER).ToList();
                    if (lstUserApproval.Count > 0)
                    {
                        foreach (var user in lstUserApproval)
                        {
                            lstUserNotify.Add(user);
                        }
                    }

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>Đề nghị thầu [{4} - {5}] đã bị từ chối!</span>
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
                            <span>Bid proposals [{4} - {5}] has been rejected!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                ";

                    foreach (var userName in lstUserNotify)
                    {
                        string strContent = string.Format(strTemplate,
                        newId,
                        $"/EB/BidRequire/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent = $"Đề nghị thầu {this.ObjHeader.CODE} đã bị từ chối!";

                        string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/EB/BidRequire/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                        string strRawContent_EN = $"Bid proposals {this.ObjHeader.CODE} has been rejected!!";

                        serviceNotify.ObjDetail = new T_CM_NOTIFY()
                        {
                            PKID = newId,
                            CONTENTS = strContent,
                            RAW_CONTENTS = strRawContent,
                            CONTENTS_EN = strContent_EN,
                            RAW_CONTENTS_EN = strRawContent_EN,
                            USER_NAME = userName
                        };
                        serviceNotify.Create();
                    }
                }

                SMOUtilities.SendNotify(lstUserNotify);
            }
            catch (Exception ex)
            {
            }

        }

        public void Cancel(string code)
        {
            try
            {
                this.Get(code);

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP206", this.ObjHeader.PROJECT_CODE);
                //if (!checkRight.Item1)
                //{
                //    this.State = false;
                //    this.ErrorMessage = checkRight.Item2;
                //    return;
                //}

                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjHeader.STATUS = EB_Status.Huy;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);
                // Tạo history
                UnitOfWork.Repository<BidRequiredHistoryRepo>().Create(new T_EB_BID_REQUIRE_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.Huy,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    HEADER_CODE = this.ObjHeader.CODE,
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

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP207", this.ObjHeader.PROJECT_CODE);
                //if (!checkRight.Item1)
                //{
                //    this.State = false;
                //    this.ErrorMessage = checkRight.Item2;
                //    return;
                //}

                var userApprove = this.ObjHeader.ListUser.FirstOrDefault(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.IS_APPROVED == false);
                if (userApprove == null)
                {
                    this.State = false;
                    return;
                }

                // Kiểm tra user level thấp hơn đã phê duyệt hay chưa
                if (this.ObjHeader.ListUser.FirstOrDefault(x => x.C_LEVEL < userApprove.C_LEVEL && x.IS_APPROVED == false) != null)
                {
                    this.State = false;
                    return;
                }

                userApprove.IS_APPROVED = true;
                userApprove.DATE_APPROVE = DateTime.Now;
                this.ObjHeader.STATUS = EB_Status.DaPheDuyet;
                if (this.ObjHeader.ListUser.Count(x => x.IS_APPROVED == false) > 0)
                {
                    this.ObjHeader.STATUS = EB_Status.DangPheDuyet;
                }
                userApprove.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);
                // Tạo history
                UnitOfWork.Repository<BidRequiredHistoryRepo>().Create(new T_EB_BID_REQUIRE_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.PheDuyet,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    HEADER_CODE = this.ObjHeader.CODE,
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

        public void CancelApprove(string code)
        {
            try
            {
                this.Get(code);

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP207", this.ObjHeader.PROJECT_CODE);
                //if (!checkRight.Item1)
                //{
                //    this.State = false;
                //    this.ErrorMessage = checkRight.Item2;
                //    return;
                //}

                var userApprove = this.ObjHeader.ListUser.FirstOrDefault(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.IS_APPROVED == true);
                if (userApprove == null)
                {
                    this.State = false;
                    return;
                }

                // Kiểm tra user level thấp hơn đã phê duyệt hay chưa
                if (this.ObjHeader.ListUser.FirstOrDefault(x => x.C_LEVEL < userApprove.C_LEVEL && x.IS_APPROVED == false) != null)
                {
                    this.State = false;
                    return;
                }

                userApprove.IS_APPROVED = false;
                userApprove.DATE_APPROVE = null;
                this.ObjHeader.STATUS = EB_Status.ChoPheDuyet;
                if (this.ObjHeader.ListUser.Count(x => x.C_LEVEL < userApprove.C_LEVEL && x.IS_APPROVED == true) > 0)
                {
                    this.ObjHeader.STATUS = EB_Status.DangPheDuyet;
                }
                userApprove.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);
                // Tạo history
                UnitOfWork.Repository<BidRequiredHistoryRepo>().Create(new T_EB_BID_REQUIRE_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.HuyPheDuyet,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    HEADER_CODE = this.ObjHeader.CODE,
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

        public void Reject(string code, string reason)
        {
            try
            {
                this.Get(code);


                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP208", this.ObjHeader.PROJECT_CODE);
                //if (!checkRight.Item1)
                //{
                //    this.State = false;
                //    this.ErrorMessage = checkRight.Item2;
                //    return;
                //}

                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjHeader.STATUS = EB_Status.TuChoi;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);
                // Tạo history
                UnitOfWork.Repository<BidRequiredHistoryRepo>().Create(new T_EB_BID_REQUIRE_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.TuChoi,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    HEADER_CODE = this.ObjHeader.CODE,
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

        public void RequireUpdate(string code, string reason)
        {
            try
            {
                this.Get(code);

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP207", this.ObjHeader.PROJECT_CODE);
                //if (!checkRight.Item1)
                //{
                //    this.State = false;
                //    this.ErrorMessage = checkRight.Item2;
                //    return;
                //}

                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjHeader.STATUS = EB_Status.ChoCapNhat;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);
                // Tạo history
                UnitOfWork.Repository<BidRequiredHistoryRepo>().Create(new T_EB_BID_REQUIRE_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.YeuCauCapNhat,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    HEADER_CODE = this.ObjHeader.CODE,
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

        public override void Get(object id, dynamic param = null)
        {
            try
            {
                this.ObjHeader = this.CurrentRepository.Get(id, param);
                if (UtilsRepo.IsPropertyExist(param, "IsFetch"))
                {
                    foreach (var pr in this.ObjHeader.ListPR)
                    {
                        if (pr.TYPE == "SAP")
                        {
                            var prSAP = UnitOfWork.Repository<PRSAPRepo>().Get(pr.PR_CODE);
                            pr.PR_NAME = prSAP.REQUIRE_NAME;
                        }
                        else
                        {
                            var prEB = UnitOfWork.Repository<PREBRepo>().Get(pr.PR_CODE);
                            pr.PR_NAME = prEB.REQUIRE_NAME;
                            foreach (var detail in prEB.ListDetail)
                            {
                                pr.ListDetail.Add(new T_PR_SAP_DETAIL()
                                {
                                    MATERIAL_NAME = detail.Material.TEXT,
                                    MATERIAL_CODE = detail.MATERIAL_CODE,
                                    QUANTITY = detail.QUANTITY,
                                    UNIT = detail.UNIT_CODE,
                                    SUM_VALUE = detail.SUM_VALUE,
                                    CURRENCY = detail.CURRENCY_CODE
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }
    }
}
