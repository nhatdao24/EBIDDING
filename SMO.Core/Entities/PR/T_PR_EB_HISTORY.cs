using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities.PR
{
    public class T_PR_EB_HISTORY : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string HEADER_CODE { get; set; }
        public virtual string ACTION { get; set; }
        public virtual string ACTION_USER { get; set; }
        public virtual DateTime? ACTION_DATE { get; set; }
        public virtual string NOTES { get; set; }
        public virtual T_AD_USER ActionUser { get; set; }
    }
}
