using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_CONTRACTOR_PROJECT : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string TEN_DU_AN { get; set; }
        public virtual string MO_TA { get; set; }
        public virtual string CHU_DAU_TU { get; set; }
        public virtual decimal GIA_TRI { get; set; }
        public virtual string GIA_TRI_CURRENCY { get; set; }
        public virtual string LOAI_VON { get; set; }
        public virtual DateTime? BAT_DAU { get; set; }
        public virtual DateTime? KET_THUC { get; set; }
    }
}
