using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_CONTRACTOR_MARKET : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string COUNTRY_CODE { get; set; }
        public virtual T_MD_COUNTRY Country { get; set; }
    }
}
