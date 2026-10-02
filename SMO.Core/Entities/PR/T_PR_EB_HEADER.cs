using SharpSapRfc;
using SMO.Core.Entities.PR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_PR_EB_HEADER : BaseEntity
    {
        public virtual string CODE { get; set; }
        public virtual string COMPANY_CODE { get; set; }

        [Required(ErrorMessage = "Trường này bắt buộc nhập.", AllowEmptyStrings = false)]
        public virtual string REQUIRE_NAME { get; set; }
        public virtual DateTime? REQUIRE_DATE { get; set; }
        public virtual DateTime? DESTINATION_DATE { get; set; }
        public virtual bool IS_SELECTED { get; set; }
        public virtual bool IS_RENT{ get; set; }
        public virtual string STATUS { get; set; }
        public virtual string DESCRIPTION { get; set; }
        public virtual string COMMENT_ID { get; set; }
        [Required(ErrorMessage = "Bắt buộc chọn thông tin người phê duyệt")]
        public virtual string USER_APPROVE { get; set; }
        public virtual string REASON { get; set; }
        public virtual string PACKAGE_CODE { get; set; }
        [Required(ErrorMessage = "Bắt buộc chọn thông tin dự án.")]
        public virtual string PROJECT_CODE { get; set; }
        public virtual string SCOPE_CODE { get; set; }
        public virtual string BID_TYPE_CODE { get; set; }
        public virtual string SCOPE_TYPE_CODE { get; set; }
        public virtual List<string> ListFileDelete { get; set; }
        public virtual List<string> ListFilterProject { get; set; }

        public virtual List<string> ListPrForAdd { get; set; }
        public virtual T_AD_USER UserApprove { get; set; }    
        public virtual T_AD_USER UserSent { get; set; }
        public virtual T_MD_PACKAGE Package { get; set; }
        public virtual T_MD_PROJECT Project { get; set; }
        public virtual T_MD_SCOPE Scope { get; set; }
        public virtual T_MD_SCOPE_TYPE ScopeType { get; set; }
        public virtual T_MD_BID_TYPE BidType { get; set; }
        public virtual IList<T_PR_EB_DETAIL> ListDetail { get; set; }
        public virtual IList<T_EB_BID_REQUIRE_PR> ListBidRequirePR { get; set; }
        public virtual IList<T_PR_EB_FILE> ListFile { get; set; }
        public virtual IList<T_PR_EB_HISTORY> ListHistory { get; set; }

        public T_PR_EB_HEADER() 
        {
            ListDetail = new List<T_PR_EB_DETAIL>();
            ListFile = new List<T_PR_EB_FILE>();
            ListBidRequirePR = new List<T_EB_BID_REQUIRE_PR>();
            ListHistory = new List<T_PR_EB_HISTORY>();
        }
    }
}
