using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW : BaseEntity
    {
        public virtual string CODE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string DESCRIPTION { get; set; }
        public virtual string ORG_CODE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string PROJECT_CODE { get; set; }
        public virtual string STATUS { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual DateTime EVALUATE_DATE { get; set; }
        public virtual DateTime END_DATE { get; set; }
        public virtual int MAX_SCORE { get; set; }
        public virtual T_AD_ORGANIZE Organize { get; set; }
        public virtual T_MD_PROJECT Project { get; set; }
        public virtual IList<T_EB_REVIEW_CONTRACTOR> ListContractor { get; set; }
        public virtual IList<T_EB_REVIEW_COUNCIL> ListCouncil { get; set; }

        public T_EB_REVIEW()
        {
            ListContractor = new List<T_EB_REVIEW_CONTRACTOR>();
            ListCouncil = new List<T_EB_REVIEW_COUNCIL>();
        }

        // Trường tìm kiếm
        public virtual DateTime? FROM_DATE { get; set; }
        public virtual DateTime? TO_DATE { get; set; }
    }
}
