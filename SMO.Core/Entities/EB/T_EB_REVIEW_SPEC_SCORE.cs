using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_SPEC_SCORE : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string SPEC_ID { get; set; }
        public virtual string REVIEW_CONTRACTOR_DETAIL_ID { get; set; }
        public virtual decimal? SCORE { get; set; }
        public virtual string USER_SCORE { get; set; }
        public virtual string NOTES { get; set; }
        // Thông tin thêm
        public virtual decimal SCORE_MULTIPLY { get; set; }

    }
}
