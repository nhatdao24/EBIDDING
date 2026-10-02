using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_FINAL : BaseEntity
    {
        public virtual string CODE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string DESCRIPTION { get; set; }
        public virtual string STATUS { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual DateTime? FROM_DATE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual DateTime? TO_DATE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string USER_APPROVE { get; set; }
        public virtual DateTime? APPROVE_DATE { get; set; }
        public virtual IList<T_EB_REVIEW_FINAL_USER> ListUser { get; set; }
        public virtual IList<T_EB_REVIEW_FINAL_HISTORY> ListHistory { get; set; }
        public virtual ISet<T_EB_REVIEW_FINAL_CONTRACTOR> ListContractor { get; set; }
        public virtual T_AD_USER UserCreate { get; set; }

        public T_EB_REVIEW_FINAL()
        {
            ListUser = new List<T_EB_REVIEW_FINAL_USER>();
            ListHistory = new List<T_EB_REVIEW_FINAL_HISTORY>();
            ListContractor = new HashSet<T_EB_REVIEW_FINAL_CONTRACTOR>(new List<T_EB_REVIEW_FINAL_CONTRACTOR>());
            UserCreate = new T_AD_USER();
        }
    }
}
