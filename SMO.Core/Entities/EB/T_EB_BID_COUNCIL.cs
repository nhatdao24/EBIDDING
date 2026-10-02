using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_COUNCIL : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string USER_NAME { get; set; }
        public virtual string VAI_TRO { get; set; }
        public virtual bool? IS_QUYET_DINH { get; set; }
        public virtual T_AD_USER User { get; set; }
    }
}
