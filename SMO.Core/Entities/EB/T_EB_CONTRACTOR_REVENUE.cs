using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_CONTRACTOR_REVENUE : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string YEAR { get; set; }
        public virtual decimal VALUE { get; set; }
        public virtual string UNIT_CODE { get; set; }
    }
}
