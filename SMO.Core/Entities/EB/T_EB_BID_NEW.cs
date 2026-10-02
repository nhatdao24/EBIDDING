using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_NEW : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string TIEU_DE { get; set; }
        public virtual string MO_TA_NGAN { get; set; }
        public virtual string MO_TA_NGAN_EN { get; set; }
        public virtual string BAI_VIET { get; set; }
        public virtual string BAI_VIET_EN { get; set; }
        public virtual bool IS_PUBLIC { get; set; }
        public virtual string IMAGE_PATH { get; set; }
    }
}
