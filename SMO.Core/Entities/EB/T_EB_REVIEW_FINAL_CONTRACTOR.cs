using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_FINAL_CONTRACTOR : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string REVIEW_ID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual decimal SCORE { get; set; }
        public virtual string RATE { get; set; }
        public virtual string NOTES { get; set; }
        public virtual string COMMENT { get; set; }
        public virtual bool IS_NEW_CONTRACTOR { get; set; }
        public virtual T_EB_CONTRACTOR Contractor { get; set; }
        public virtual T_EB_REVIEW_FINAL ReviewFinal { get; set; }
        public virtual IList<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL> ListDetail { get; set; }
        public virtual IList<T_EB_REVIEW_FINAL_NOTE> ListNote { get; set; }

        public T_EB_REVIEW_FINAL_CONTRACTOR()
        {
            ReviewFinal = new T_EB_REVIEW_FINAL();
            Contractor = new T_EB_CONTRACTOR();
            ListDetail = new List<T_EB_REVIEW_FINAL_CONTRACTOR_DETAIL>();
            ListNote = new List<T_EB_REVIEW_FINAL_NOTE>();
        }
    }
}
