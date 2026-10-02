using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REQUEST : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string DMS { get; set; }
        public virtual string MA_SO_THUE { get; set; }
        public virtual string TEN_NGAN { get; set; }
        public virtual string TEN_DAI { get; set; }
        public virtual string DIA_CHI { get; set; }
        public virtual string DIA_CHI_VP { get; set; }
        public virtual string TRU_SO_VP { get; set; }
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
        public virtual string FILE_HO_SO_ID { get; set; }
        public virtual string LOGO_PATH { get; set; }
        public virtual string COMMENT { get; set; }
        public virtual string STATUS { get; set; }
        public virtual string LINH_VUC_CHINH { get; set; }
        public virtual string LINH_VUC_CHINH_TEXT { get; set; }
        public virtual string ACTIVE_AREA { get; set; }
        public virtual string SO_LUONG_DU_AN { get; set; }
        public virtual string SO_LUONG_DU_AN_TEXT { get; set; }
        public virtual string TUY_CHON { get; set; }
        public virtual string TUY_CHON_TEXT { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string NOTES { get; set; }
        public virtual string NGUOI_NHAN_THONG_BAO { get; set; }
        public virtual bool SEARCH_IS_ONLY { get; set; }

        public virtual ISet<T_EB_REQUEST_SCOPE> ListScope { get; set; }
        public virtual ISet<T_EB_REQUEST_REVENUE> ListRevenue { get; set; }
        public virtual T_MD_COUNTRY Country { get; set; }
        public virtual T_MD_NUMBER_OF_PROJECT NumberOfProject { get; set; }
        public virtual T_MD_OPTION Option { get; set; }
        public virtual T_CM_FILE_UPLOAD FileImport { get; set; }
        public virtual List<T_MD_ACTIVE_AREA> ListActiveArea { get; set; }
        public virtual List<T_EB_BID> ListBid { get; set; }
        public T_EB_REQUEST()
        {
            ListScope = new HashSet<T_EB_REQUEST_SCOPE>(new List<T_EB_REQUEST_SCOPE>());
            ListRevenue = new HashSet<T_EB_REQUEST_REVENUE>(new List<T_EB_REQUEST_REVENUE>());
            Country = new T_MD_COUNTRY();
            NumberOfProject = new T_MD_NUMBER_OF_PROJECT();
            Option = new T_MD_OPTION();
            FileImport = new T_CM_FILE_UPLOAD();
            ListActiveArea = new List<T_MD_ACTIVE_AREA>();
            ListBid = new List<T_EB_BID>();
        }
    }
}
