using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_CONTRACTOR_EMAIL : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string EMAIL_ID { get; set; }
        public virtual string TYPE { get; set; }
        public virtual T_EB_CONTRACTOR Contractor { get; set; }
        public virtual T_CM_EMAIL Email{ get; set; }
    }
}
