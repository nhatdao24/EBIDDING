using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_SPEC_CONTRACTOR : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string BID_ID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string SPEC_ID { get; set; }
        public virtual string FIRST_VALUE { get; set; }
        public virtual string VALUE { get; set; }
    }
}
