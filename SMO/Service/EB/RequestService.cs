using SMO.Core.Entities;
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
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class RequestService : GenericService<T_EB_REQUEST, RequestRepo>
    {
        public List<T_EB_REQUEST_REVENUE> ObjListRevenue { get; set; }
        public T_MD_DICTIONARY ObjDictionary { get; set; }
        public List<NodeAddress> ObjListAddress { get; set; }
        public RequestService() : base()
        {
            ObjListRevenue = new List<T_EB_REQUEST_REVENUE>();
            ObjDictionary = new T_MD_DICTIONARY();
            ObjListAddress = new List<NodeAddress>();
        }

        public void Create(string lang, HttpRequestBase Request)
        {
            //Validate
            if (string.IsNullOrEmpty(this.ObjDetail.LINH_VUC_CHINH))
            {
                this.State = false;
                if (lang == "vi")
                {
                    this.ErrorMessage = "Chưa nhập Lĩnh vực chính!";
                }
                else
                {
                    this.ErrorMessage = "Please, select main area!";
                }
                return;
            }

            if (UnitOfWork.Repository<RequestRepo>().CheckExist(x => x.MA_SO_THUE == this.ObjDetail.MA_SO_THUE))
            {
                this.State = false;
                if (lang == "vi")
                {
                    this.ErrorMessage = "Hệ thống ghi nhận công ty của bạn đã đăng ký e-bidding.<br/> Để tiếp tục, vui lòng: Gửi email đến ccm@coteccons.vn để được hỗ trợ.";

                }
                else
                {
                    this.ErrorMessage = "Our system shows that your company has already registered for e-bidding.<br/> To continue, please send an email to ccm@coteccons.vn for support.";
                }
                return;
            }

            if (UnitOfWork.Repository<ContractorRepo>().CheckExist(x => x.MA_SO_THUE == this.ObjDetail.MA_SO_THUE))
            {
                this.State = false;
                if (lang == "vi")
                {
                    this.ErrorMessage = "Hệ thống ghi nhận công ty của bạn đã có tài khoản e-bidding.<br/> Để tiếp tục, vui lòng: <br/> -Nhấn \\\"Quên mật khẩu?\\\" để đặt lại tài khoản.<br/> - Hoặc gửi email đến ccm@coteccons.vn để được hỗ trợ.";

                }
                else
                {
                    this.ErrorMessage = "The system has recorded that your company already has an e-bidding account. <br/> To continue, please: <br/> - Click \"Forgot Password?\" to reset your account. <br/> - Or send an email to ccm@coteccons.vn for assistance.";

                }
                return;
            }

          
            var strFileHoSo = "fileHoSoNL";
            try
            {
                UnitOfWork.BeginTransaction();
                // Lưu file vào database
                var lstFileStream = new List<FILE_STREAM>();
                var code = Guid.NewGuid().ToString();
                var referenceCode = Guid.NewGuid().ToString();

                for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                {
                    var file = Request.Files[i];
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

                //Khởi tạo dữ liệu
                this.ObjDetail.FILE_HO_SO_ID = code;
                if (this.ObjListAddress.Count > 0)
                {
                    var lstAddress = this.ObjListAddress.Where(x => x.Is_Remove == false).Select(x => x.Address).ToList();
                    this.ObjDetail.DIA_CHI_VP = string.Join("|", lstAddress);
                }

                //Lưu doanh thu 3 năm gần nhất
                foreach (var item in this.ObjDetail.ListRevenue)
                {
                    item.PKID = Guid.NewGuid().ToString();
                    item.REQUEST_ID = this.ObjDetail.PKID;
                    item.CREATE_BY = "new_user";
                    UnitOfWork.Repository<RequestRevenueRepo>().Create(item);
                }

                //Lưu lĩnh vực chính
                if (!string.IsNullOrEmpty(this.ObjDetail.LINH_VUC_CHINH))
                {
                    foreach (var item in this.ObjDetail.LINH_VUC_CHINH.Split(','))
                    {

                        var scope = new T_EB_REQUEST_SCOPE()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            REQUEST_ID = this.ObjDetail.PKID,
                            SCOPE_CODE = item,
                            CREATE_BY = "new_user"
                        };
                        UnitOfWork.Repository<RequestScopeRepo>().Create(scope);
                    }
                }

                this.ObjDetail.STATUS = EB_Status.KhoiTao;
                base.Create();
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }


        }

        public void Reject(string id, string comment)
        {
            this.Get(id);
            this.ObjDetail.STATUS = EB_Status.TuChoi;
            this.ObjDetail.COMMENT = comment;
            this.Update();
            this.CreateEmail(comment);
        }

        public void CreateEmail(string comment)
        {
            try
            {
                string LY_DO = "{LY_DO}";
                string TEN_NHA_THAU_DAI = "{TEN_NHA_THAU_DAI}";
                string TEN_NHA_THAU_NGAN = "{TEN_NHA_THAU_NGAN}";

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

                var subjectTemplate = template.REJECT_REQUEST_ACCOUNT_SUBJECT;
                var bodyTemplate = template.REJECT_REQUEST_ACCOUNT_BODY;

                if (string.IsNullOrWhiteSpace(subjectTemplate) || string.IsNullOrWhiteSpace(bodyTemplate))
                {
                    return;
                }

                var contentSubject = subjectTemplate
                        .Replace(LY_DO, comment)
                        .Replace(TEN_NHA_THAU_DAI, this.ObjDetail.TEN_DAI)
                        .Replace(TEN_NHA_THAU_NGAN, this.ObjDetail.TEN_NGAN);

                var contentBody = bodyTemplate
                        .Replace(LY_DO, comment)
                        .Replace(TEN_NHA_THAU_DAI, this.ObjDetail.TEN_DAI)
                        .Replace(TEN_NHA_THAU_NGAN, this.ObjDetail.TEN_NGAN);

                serviceEmail.ObjDetail = new T_CM_EMAIL()
                {
                    PKID = Guid.NewGuid().ToString(),
                    EMAIL = this.ObjDetail.EMAIL,
                    SUBJECT = contentSubject,
                    CONTENTS = contentBody
                };
                serviceEmail.Create();
            }
            catch (Exception)
            {

            }
        }

        public static string CreatePath(string parentPath)
        {
            var path = "";
            try
            {
                var now = DateTime.Now;
                path = Path.Combine(parentPath, now.Year.ToString(), now.Month.ToString(), now.Day.ToString());
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return path;
        }

        public override void Get(object id, dynamic param = null)
        {
            try
            {
                this.ObjDetail = this.CurrentRepository.Get(id, param);
                this.ObjDetail.ListActiveArea = UnitOfWork.Repository<ActiveAreaRepo>().Queryable().Where(x => this.ObjDetail.ACTIVE_AREA.Split(',').Contains(x.CODE)).ToList();

                this.ObjDictionary = UnitOfWork.Repository<DictionaryRepo>().Queryable().FirstOrDefault(x => x.CODE == this.ObjDetail.LOAI_HINH_DOANH_NGHIEP && x.FK_DOMAIN == Domain.LOAI_HINH_DOANH_NGHIEP.ToString());
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }
    }
}
