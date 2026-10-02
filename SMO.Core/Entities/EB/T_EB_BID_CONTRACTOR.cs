using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_CONTRACTOR : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string STATUS { get; set; }
        public virtual bool IS_SELECTED { get; set; }
        public virtual bool IS_WINNING { get; set; }
        public virtual decimal? PRICE { get; set; } 
        public virtual string CURRENCY_CODE { get; set; }
        public virtual string EMAIL_SENT_ID { get; set; }
        public virtual bool IS_SELF_JOIN { get; set; }
        public virtual decimal? PERCENT_DEBUT { get; set; }
        public virtual decimal? PERCENT_WINNING { get; set; }
        public virtual T_EB_CONTRACTOR Contractor { get; set; }
        public virtual T_EB_BID Bid { get; set; }
        public virtual T_CM_EMAIL EmailSent { get; set; }
    }
}
