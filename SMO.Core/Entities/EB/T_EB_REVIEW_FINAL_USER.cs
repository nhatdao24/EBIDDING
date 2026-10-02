using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_FINAL_USER : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string REVIEW_ID { get; set; }
        public virtual string USER_NAME { get; set; }
        public virtual int C_LEVEL { get; set; }
        public virtual bool IS_APPROVED { get; set; }
        public virtual string ROLE { get; set; }
        public virtual DateTime? DATE_APPROVE { get; set; }
        public virtual string IS_DELETE { get; set; }
        public virtual T_AD_USER User { get; set; }

        public T_EB_REVIEW_FINAL_USER()
        {
            User = new T_AD_USER();
        }
    }
}
