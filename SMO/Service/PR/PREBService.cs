using Azure.Core;
using Newtonsoft.Json;
using SharpSapRfc;
using SharpSapRfc.Plain;
using SMO.Core.Entities;
using SMO.Core.Entities.AD;
using SMO.Core.Entities.PR;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Repository.Implement.PR;
using SMO.Repository.Interface.PR;
using SMO.SAPINT;
using SMO.SAPINT.Function;
using SMO.Service.AD;
using SMO.Service.CF;
using SMO.Service.Class;
using SMO.Service.CM;
using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Configuration; 
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.PR 
{
    public class PREBService : GenericService<T_PR_EB_HEADER, PREBRepo>
    {
        public T_PR_EB_HEADER ObjHeader { get; set; }
        public List<T_PR_EB_DETAIL> ObjListDetail { get; set; }

        public PREBService() : base()
        {
            ObjHeader = new T_PR_EB_HEADER();
            ObjListDetail = new List<T_PR_EB_DETAIL>();
        }

        public void Search(string lstPrCodeJson)
        {
            if(lstPrCodeJson == null)
            {
                this.ObjDetail.ListFilterProject = UnitOfWork.GetSession().Query<T_AD_USER_PROJECT>().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).Select(x => x.PROJECT_CODE).ToList();
                base.Search();

            }
            else
            {
                string[] prCodes = JsonConvert.DeserializeObject<string[]>(lstPrCodeJson);
                List<string> prCodeList = prCodes.ToList();
                //Tìm xem user thuộc dự án nào
                this.ObjDetail.ListFilterProject = UnitOfWork.GetSession().Query<T_AD_USER_PROJECT>().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).Select(x => x.PROJECT_CODE).ToList();

                this.ObjDetail.ListPrForAdd = prCodeList;

                base.Search();
            }
           
        }


        public void Create(HttpRequestBase Request)
        {

            //ConfigViettinBank: Xoá phân quyền dự án

            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP102", this.ObjHeader.PROJECT_CODE);
            //if (!checkRight.Item1)
            //{
            //    this.State = false;
            //    this.ErrorMessage= checkRight.Item2;
            //    return;
            //}

            try
            {
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


                this.ObjHeader.CODE = "E" + this.GetSequence("PR");
                this.ObjHeader.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
                this.ObjHeader.STATUS = EB_Status.KhoiTao;
                this.ObjHeader.ACTIVE = true;
                this.ObjHeader.CREATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjHeader.COMMENT_ID = Guid.NewGuid().ToString();

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Create(this.ObjHeader);

                int numberItem = 1;
                foreach (var detail in this.ObjListDetail)
                {
                    detail.PKID = Guid.NewGuid().ToString();
                    detail.ITEM_NUMBER = numberItem++;
                    detail.HEADER_CODE = this.ObjHeader.CODE;
                    detail.NOTE = detail.NOTE;
                    detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                }

                UnitOfWork.Repository<PREBDetailRepo>().Create(this.ObjListDetail);
                // Tạo history
                UnitOfWork.Repository<PREBHistoryRepo>().Create(new T_PR_EB_HISTORY()
                {
                    PKID = Guid.NewGuid().ToString(),
                    ACTION = Action.TaoMoi,
                    ACTION_USER = ProfileUtilities.User.USER_NAME,
                    ACTION_DATE = DateTime.Now,
                    HEADER_CODE = this.ObjHeader.CODE,
                    CREATE_BY = ProfileUtilities.User.USER_NAME
                });


                foreach (var file in lstFileStream)
                {
                    UnitOfWork.Repository<PREBFileRepo>().Create(new T_PR_EB_FILE()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        HEADER_CODE = this.ObjHeader.CODE,
                        FILE_ID = file.REFERENCE_ID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }
                UnitOfWork.Commit();

                var serviceUserApprove = new UserApproveService();
                serviceUserApprove.Update(this.ObjHeader.USER_APPROVE, "PREB");
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

            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP103", this.ObjHeader.PROJECT_CODE);
            //if (!checkRight.Item1)
            //{
            //    this.State = false;
            //    this.ErrorMessage = checkRight.Item2;
            //    return;
            //}
            try
            {
                // Kiểm tra hack thay đổi thông tin header
                var poCheck = this.CurrentRepository.Get(this.ObjHeader.CODE);
                UnitOfWork.GetSession().Evict(poCheck);
                if (poCheck == null)
                {
                    this.State = false;
                    this.ErrorMessage = "Thông tin không hợp lệ!";
                    return;
                }

                //if (poCheck.STATUS != EB_Status.ChoCapNhat || poCheck.STATUS)
                //{
                //    this.ErrorMessage = "Thông tin không hợp lệ!";
                //    this.State = false;
                //    return;
                //}

                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;

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
                        strDeleteFile += $"DELETE T_PR_EB_FILE WHERE FILE_ID = '{item}';";
                    }

                    if (!string.IsNullOrEmpty(strDeleteFile))
                    {
                        UnitOfWork.GetSession().CreateSQLQuery(strDeleteFile).ExecuteUpdate();
                    }
                }
                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);

                var strSQL = "DELETE T_PR_EB_DETAIL WHERE HEADER_CODE = :HEADER_CODE";
                UnitOfWork.GetSession().CreateSQLQuery(strSQL)
                    .SetParameter("HEADER_CODE", this.ObjHeader.CODE)
                    .ExecuteUpdate();

                int numberItem = 1;
                foreach (var detail in this.ObjListDetail)
                {
                    detail.PKID = Guid.NewGuid().ToString();
                    detail.ITEM_NUMBER = numberItem++;
                    detail.HEADER_CODE = this.ObjHeader.CODE;
                    detail.NOTE = detail.NOTE;
                    detail.CREATE_BY = ProfileUtilities.User.USER_NAME;
                }

                UnitOfWork.Repository<PREBDetailRepo>().Create(this.ObjListDetail);

                UnitOfWork.Repository<PREBHistoryRepo>().Create(new T_PR_EB_HISTORY()
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
                    UnitOfWork.Repository<PREBFileRepo>().Create(new T_PR_EB_FILE()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        HEADER_CODE = this.ObjHeader.CODE,
                        FILE_ID = file.REFERENCE_ID,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }

                UnitOfWork.Commit();

                var serviceUserApprove = new UserApproveService();
                serviceUserApprove.Update(this.ObjHeader.USER_APPROVE, "PREB");
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void SendPR(string code)
        {
            try
            {
                this.Get(code);

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP105", this.ObjHeader.PROJECT_CODE);
                //if (!checkRight.Item1)
                //{
                //    this.State = false;
                //    this.ErrorMessage = checkRight.Item2;
                //    return;
                //}

                if (string.IsNullOrWhiteSpace(this.ObjHeader.USER_APPROVE))
                {
                    this.ErrorMessage = "Bạn chưa chọn người phê duyệt cho PR này!";
                    this.State = false;
                    return;
                }

                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjHeader.STATUS = EB_Status.ChoPheDuyet;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);
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
                        .Replace(LOAI_CONG_VIEC, "Yêu cầu mua sắm (PR)")
                        .Replace(MA, this.ObjHeader.CODE)
                        .Replace(DU_AN, this.ObjHeader.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjHeader.Project.NAME_EN)
                        .Replace(TEN, this.ObjHeader.REQUIRE_NAME)
                        .Replace(TRANG_THAI, EB_Status.GetStatusText(this.ObjHeader.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenPR?id=" + this.ObjHeader.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjHeader.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjHeader.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjHeader.UserSent.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, this.ObjHeader.UserApprove.FULL_NAME);

                if (this.ObjHeader.UserSent.Organize != null)
                {
                    contentSubject = contentSubject.Replace(DU_AN, this.ObjHeader.Project.NAME);
                }

                var contentBody = bodyTemplate
                        .Replace(LOAI_CONG_VIEC, "Yêu cầu mua sắm (PR)")
                        .Replace(MA, this.ObjHeader.CODE)
                        .Replace(TEN, this.ObjHeader.REQUIRE_NAME)
                        .Replace(DU_AN, this.ObjHeader.Project.NAME)
                        .Replace(DU_AN_EN, this.ObjHeader.Project.NAME_EN)

                        .Replace(TRANG_THAI, EB_Status.GetStatusText(this.ObjHeader.STATUS))
                        .Replace(LINK_CHI_TIET, url + "Admin/OpenPR?id=" + this.ObjHeader.CODE)
                        .Replace(USER_NAME_NGUOI_DE_XUAT, this.ObjHeader.CREATE_BY)
                        .Replace(USER_NAME_NGUOI_PHE_DUYET, this.ObjHeader.USER_APPROVE)
                        .Replace(HO_TEN_NGUOI_DE_XUAT, this.ObjHeader.UserSent.FULL_NAME)
                        .Replace(HO_TEN_NGUOI_PHE_DUYET, this.ObjHeader.UserApprove.FULL_NAME);

                if (this.ObjHeader.UserSent.Organize != null)
                {
                    contentBody = contentBody.Replace(DU_AN, this.ObjHeader.Project.NAME);
                }

                if (this.ObjHeader.STATUS == EB_Status.ChoPheDuyet)
                {
                    serviceEmail.ObjDetail = new T_CM_EMAIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        EMAIL = this.ObjHeader.UserApprove.EMAIL,
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
                        EMAIL = this.ObjHeader.UserSent.EMAIL,
                        SUBJECT = contentSubject,
                        CONTENTS = contentBody
                    };
                    serviceEmail.Create();
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

                if (this.ObjHeader.STATUS == EB_Status.ChoPheDuyet)
                {
                    lstUserNotify.Add(this.ObjHeader.USER_APPROVE);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>PR [{4} - {5}]  đang chờ phê duyệt!</span>
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
                            <span>PR [{4} - {5}]  pending approval!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                    ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/PR/PREB/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.REQUIRE_NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"PR {this.ObjHeader.CODE} đang chờ phê duyệt!";

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/PR/PREB/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.REQUIRE_NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent_EN = $"PR {this.ObjHeader.CODE} pending approval!";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = this.ObjHeader.USER_APPROVE
                    };
                    serviceNotify.Create();
                }
                else if (this.ObjHeader.STATUS == EB_Status.DaPheDuyet)
                {
                    lstUserNotify.Add(this.ObjHeader.CREATE_BY);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>PR [{4} - {5}] đã được phê duyệt!</span>
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
                            <span>PR [{4} - {5}] has been approved!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                    ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/PR/PREB/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.REQUIRE_NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"PR {this.ObjHeader.CODE} đã được phê duyệt!";

                    string strContent_EN = string.Format(strTemplate_EN,
                       newId,
                       $"/PR/PREB/Detail?id={this.ObjHeader.CODE}",
                       EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                       EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                       this.ObjHeader.CODE,
                       this.ObjHeader.REQUIRE_NAME,
                       DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent_EN = $"PR {this.ObjHeader.CODE} has been approved!";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = this.ObjHeader.CREATE_BY
                    };
                    serviceNotify.Create();
                }
                else if (this.ObjHeader.STATUS == EB_Status.ChoCapNhat)
                {
                    lstUserNotify.Add(this.ObjHeader.CREATE_BY);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>PR [{4} - {5}] Chưa được phê duyệt.Yêu cầu cập nhật thêm!</span>
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
                            <span>PR [{4} - {5}] Not approved yet. Further update required!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                    ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/PR/PREB/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.REQUIRE_NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"PR {this.ObjHeader.CODE} Chờ cập nhật!";

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/PR/PREB/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.REQUIRE_NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent_EN = $"PR {this.ObjHeader.CODE} Wait for update!";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = this.ObjHeader.CREATE_BY
                    };
                    serviceNotify.Create();
                }
                else if (this.ObjHeader.STATUS == EB_Status.TuChoi)
                {
                    lstUserNotify.Add(this.ObjHeader.CREATE_BY);

                    string strTemplate = @"
                    <a href='#' id='a{0}' onclick = 'SendNotifyIsReaded(""{0}""); Forms.LoadAjax(""{1}"");'>
                        <div class='icon-circle {2}'>
                            <i class='material-icons'>{3}</i>
                        </div>
                        <div class='menu-info'>
                            <span>PR [{4} - {5}] đã bị từ chối!</span>
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
                            <span>PR [{4} - {5}] has been rejected!</span>
                            <p>
                                <i class='material-icons'>access_time</i> {6}
                            </p>
                        </div>
                    </a>
                    ";

                    string strContent = string.Format(strTemplate,
                        newId,
                        $"/PR/PREB/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.REQUIRE_NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent = $"PR {this.ObjHeader.CODE} đã bị từ chối!";

                    string strContent_EN = string.Format(strTemplate_EN,
                        newId,
                        $"/PR/PREB/Detail?id={this.ObjHeader.CODE}",
                        EB_Status.GetStatusColor(this.ObjHeader.STATUS),
                        EB_Status.GetStatusIcon(this.ObjHeader.STATUS),
                        this.ObjHeader.CODE,
                        this.ObjHeader.REQUIRE_NAME,
                        DateTime.Now.ToString(Global.DateTimeToStringFormat));

                    string strRawContent_EN = $"PR {this.ObjHeader.CODE} has been rejected !";

                    serviceNotify.ObjDetail = new T_CM_NOTIFY()
                    {
                        PKID = newId,
                        CONTENTS = strContent,
                        RAW_CONTENTS = strRawContent,
                        CONTENTS_EN = strContent_EN,
                        RAW_CONTENTS_EN = strRawContent_EN,
                        USER_NAME = this.ObjHeader.CREATE_BY
                    };
                    serviceNotify.Create();
                }

                SMOUtilities.SendNotify(lstUserNotify);
            }
            catch (Exception)
            {
            }
            
        }

        public void CancelPR(string code)
        {
            try
            {
                this.Get(code);

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP106", this.ObjHeader.PROJECT_CODE);
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
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void ApprovePR(string code, string reason)
        {
            try
            {
                this.Get(code);

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP107", this.ObjHeader.PROJECT_CODE);
                //if (!checkRight.Item1)
                //{
                //    this.State = false;
                //    this.ErrorMessage = checkRight.Item2;
                //    return;
                //}

                this.ObjHeader.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.ObjHeader.STATUS = EB_Status.DaPheDuyet;

                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Update(this.ObjHeader);
                UnitOfWork.Repository<PREBHistoryRepo>().Create(new T_PR_EB_HISTORY()
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

        public void RejectPR(string code, string reason)
        {
            try
            {
                this.Get(code);

                //ConfigViettinBank: Xoá phân quyền dự án

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP108", this.ObjHeader.PROJECT_CODE);
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
                UnitOfWork.Repository<PREBHistoryRepo>().Create(new T_PR_EB_HISTORY()
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

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP107", this.ObjHeader.PROJECT_CODE);
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
                UnitOfWork.Repository<PREBHistoryRepo>().Create(new T_PR_EB_HISTORY()
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
                //this.ObjHeader.ListDetail = this.ObjHeader.ListDetail.OrderBy(x => x.ITEM_NUMBER).ToList();
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

       public IList<T_PR_EB_DETAIL> GetJobDetailsByHeaderCode(string headerCode)
        {
            return UnitOfWork.Repository<PREBDetailRepo>().Queryable().Where(x => x.HEADER_CODE == headerCode).ToList();
        }
    }
}
