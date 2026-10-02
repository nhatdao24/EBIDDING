using NHibernate.Criterion;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Service.AD;
using SMO.Service.CF;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SMO.Service.EB
{
    public class ContractorService : GenericService<T_EB_CONTRACTOR, ContractorRepo>
    {
        public string BidId { get; set; }
        public string ReviewId { get; set; }
        public string RequestId { get; set; }
        public string VendorId { get; set; }
        public List<T_EB_CONTRACTOR_LOCATION> ObjListContractorLocation { get; set; }
        public List<T_EB_CONTRACTOR_REVENUE> ObjListContractorRevenue { get; set; }
        public ContractorService() : base()
        {
            ObjListContractorLocation = new List<T_EB_CONTRACTOR_LOCATION>();
            ObjListContractorRevenue = new List<T_EB_CONTRACTOR_REVENUE>();
        }

        public void ContructorContractor()
        {
            if (!string.IsNullOrWhiteSpace(RequestId))
            {
                var request = UnitOfWork.Repository<RequestRepo>().Get(this.RequestId);
                if (request != null)
                {
                    this.ObjDetail.USER_NAME = request.MA_SO_THUE;
                    this.ObjDetail.MA_SO_THUE = request.MA_SO_THUE;
                    this.ObjDetail.DMS = request.DMS;
                    this.ObjDetail.TEN_NGAN = request.TEN_NGAN;
                    this.ObjDetail.TEN_DAI = request.TEN_DAI != null ? request.TEN_DAI.ToUpper() : "";
                    this.ObjDetail.DIA_CHI = request.DIA_CHI;
                    this.ObjDetail.LOAI_HINH_DOANH_NGHIEP = request.LOAI_HINH_DOANH_NGHIEP;
                    this.ObjDetail.QUOC_GIA = request.QUOC_GIA;
                    this.ObjDetail.SO_LUONG_NHAN_VIEN = request.SO_LUONG_NHAN_VIEN;
                    this.ObjDetail.VON_DIEU_LE = request.VON_DIEU_LE;
                    this.ObjDetail.VON_DIEU_LE_UNIT = request.VON_DIEU_LE_UNIT;
                    this.ObjDetail.NGUOI_DAI_DIEN = request.NGUOI_DAI_DIEN != null ? request.NGUOI_DAI_DIEN.ToUpper() : "";
                    this.ObjDetail.WEBSITE = request.WEBSITE;
                    this.ObjDetail.EMAIL = request.EMAIL;
                    this.ObjDetail.PHONE = request.PHONE;
                    this.ObjDetail.LINH_VUC_CHINH = request.LINH_VUC_CHINH;
                    this.ObjDetail.LINH_VUC_CHINH_TEXT = request.LINH_VUC_CHINH_TEXT;
                    this.ObjDetail.SO_LUONG_DU_AN = request.SO_LUONG_DU_AN;
                    this.ObjDetail.SO_LUONG_DU_AN_TEXT = request.SO_LUONG_DU_AN_TEXT;
                    this.ObjDetail.TUY_CHON = request.TUY_CHON;
                    this.ObjDetail.TUY_CHON_TEXT = request.TUY_CHON_TEXT;
                    this.ObjDetail.ACTIVE_AREA = request.ACTIVE_AREA;
                    this.ObjDetail.GHI_CHU = request.NOTES;
                    this.ObjDetail.FILE_GIOI_THIEU_ID = request.FILE_HO_SO_ID;
                    this.ObjDetail.NGUOI_NHAN_THONG_BAO = request.NGUOI_NHAN_THONG_BAO != null ? request.NGUOI_NHAN_THONG_BAO.ToUpper() : "";
                    if (!string.IsNullOrEmpty(request.DIA_CHI_VP))
                    {
                        foreach (var item in request.DIA_CHI_VP.Split('|'))
                        {
                            var address = new T_EB_CONTRACTOR_LOCATION();
                            address.PKID = Guid.NewGuid().ToString();
                            address.ADDRESS = item;
                            this.ObjListContractorLocation.Add(address);
                        }
                    }

                    foreach (var item in request.ListRevenue)
                    {
                        var revenue = new T_EB_CONTRACTOR_REVENUE();
                        revenue.PKID = Guid.NewGuid().ToString();
                        revenue.YEAR = item.YEAR;
                        revenue.VALUE = item.VALUE;
                        revenue.UNIT_CODE = item.UNIT_CODE;
                        this.ObjListContractorRevenue.Add(revenue);
                    }

                    this.ObjDetail.FileGioiThieu = request.FileImport;
                }
            }
            else if (!string.IsNullOrWhiteSpace(VendorId))
            {
                var vendor = UnitOfWork.Repository<VendorRepo>().Get(this.VendorId);
                if (vendor != null)
                {
                    this.ObjDetail.USER_NAME = vendor.MA_SO_THUE;
                    this.ObjDetail.MA_SO_THUE = vendor.MA_SO_THUE;
                    this.ObjDetail.TEN_NGAN = vendor.SHORT_NAME;
                    this.ObjDetail.TEN_DAI = vendor.LONG_NAME.ToUpper();
                    this.ObjDetail.DIA_CHI = vendor.DIA_CHI;
                    this.ObjDetail.NGUOI_DAI_DIEN = vendor.LIEN_HE;
                    this.ObjDetail.WEBSITE = vendor.WEBSITE;
                    this.ObjDetail.EMAIL = vendor.EMAIL;
                    this.ObjDetail.PHONE = vendor.SO_DIEN_THOAI;
                    this.ObjDetail.LINH_VUC_CHINH = vendor.LINH_VUC_CHINH;
                }
            }
        }

        public void CreateNotiEmail(string password)
        {
            try
            {
                string TEN_NHA_THAU_DAI = "{TEN_NHA_THAU_DAI}";
                string TEN_NHA_THAU_NGAN = "{TEN_NHA_THAU_NGAN}";
                string USER_NAME = "{USER_NAME}";
                string PASSWORD = "{PASSWORD}";

                if (string.IsNullOrWhiteSpace(this.ObjDetail.EMAIL))
                {
                    return;
                }

                var serviceTemplate = new ConfigTemplateNotifyService();
                serviceTemplate.GetAll();
                if (serviceTemplate.ObjList.Count() == 0)
                {
                    return;
                }

                var serviceConfig = new SystemConfigService();
                serviceConfig.GetConfig();

                var template = serviceTemplate.ObjList.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(this.RequestId) && (string.IsNullOrEmpty(template.ACCOUNT_SUBJECT) || string.IsNullOrEmpty(template.ACCOUNT_BODY)))
                {
                    return;
                }
                else if (!string.IsNullOrWhiteSpace(this.RequestId) && (string.IsNullOrEmpty(template.REQUEST_ACCOUNT_SUBJECT) || string.IsNullOrEmpty(template.REQUEST_ACCOUNT_BODY)))
                {
                    return;
                }

                var serviceEmail = new EmailNotifyService();

                var contentSubject = template.ACCOUNT_SUBJECT
                    .Replace(TEN_NHA_THAU_DAI, this.ObjDetail.TEN_DAI)
                    .Replace(TEN_NHA_THAU_NGAN, this.ObjDetail.TEN_NGAN)
                    .Replace(USER_NAME, this.ObjDetail.USER_NAME)
                    .Replace(PASSWORD, password)
                    ;

                var contentBody = template.ACCOUNT_BODY
                   .Replace(TEN_NHA_THAU_DAI, this.ObjDetail.TEN_DAI)
                   .Replace(TEN_NHA_THAU_NGAN, this.ObjDetail.TEN_NGAN)
                   .Replace(USER_NAME, this.ObjDetail.USER_NAME)
                   .Replace(PASSWORD, password)
                   ;

                if (!string.IsNullOrWhiteSpace(this.RequestId))
                {
                    contentSubject = template.REQUEST_ACCOUNT_SUBJECT
                       .Replace(TEN_NHA_THAU_DAI, this.ObjDetail.TEN_DAI)
                       .Replace(TEN_NHA_THAU_NGAN, this.ObjDetail.TEN_NGAN)
                       .Replace(USER_NAME, this.ObjDetail.USER_NAME)
                       .Replace(PASSWORD, password)
                   ;

                    contentBody = template.REQUEST_ACCOUNT_BODY
                       .Replace(TEN_NHA_THAU_DAI, this.ObjDetail.TEN_DAI)
                       .Replace(TEN_NHA_THAU_NGAN, this.ObjDetail.TEN_NGAN)
                       .Replace(USER_NAME, this.ObjDetail.USER_NAME)
                       .Replace(PASSWORD, password)
                       ;
                }

                var lstContractorEmail = this.ObjDetail.EMAIL.Split(';').ToList();
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
            catch (Exception)
            {
            }

        }


        public async Task CreateUserContractor(HttpRequestBase Request)
        {
            var strFileLogo = "fileLogo";
            var strFileGioiThieu = "fileGioiThieu";
            try
            {
                // Kiểm tra thông tin
                if (UnitOfWork.Repository<UserRepo>().CheckExist(x => x.USER_NAME == this.ObjDetail.USER_NAME))
                {
                    this.State = false;
                    this.MesseageCode = "1104";
                    return;
                }

                if (UnitOfWork.Repository<ContractorRepo>().CheckExist(x => x.MA_SO_THUE == this.ObjDetail.MA_SO_THUE))
                {
                    this.State = false;
                    this.MesseageCode = "1105";
                    return;
                }

                // Lưu file logo
                var lstFileLogo = new List<FILE_STREAM>();
                for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                {
                    if (Request.Files.GetKey(i) == strFileLogo)
                    {
                        var logoFile = Request.Files[i];
                        var logoCode = Guid.NewGuid().ToString();
                        var logoReferenceCode = Guid.NewGuid().ToString();

                        lstFileLogo.Add(new FILE_STREAM()
                        {
                            PKID = logoCode,
                            FILE_OLD_NAME = logoFile.FileName,
                            REFERENCE_ID = logoReferenceCode,
                            FILE_EXT = Path.GetExtension(logoFile.FileName),
                            FILE_SIZE = logoFile.ContentLength,
                            FILESTREAM = Request.Files[i]
                        });
                        break;
                    }
                }
                await FileStreamService.InsertFile(lstFileLogo);

                // Lưu file giới thiệu vào database
                var lstFileStream = new List<FILE_STREAM>();
                for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                {
                    if (Request.Files.GetKey(i) == strFileGioiThieu)
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
                        break;
                    }
                }
                await FileStreamService.InsertFile(lstFileStream);

                // Tạo dữ liệu
                this.ObjDetail.PKID = Guid.NewGuid().ToString();
                if (lstFileLogo.Count > 0)
                {
                    this.ObjDetail.LOGO_PATH = lstFileLogo[0].REFERENCE_ID;
                }
                if (lstFileStream.Count > 0)
                {
                    this.ObjDetail.FILE_GIOI_THIEU_ID = lstFileStream[0].REFERENCE_ID;
                }

                // Kiểm tra có được tạo từ request không
                var request = new T_EB_REQUEST();
                if (!string.IsNullOrWhiteSpace(this.RequestId))
                {
                    request = UnitOfWork.Repository<RequestRepo>().Get(this.RequestId);
                }

                // Kiểm tra có được tạo từ vendor md không
                var vendor = new T_MD_VENDOR();
                if (!string.IsNullOrWhiteSpace(this.VendorId))
                {
                    vendor = UnitOfWork.Repository<VendorRepo>().Get(this.VendorId);
                }

                var org = UnitOfWork.GetSession().Query<T_AD_ORGANIZE>().FirstOrDefault(x => x.COMPANY_CODE == "CTD" && x.TYPE == "CP");

                UnitOfWork.BeginTransaction();

                if (!string.IsNullOrEmpty(request.PKID))
                {
                    request.STATUS = EB_Status.DaPheDuyet;
                    UnitOfWork.Repository<RequestRepo>().Update(request);
                }

                if (!string.IsNullOrEmpty(vendor.CODE))
                {
                    vendor.USER_NAME = this.ObjDetail.USER_NAME;
                    vendor.STATUS = true;
                    UnitOfWork.Repository<VendorRepo>().Update(vendor);
                }

                var password = this.ObjDetail.USER_NAME + "@123";

                UnitOfWork.Repository<UserRepo>().Create(new T_AD_USER()
                {
                    USER_NAME = this.ObjDetail.USER_NAME,
                    FULL_NAME = this.ObjDetail.TEN_NGAN.ToUpper(),
                    PASSWORD = UtilsCore.EncryptStringMD5(this.ObjDetail.USER_NAME + "@123"),
                    ACTIVE = true,
                    CONTRACTOR_ID = this.ObjDetail.PKID,
                    USER_TYPE = UserType.NhaThau,
                    CREATE_BY = ProfileUtilities.User.USER_NAME,
                    IS_MODIFY_RIGHT = true
                });


                // Tạo quyền mặc định
                UnitOfWork.Repository<UserRightRepo>().Create(new T_AD_USER_RIGHT()
                {
                    USER_NAME = this.ObjDetail.USER_NAME,
                    FK_RIGHT = "R40",
                    ORGANIZE_ID = org.PKID,
                    IS_ADD = true
                });

                UnitOfWork.Repository<UserRightRepo>().Create(new T_AD_USER_RIGHT()
                {
                    USER_NAME = this.ObjDetail.USER_NAME,
                    FK_RIGHT = "R450",
                    ORGANIZE_ID = org.PKID,
                    IS_ADD = true
                });

                UnitOfWork.Repository<UserRightRepo>().Create(new T_AD_USER_RIGHT()
                {
                    USER_NAME = this.ObjDetail.USER_NAME,
                    FK_RIGHT = "R451",
                    ORGANIZE_ID = org.PKID,
                    IS_ADD = true
                });

                UnitOfWork.Repository<UserRightRepo>().Create(new T_AD_USER_RIGHT()
                {
                    USER_NAME = this.ObjDetail.USER_NAME,
                    FK_RIGHT = "R452",
                    ORGANIZE_ID = org.PKID,
                    IS_ADD = true
                });

                UnitOfWork.Repository<UserRightRepo>().Create(new T_AD_USER_RIGHT()
                {
                    USER_NAME = this.ObjDetail.USER_NAME,
                    FK_RIGHT = "R453",
                    ORGANIZE_ID = org.PKID,
                    IS_ADD = true
                });

                UnitOfWork.Repository<UserRightRepo>().Create(new T_AD_USER_RIGHT()
                {
                    USER_NAME = this.ObjDetail.USER_NAME,
                    FK_RIGHT = "R454",
                    ORGANIZE_ID = org.PKID,
                    IS_ADD = true
                });

                UnitOfWork.Repository<UserRightRepo>().Create(new T_AD_USER_RIGHT()
                {
                    USER_NAME = this.ObjDetail.USER_NAME,
                    FK_RIGHT = "R702",
                    ORGANIZE_ID = org.PKID,
                    IS_ADD = true
                });

                UnitOfWork.Repository<UserRightRepo>().Create(new T_AD_USER_RIGHT()
                {
                    USER_NAME = this.ObjDetail.USER_NAME,
                    FK_RIGHT = "R703",
                    ORGANIZE_ID = org.PKID,
                    IS_ADD = true
                });

                if (!string.IsNullOrEmpty(request.PKID))
                {
                    this.ObjDetail.REQUEST_DATE = request.CREATE_DATE;
                    this.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;

                    //Lưu Contractor Scope
                    //var lstScopeCode = UnitOfWork.Repository<RequestScopeRepo>().Queryable().Where(x => x.REQUEST_ID == request.PKID).Select(x => x.SCOPE_CODE).ToList();
                    //if (lstScopeCode.Count > 0)
                    //{
                    //    foreach (var item in lstScopeCode)
                    //    {
                    //        UnitOfWork.Repository<ContractorScopeRepo>().Create(new T_EB_CONTRACTOR_SCOPE()
                    //        {
                    //            PKID = Guid.NewGuid().ToString(),
                    //            CONTRACTOR_ID = this.ObjDetail.PKID,
                    //            SCOPE_CODE = item,
                    //            CREATE_BY = ProfileUtilities.User.USER_NAME
                    //        });
                    //    }
                    //}
                }

                if (!string.IsNullOrEmpty(this.ObjDetail.LINH_VUC_CHINH))
                {
                    foreach (var item in this.ObjDetail.LINH_VUC_CHINH.Split(','))
                    {
                        UnitOfWork.Repository<ContractorScopeRepo>().Create(new T_EB_CONTRACTOR_SCOPE()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            CONTRACTOR_ID = this.ObjDetail.PKID,
                            SCOPE_CODE = item,
                            CREATE_BY = ProfileUtilities.User.USER_NAME
                        });
                    }
                    this.ObjDetail.LINH_VUC_CHINH = "";
                }

                foreach (var item in this.ObjListContractorLocation)
                {
                    item.PKID = Guid.NewGuid().ToString();
                    item.CONTRACTOR_ID = this.ObjDetail.PKID;
                    item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                }

                foreach (var item in this.ObjListContractorRevenue)
                {
                    item.PKID = Guid.NewGuid().ToString();
                    item.CONTRACTOR_ID = this.ObjDetail.PKID;
                    item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                }

                UnitOfWork.Repository<ContractorLocationRepo>().Create(this.ObjListContractorLocation);
                UnitOfWork.Repository<ContratorRevenueRepo>().Create(this.ObjListContractorRevenue);
                UnitOfWork.Repository<ContractorRepo>().Create(this.ObjDetail);
                UnitOfWork.Commit();

                this.CreateNotiEmail(password);
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public async Task Update(HttpRequestBase Request)
        {
            var strFileLogo = "fileLogo";
            var strFileGioiThieu = "fileGioiThieu";
            try
            {
                // Lưu file logo
                var lstFileLogo = new List<FILE_STREAM>();
                for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                {
                    if (Request.Files.GetKey(i) == strFileLogo)
                    {
                        var logoFile = Request.Files[i];
                        var logoCode = Guid.NewGuid().ToString();
                        var logoReferenceCode = Guid.NewGuid().ToString();
                        lstFileLogo.Add(new FILE_STREAM()
                        {
                            PKID = logoCode,
                            FILE_OLD_NAME = logoFile.FileName,
                            REFERENCE_ID = logoReferenceCode,
                            FILE_EXT = Path.GetExtension(logoFile.FileName),
                            FILE_SIZE = logoFile.ContentLength,
                            FILESTREAM = Request.Files[i]
                        });
                        break;
                    }
                }
                FileStreamService.InsertFile(lstFileLogo);

                // Lưu file giới thiệu vào database
                var lstFileStream = new List<FILE_STREAM>();
                for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                {
                    if (Request.Files.GetKey(i) == strFileGioiThieu)
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
                        break;
                    }
                }
               FileStreamService.InsertFile(lstFileStream);

                if (lstFileLogo.Count > 0)
                {
                    this.ObjDetail.LOGO_PATH = lstFileLogo[0].REFERENCE_ID;
                }
                if (lstFileStream.Count > 0)
                {
                    this.ObjDetail.FILE_GIOI_THIEU_ID = lstFileStream[0].REFERENCE_ID;
                }
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                //UnitOfWork.Clear();
                UnitOfWork.BeginTransaction();
                UnitOfWork.Repository<ContractorRepo>().Update(this.ObjDetail);
                foreach (var item in this.ObjDetail.ListRevenue)
                {
                    item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    UnitOfWork.Repository<ContratorRevenueRepo>().Update(item);
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

        public void SearchForAddContractorBid()
        {
            var queryBidContractor = UnitOfWork.Repository<BidContractorRepo>().Queryable();
            queryBidContractor = queryBidContractor.Where(x => x.BID_ID == this.BidId);
            var lstContractorOifBid = queryBidContractor.ToList().Select(x => x.CONTRACTOR_ID);


            var query = UnitOfWork.Repository<ContractorRepo>().Queryable();
            query = query.Where(x => !lstContractorOifBid.Contains(x.PKID));
            if (!string.IsNullOrWhiteSpace(this.ObjDetail.TEN_NGAN))
            {
                query = query.Where(
                    x => x.TEN_NGAN.Contains(this.ObjDetail.TEN_NGAN) ||
                    x.TEN_DAI.Contains(this.ObjDetail.TEN_NGAN) ||
                    x.MA_SO_THUE.Contains(this.ObjDetail.MA_SO_THUE));
            }

            if (!string.IsNullOrWhiteSpace(this.ObjDetail.EMAIL))
            {
                query = query.Where(
                    x => x.EMAIL.Contains(this.ObjDetail.EMAIL));
            }

            //if (!string.IsNullOrWhiteSpace(this.ObjDetail.LINH_VUC_CHINH))
            //{
            //    query = query.Where(
            //        x => x.LINH_VUC_CHINH.Contains(this.ObjDetail.LINH_VUC_CHINH));
            //}

            if (!string.IsNullOrWhiteSpace(this.ObjDetail.XEP_HANG))
            {
                query = query.Where(
                    x => x.XEP_HANG.Contains(this.ObjDetail.XEP_HANG));
            }

            if (!string.IsNullOrWhiteSpace(this.ObjDetail.QUOC_GIA))
            {
                var queryMarket = UnitOfWork.Repository<ContractorMarketRepo>().Queryable();
                queryMarket = queryMarket.Where(x => x.COUNTRY_CODE == this.ObjDetail.QUOC_GIA);

                var lstContractorMarket = queryMarket.Select(x => x.CONTRACTOR_ID).Distinct().ToList();

                query = query.Where(x => lstContractorMarket.Contains(x.PKID));
            }

            if (!string.IsNullOrWhiteSpace(this.ObjDetail.LINH_VUC_CHINH))
            {
                var queryScope = UnitOfWork.Repository<ContractorScopeRepo>().Queryable();
                queryScope = queryScope.Where(x => x.SCOPE_CODE == this.ObjDetail.LINH_VUC_CHINH);
                var lstContractorScope = queryScope.Select(x => x.CONTRACTOR_ID).Distinct().ToList();
                query = query.Where(x => lstContractorScope.Contains(x.PKID));
            }
            this.ObjList = query.ToList();
        }

        public void SearchForAddContractorReview()
        {
            var queryReviewContractor = UnitOfWork.Repository<ReviewContractorRepo>().Queryable();
            queryReviewContractor = queryReviewContractor.Where(x => x.REVIEW_ID == this.ReviewId);
            var lstContractorOfReview = queryReviewContractor.ToList().Select(x => x.CONTRACTOR_ID);


            var query = UnitOfWork.Repository<ContractorRepo>().Queryable();
            query = query.Where(x => !lstContractorOfReview.Contains(x.PKID));
            if (!string.IsNullOrWhiteSpace(this.ObjDetail.TEN_NGAN))
            {
                query = query.Where(
                    x => x.TEN_NGAN.Contains(this.ObjDetail.TEN_NGAN) ||
                    x.TEN_DAI.Contains(this.ObjDetail.TEN_NGAN) ||
                    x.MA_SO_THUE.Contains(this.ObjDetail.MA_SO_THUE));
            }

            if (!string.IsNullOrWhiteSpace(this.ObjDetail.EMAIL))
            {
                query = query.Where(
                    x => x.EMAIL.Contains(this.ObjDetail.EMAIL));
            }

            //if (!string.IsNullOrWhiteSpace(this.ObjDetail.LINH_VUC_CHINH))
            //{
            //    query = query.Where(
            //        x => x.LINH_VUC_CHINH.Contains(this.ObjDetail.LINH_VUC_CHINH));
            //}

            if (!string.IsNullOrWhiteSpace(this.ObjDetail.XEP_HANG))
            {
                query = query.Where(
                    x => x.XEP_HANG.Contains(this.ObjDetail.XEP_HANG));
            }

            if (!string.IsNullOrWhiteSpace(this.ObjDetail.QUOC_GIA))
            {
                var queryMarket = UnitOfWork.Repository<ContractorMarketRepo>().Queryable();
                queryMarket = queryMarket.Where(x => x.COUNTRY_CODE == this.ObjDetail.QUOC_GIA);

                var lstContractorMarket = queryMarket.Select(x => x.CONTRACTOR_ID).Distinct().ToList();

                query = query.Where(x => lstContractorMarket.Contains(x.PKID));
            }

            if (!string.IsNullOrWhiteSpace(this.ObjDetail.LINH_VUC_CHINH))
            {
                var queryScope = UnitOfWork.Repository<ContractorScopeRepo>().Queryable();
                queryScope = queryScope.Where(x => x.SCOPE_CODE == this.ObjDetail.LINH_VUC_CHINH);
                var lstContractorScope = queryScope.Select(x => x.CONTRACTOR_ID).Distinct().ToList();
                query = query.Where(x => lstContractorScope.Contains(x.PKID));
            }
            this.ObjList = query.ToList();
        }

        public void AddContractorToBid(string bidId, string lstContractorID)
        {
            try
            {
                UnitOfWork.BeginTransaction();

                foreach (var contractor in lstContractorID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    var item = new T_EB_BID_CONTRACTOR()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        BID_ID = bidId,
                        CONTRACTOR_ID = contractor,
                        STATUS = InviteStatus.Create,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    };

                    UnitOfWork.Repository<BidContractorRepo>().Create(item);
                }
                UnitOfWork.Commit();

                var lstContractor = new List<T_EB_CONTRACTOR>();
                foreach (var contractorID in lstContractorID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    var find = this.CurrentRepository.Get(contractorID);
                    if (find != null)
                    {
                        lstContractor.Add(find);
                    }
                }

                var serviceBid = new BidService();
                serviceBid.Get(bidId);
                if (serviceBid.ObjDetail.GIAI_DOAN == Phase_Status.NopHoSo)
                {
                    serviceBid.CreateNotifyInviteContractor(lstContractor.Select(x => x.USER_NAME).ToList());
                    serviceBid.CreateInviteEmailContractor(lstContractor);
                }
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public void AddContractorToReview(string reviewId, string lstContractorID)
        {
            try
            {
                UnitOfWork.BeginTransaction();

                foreach (var contractor in lstContractorID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    var item = new T_EB_REVIEW_CONTRACTOR()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        REVIEW_ID = reviewId,
                        CONTRACTOR_ID = contractor,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    };

                    UnitOfWork.Repository<ReviewContractorRepo>().Create(item);

                    var reviewContractorDetail = new T_EB_REVIEW_CONTRACTOR_DETAIL()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        HEADER_ID = item.PKID,
                        INFO_BID = "Công việc dự án",
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    };
                    this.UnitOfWork.Repository<ReviewContractorDetailRepo>().Create(reviewContractorDetail);
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

        public void ExportExcel(ref MemoryStream outFileStream)
        {
            this.ObjDetail.IS_NOT_PAGEING = true;
            this.Search();
            var lstMdContractor = this.UnitOfWork.Repository<VendorRepo>().GetAll();
            var lstActiveArea = this.UnitOfWork.Repository<ActiveAreaRepo>().GetAll();
            var lstRate = this.UnitOfWork.Repository<MdRatingRepo>().GetAll();
            try
            {
                //Ghi file Excel
                XSSFWorkbook wb = new XSSFWorkbook();
                ISheet sheet = wb.CreateSheet();

                //Tạo row
                var row0 = sheet.CreateRow(0);
                //Merge lại row đầu các cột
                row0.CreateCell(0); //Tạo ra cell trc khi merge
                CellRangeAddress cellMerge = new CellRangeAddress(0, 0, 0, 13);
                sheet.AddMergedRegion(cellMerge);
                row0.GetCell(0).SetCellValue("DANH SÁCH NHÀ THẦU");

                //Ghi tên cột ở row1
                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("STT");
                row1.CreateCell(1).SetCellValue("TÀI KHOẢN");
                row1.CreateCell(2).SetCellValue("TÊN CÔNG TY");
                row1.CreateCell(3).SetCellValue("NHÓM NTP");
                row1.CreateCell(4).SetCellValue("LĨNH VỰC KINH DOANH");
                row1.CreateCell(5).SetCellValue("ĐỊA CHỈ THEO ĐKKD");
                row1.CreateCell(6).SetCellValue("KHU VỰC HOẠT ĐỘNG");
                row1.CreateCell(7).SetCellValue("THÔNG TIN");
                row1.CreateCell(10).SetCellValue("THÔNG TIN ĐÁNH GIÁ NĂNG LỰC");
                row1.CreateCell(13).SetCellValue("NGƯỜI TẠO");
                row1.CreateCell(14).SetCellValue("ĐƠN VỊ");
                row1.CreateCell(15).SetCellValue("NGÀY YÊU CẦU");
                row1.CreateCell(16).SetCellValue("NGÀY TẠO");
                row1.CreateCell(17).SetCellValue("BLACK LIST");

                CellRangeAddress cellMerge1 = new CellRangeAddress(1, 2, 0, 0);
                sheet.AddMergedRegion(cellMerge1);
                CellRangeAddress cellMerge2 = new CellRangeAddress(1, 2, 1, 1);
                sheet.AddMergedRegion(cellMerge2);
                CellRangeAddress cellMerge3 = new CellRangeAddress(1, 2, 2, 2);
                sheet.AddMergedRegion(cellMerge3);
                CellRangeAddress cellMerge4 = new CellRangeAddress(1, 2, 3, 3);
                sheet.AddMergedRegion(cellMerge4);
                CellRangeAddress cellMerge5 = new CellRangeAddress(1, 2, 4, 4);
                sheet.AddMergedRegion(cellMerge5);
                CellRangeAddress cellMerge6 = new CellRangeAddress(1, 2, 5, 5);
                sheet.AddMergedRegion(cellMerge6);
                CellRangeAddress cellMerge7 = new CellRangeAddress(1, 2, 6, 6);
                sheet.AddMergedRegion(cellMerge7);
                CellRangeAddress cellMerge8 = new CellRangeAddress(1, 2, 13, 13);
                sheet.AddMergedRegion(cellMerge8);
                CellRangeAddress cellMerge9 = new CellRangeAddress(1, 2, 14, 14);
                sheet.AddMergedRegion(cellMerge9);
                CellRangeAddress cellMerge10 = new CellRangeAddress(1, 2, 15, 15);
                sheet.AddMergedRegion(cellMerge10);
                CellRangeAddress cellMerge11 = new CellRangeAddress(1, 2, 16, 16);
                sheet.AddMergedRegion(cellMerge11);
                CellRangeAddress cellMerge12 = new CellRangeAddress(1, 2, 17, 17);
                sheet.AddMergedRegion(cellMerge12);

                CellRangeAddress cellMerge13 = new CellRangeAddress(1, 1, 7, 9);
                sheet.AddMergedRegion(cellMerge13);
                CellRangeAddress cellMerge14 = new CellRangeAddress(1, 1, 10, 12);
                sheet.AddMergedRegion(cellMerge14);

                var row2 = sheet.CreateRow(2);
                row2.CreateCell(7).SetCellValue("HỌ & TÊN");
                row2.CreateCell(8).SetCellValue("SĐT");
                row2.CreateCell(9).SetCellValue("EMAIL");
                row2.CreateCell(10).SetCellValue("ĐIỂM ĐÁNH GIÁ");
                row2.CreateCell(11).SetCellValue("XẾP HẠNG NTP");
                row2.CreateCell(12).SetCellValue("NGÀY ĐÁNH GIÁ");

                //bắt đầu duyệt mảng và ghi tiếp tục
                int rowIndex = 3;
                int stt = 1;
                foreach (var item in this.ObjList)
                {
                    var vendor = lstMdContractor.FirstOrDefault(x => x.MA_SO_THUE == item.MA_SO_THUE);
                    var contact = item.ListContact.FirstOrDefault(x => x.IS_DEFAULT == true);
                    var rate = lstRate.FirstOrDefault(x => x.RATING == item.XEP_HANG);
                    //string Nhóm NTP
                    var strScopeType = string.Empty;
                    foreach (var scope in item.ListScope)
                    {
                        strScopeType += scope.Scope.SCOPE_TYPE.TEXT + " | ";
                    }

                    //string Lĩnh vực chính
                    var strScope = string.Empty;
                    foreach (var scope in item.ListScope)
                    {
                        strScope += scope.Scope.TEXT + " | ";
                    }
                    //string thị trường kinh doanh
                    var strMarket = string.Empty;
                    foreach (var market in item.ListMarket)
                    {
                        strMarket += market.Country.TEXT + " | ";
                    }
                    //string hàng hóa dịch vụ
                    var strProduct = string.Empty;
                    foreach (var product in item.ListProduct)
                    {
                        strProduct += product.Product.TEXT + " | ";
                    }
                    //Khu vực hoạt động
                    var strActiveArea = string.Empty;
                    if (!string.IsNullOrEmpty(item.ACTIVE_AREA))
                    {
                        var listActiveAreaID = item.ACTIVE_AREA.Split(',').ToList();
                        foreach (var activeArea in lstActiveArea.Where(x => listActiveAreaID.Contains(x.CODE)).ToList())
                        {
                            strActiveArea += activeArea.TEXT + " | ";
                        }
                    }
                    //string TÊN CÔN TY
                    var tenCty = "";
                    tenCty += item.TEN_NGAN;
                    tenCty += " | " + item.TEN_DAI;
                    tenCty += " | MST: " + item.MA_SO_THUE;
                    tenCty += " | Mã NCC: " + ((vendor != null) ? vendor.CODE : "");

                    //tạo row mới
                    var newRow = sheet.CreateRow(rowIndex);

                    //set giá trị
                    newRow.CreateCell(0).SetCellValue(stt);
                    newRow.CreateCell(1).SetCellValue(item.USER_NAME);
                    newRow.CreateCell(2).SetCellValue(tenCty);
                    newRow.CreateCell(3).SetCellValue(strScopeType);
                    newRow.CreateCell(4).SetCellValue(strScope);
                    newRow.CreateCell(5).SetCellValue(item.DIA_CHI);
                    newRow.CreateCell(6).SetCellValue(strActiveArea);
                    newRow.CreateCell(7).SetCellValue((contact != null) ? contact.FULL_NAME : item.NGUOI_NHAN_THONG_BAO);
                    newRow.CreateCell(8).SetCellValue((contact != null) ? contact.PHONE : item.PHONE);
                    newRow.CreateCell(9).SetCellValue((contact != null) ? contact.EMAIL : item.EMAIL);
                    newRow.CreateCell(10).SetCellValue(item.DIEM_DANH_GIA > 0 ? item.DIEM_DANH_GIA.ToStringVN() : "0");
                    newRow.CreateCell(11).SetCellValue(item.XEP_HANG);
                    newRow.CreateCell(12).SetCellValue(item.NGAY_DANH_GIA.HasValue ? item.NGAY_DANH_GIA.Value.ToString(Global.DateToStringFormat) : "");
                    newRow.CreateCell(13).SetCellValue(item.USER_CREATE != null ? (item.USER_CREATE.FULL_NAME + "(" + item.USER_CREATE.Organize.NAME + ")") : "");
                    newRow.CreateCell(14).SetCellValue((!string.IsNullOrEmpty(item.CREATE_BY) && item.USER_CREATE.Organize != null) ? item.USER_CREATE.Organize.NAME : "");
                    newRow.CreateCell(15).SetCellValue(item.REQUEST_DATE.HasValue ? item.REQUEST_DATE.Value.ToString(Global.DateToStringFormat) : "");
                    newRow.CreateCell(16).SetCellValue(item.CREATE_DATE.HasValue ? item.CREATE_DATE.Value.ToString(Global.DateToStringFormat) : "");
                    newRow.CreateCell(17).SetCellValue((rate != null && rate.IS_BLACK_LIST) ? "x" : "");

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
    }
}
