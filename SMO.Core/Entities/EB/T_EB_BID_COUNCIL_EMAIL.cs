using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_COUNCIL_EMAIL : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string USER_NAME { get; set; }
        public virtual string EMAIL_ID { get; set; }
        public virtual T_AD_USER User { get; set; }
        public virtual T_CM_EMAIL Email { get; set; }
    }
}
