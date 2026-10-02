using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_CONTRACTOR_PRICE : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual Decimal FIRST_PRICE { get; set; }
        public virtual Decimal PRICE { get; set; }
        public virtual string CURRENCY_CODE { get; set; }
        public virtual string TYPE { get; set; }
        public virtual int C_ORDER { get; set; }
        public virtual string NOTE { get; set; }
        public virtual string IS_DELETE { get; set; }
        public virtual string ListFileDelete { get; set; }
    }
}
