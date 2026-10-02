using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_CONTRACTOR_FINANCE : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual int YEAR { get; set; }
        public virtual decimal DOANH_THU { get; set; } 
        public virtual string DOANH_THU_CURRENCY { get; set; }
        public virtual decimal LOI_NHUAN_RONG { get; set; }
        public virtual string LOI_NHUAN_RONG_CURRENCY { get; set; }
        public virtual decimal LOI_NHUAN_GOP { get; set; }
        public virtual string LOI_NHUAN_GOP_CURRENCY { get; set; }
        public virtual decimal VON_CSH { get; set; }
        public virtual string VON_CSH_CURRENCY { get; set; }
        public virtual string FILE_ID { get; set; }
        public virtual T_CM_FILE_UPLOAD File { get; set; }

    }
}
