using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_RATING : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string REVIEW_ID { get; set; }
        public virtual decimal FROM_VALUE { get; set; }
        public virtual decimal TO_VALUE { get; set; }
        public virtual string RATING { get; set; }
        public virtual int C_ORDER { get; set; }
    }
}
