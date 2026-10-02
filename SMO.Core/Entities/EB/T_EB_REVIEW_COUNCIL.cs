using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_COUNCIL : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string REVIEW_ID { get; set; }
        public virtual string USER_NAME { get; set; }
        public virtual string VAI_TRO { get; set; }
        public virtual string EMAIL_ID { get; set; }
        public virtual string STATUS { get; set; }
        public virtual T_AD_USER User { get; set; }
    }
}
