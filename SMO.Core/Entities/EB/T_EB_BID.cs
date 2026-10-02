using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID : BaseEntity
    {
        public virtual string PKID { get; set; }
        //ConfigViettinBank: Xóa require để tạo gói thầu trực tiếp
        //[Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string PARENT_BID { get; set; }
        public virtual string COMPANY_CODE { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa 500 kí tự")]
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string TEN_GOI_THAU { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa 500 kí tự")]

        public virtual string TEN_GOI_THAU_KD { get; set; }
        public virtual string TEN_GOI_THAU_EN { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual DateTime NGAY_MO_THAU { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual DateTime NGAY_DONG_THAU { get; set; }
        public virtual DateTime? NGAY_KT_CHAM_THAU { get; set; }
        public virtual string MO_TA { get; set; }
        public virtual string MO_TA_KD { get; set; }
        public virtual string MO_TA_EN { get; set; }
        public virtual string REASON { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string TYPE { get; set; }
        public virtual string REQUIRED_CODE { get; set; }
        public virtual string STATUS { get; set; }
        public virtual string GIAI_DOAN { get; set; }
        public virtual bool IS_PUBLIC { get; set; }
        public virtual bool IS_BROADCAST_MAIL { get; set; }
        public virtual bool IS_MULTIL_ROUND { get; set; }
        public virtual int ROUND { get; set; }
        public virtual string COMMENT_ID { get; set; }
        public virtual string COMMENT { get; set; }
        [Required(ErrorMessage = "Hãy chọn người phê duyệt", AllowEmptyStrings = false)]
        public virtual string USER_APPROVE { get; set; }
        public virtual string USER_MANAGE { get; set; }
        public virtual bool IS_NEW_ROUND { get; set; }
        //ConfigViettinBank: Xóa require để tạo gói thầu trực tiếp
        //[Required(ErrorMessage = "Hãy chọn thông tin dự án", AllowEmptyStrings = false)]
        public virtual string PROJECT_CODE { get; set; }
        public virtual string PACKAGE_CODE { get; set; }
        
        public virtual DateTime? CLOSE_DATE { get; set; }

        public virtual string SCOPE_CODE { get; set; }
        public virtual string ContractorID { get; set; }
        public virtual string SCOPE_TYPE_CODE { get; set; }

        public virtual string TOTAL_PRICE { get; set; }

        public virtual string BID_TYPE { get; set; }


        public virtual T_AD_USER UserSent { get; set; }
        public virtual T_AD_USER UserManage { get; set; }

        public virtual List<string> ListFilterProject { get; set; }
        public virtual ISet<T_EB_BID_FILE> ListFile { get; set; }
        public virtual ISet<T_EB_BID_NEW> ListNew { get; set; }
        public virtual ISet<T_EB_BID_HISTORY> ListHistory { get; set; }
        public virtual IList<T_EB_BID_CONTRACTOR> ListContractor { get; set; }
        public virtual T_MD_BID_TYPE BidType { get; set; }
        public virtual T_MD_PROJECT Project { get; set; }

        public virtual T_MD_PACKAGE Package { get; set; }
        public virtual T_MD_SCOPE Scope { get; set; }
        public virtual T_MD_SCOPE_TYPE ScopeType { get; set; }

        public virtual T_AD_USER UserApprove { get; set; }
        public virtual string ListFileDelete { get; set; }

        public virtual bool IS_EXPORT_EXCEL { get; set; }
        public virtual bool CHECK_BID_SPEC { get; set; }

        //Trường phục vụ tìm kiếm
        public virtual DateTime? FROM_DATE { get; set; }
        public virtual DateTime? TO_DATE { get; set; }
        public virtual DateTime? FROM_DATE_NGAY_MO_THAU { get; set; }
        public virtual DateTime? TO_DATE_NGAY_MO_THAU { get; set; }
        public virtual DateTime? FROM_DATE_NGAY_DONG_THAU { get; set; }
        public virtual DateTime? TO_DATE_NGAY_DONG_THAU { get; set; }
        public virtual string SEARCH_TRANG_THAI { get; set; }
        public virtual bool IS_NOT_PAGEING { get; set; }
        public T_EB_BID()
        {
            ListFile = new HashSet<T_EB_BID_FILE>(new List<T_EB_BID_FILE>());
            ListNew = new HashSet<T_EB_BID_NEW>(new List<T_EB_BID_NEW>());
            ListContractor = new List<T_EB_BID_CONTRACTOR>();
            ListHistory = new HashSet<T_EB_BID_HISTORY>(new List<T_EB_BID_HISTORY>());
        }
    }
}
