using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_SPEC_DETAIL : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string HEADER_ID { get; set; }
        public virtual string FROM_VALUE { get; set; }
        public virtual string TO_VALUE { get; set; }
        public virtual string VALUE { get; set; }
        public virtual string VALUE_EN { get; set; }
        public virtual string SCORE { get; set; }
        public virtual int C_ORDER { get; set; }
    }
}
