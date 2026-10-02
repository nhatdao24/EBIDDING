using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_HISTORY : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string ACTION { get; set; }
        public virtual string ACTION_USER { get; set; }
        public virtual string NOTES { get; set; }
        public virtual DateTime? ACTION_DATE { get; set; }
        public virtual T_AD_USER ActionUser { get; set; }
    }
}
