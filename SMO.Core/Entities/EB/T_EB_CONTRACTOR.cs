using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    [Serializable]
    public partial class T_EB_CONTRACTOR : BaseEntity
    {
        public virtual string PKID { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập (required)", AllowEmptyStrings = false)]
        [RegularExpression(@"^\S*$", ErrorMessage = "Không được phép nhập dấu cách")]
        public virtual string USER_NAME { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập (required)", AllowEmptyStrings = false)]
        public virtual string MA_SO_THUE { get; set; }
        public virtual string DMS { get; set; }

        [Required(ErrorMessage = "Trường này bắt buộc nhập (required)", AllowEmptyStrings = false)]


        public virtual string TEN_NGAN { get; set; }

        public virtual string TEN_DAI { get; set; }

        public virtual string DIA_CHI { get; set; }
        public virtual string DIA_CHI_VP { get; set; }
        public virtual string LOAI_HINH_DOANH_NGHIEP { get; set; }
        public virtual string QUOC_GIA { get; set; }
        public virtual int SO_LUONG_NHAN_VIEN { get; set; }
        public virtual decimal VON_DIEU_LE { get; set; }
        public virtual string VON_DIEU_LE_UNIT { get; set; }

        public virtual string NGUOI_DAI_DIEN { get; set; }

        public virtual string WEBSITE { get; set; }
        public virtual string EMAIL { get; set; }
        public virtual string PHONE { get; set; }
        public virtual string FILE_GIOI_THIEU_ID { get; set; }
        public virtual string LOGO_PATH { get; set; }
        public virtual string LINH_VUC_CHINH { get; set; }
        public virtual string LINH_VUC_CHINH_TEXT { get; set; }
        public virtual string XEP_HANG { get; set; }
        public virtual decimal DIEM_DANH_GIA { get; set; }
        public virtual string REVIEW_FINAL_CODE { get; set; }
        public virtual DateTime? NGAY_DANH_GIA { get; set; }
        public virtual DateTime? REQUEST_DATE { get; set; }

        public virtual string GHI_CHU { get; set; }
        public virtual string ACTIVE_AREA { get; set; }
        public virtual string SO_LUONG_DU_AN { get; set; }
        public virtual string SO_LUONG_DU_AN_TEXT { get; set; }
        public virtual string TUY_CHON { get; set; }
        public virtual string TUY_CHON_TEXT { get; set; }
        public virtual string NGUOI_NHAN_THONG_BAO { get; set; }
        public virtual T_AD_USER User { get; set; }

        public virtual T_CM_FILE_UPLOAD FileLogo { get; set; }
        public virtual T_CM_FILE_UPLOAD FileGioiThieu { get; set; }
        public virtual T_MD_COUNTRY Country { get; set; }
        public virtual T_MD_NUMBER_OF_PROJECT NumberOfProject { get; set; }
        public virtual T_MD_OPTION Option { get; set; }
        public virtual ISet<T_EB_CONTRACTOR_MARKET> ListMarket { get; set; }
        public virtual ISet<T_EB_CONTRACTOR_PRODUCT> ListProduct { get; set; }
        public virtual ISet<T_EB_CONTRACTOR_SCOPE> ListScope { get; set; }
        public virtual ISet<T_EB_CONTRACTOR_CONTACT> ListContact { get; set; }
        public virtual IList<T_EB_CONTRACTOR_REVENUE> ListRevenue { get; set; }

        public virtual bool IS_EXPORT_EXCEL { get; set; }
        public virtual string REVIEW_FINAL_ID { get; set; }

        // Trường tìm kiếm
        public virtual DateTime? FROM_DATE { get; set; }
        public virtual DateTime? TO_DATE { get; set; }
        public virtual string PRODUCT_CODE { get; set; }
        public virtual bool IS_NOT_PAGEING { get; set; }

        public T_EB_CONTRACTOR()
        {
            ListMarket = new HashSet<T_EB_CONTRACTOR_MARKET>(new List<T_EB_CONTRACTOR_MARKET>());
            ListProduct = new HashSet<T_EB_CONTRACTOR_PRODUCT>(new List<T_EB_CONTRACTOR_PRODUCT>());
            ListScope = new HashSet<T_EB_CONTRACTOR_SCOPE>(new List<T_EB_CONTRACTOR_SCOPE>());
            ListContact = new HashSet<T_EB_CONTRACTOR_CONTACT>(new List<T_EB_CONTRACTOR_CONTACT>());
            ListRevenue = new List<T_EB_CONTRACTOR_REVENUE>();
        }
    }
}
