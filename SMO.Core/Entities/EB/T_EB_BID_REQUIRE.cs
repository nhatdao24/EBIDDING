using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_REQUIRE : BaseEntity
    {
        public virtual string CODE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string COMPANY_CODE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập (required)", AllowEmptyStrings = false)]
        [MaxLength(length: 200, ErrorMessage = "Chỉ được phép nhập tối đa 200 kí tự")]
        public virtual string NAME { get; set; }
        [MaxLength(length: 1000, ErrorMessage = "Chỉ được phép nhập tối đa 200 kí tự")]
        public virtual string DESCRIPTION { get; set; }
        public virtual string STATUS { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string COMMENT_ID { get; set; }
        public virtual string COMMENT { get; set; }
        public virtual string USER_APPROVE { get; set; }
        public virtual string REASON { get; set; }
        public virtual string NOTES { get; set; }
        public virtual string NOTES_EN { get; set; }
        public virtual string PACKAGE_CODE { get; set; }
        [Required(ErrorMessage = "Bắt buộc chọn thông tin dự án.")]
        public virtual string PROJECT_CODE { get; set; }
        public virtual string SCOPE_CODE { get; set; }
        public virtual string BID_TYPE_CODE { get; set; }
        public virtual string SCOPE_TYPE_CODE { get; set; }
        public virtual bool IS_RENT{ get; set; }
        public virtual List<string> ListFileDelete { get; set; }
        public virtual List<string> ListFilterProject { get; set; }
        public virtual T_AD_USER UserApprove { get; set; }
        public virtual T_AD_USER UserSent { get; set; }
        public virtual T_MD_PACKAGE Package { get; set; }
        public virtual T_MD_PROJECT Project { get; set; }
        public virtual T_MD_SCOPE Scope { get; set; }
        public virtual T_MD_SCOPE_TYPE ScopeType { get; set; }
        public virtual T_MD_BID_TYPE BidType { get; set; }

        public virtual IList<T_EB_BID_REQUIRE_DETAIL> ListDetail { get; set; }
        public virtual IList<T_EB_BID_REQUIRE_PR> ListPR { get; set; }
        public virtual IList<T_EB_BID_REQUIRE_USER> ListUser { get; set; }
        public virtual IList<T_EB_BID_REQUIRE_FILE> ListFile { get; set; }
        public virtual IList<T_EB_BID_REQUIRE_HISTORY> ListHistory { get; set; }

        public T_EB_BID_REQUIRE()
        {
            ListDetail = new List<T_EB_BID_REQUIRE_DETAIL>();
            ListPR = new List<T_EB_BID_REQUIRE_PR>();
            ListUser = new List<T_EB_BID_REQUIRE_USER>();
            ListFile = new List<T_EB_BID_REQUIRE_FILE>();
            ListHistory = new List<T_EB_BID_REQUIRE_HISTORY>();
        }
    }
}
