using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_HHDV : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string CODE { get; set; }
        public virtual string TEXT { get; set; }
        public virtual string DESCRIPTION { get; set; }
        public virtual string QUANTITY { get; set; }
        public virtual string UNIT { get; set; }
        public virtual decimal PRICE_VTB { get; set; }

        public virtual T_EB_BID Bid { get; set; }
       
    }
}
