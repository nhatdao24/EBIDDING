using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_CONTRACTOR_DETAIL : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string HEADER_ID { get; set; }
        public virtual string INFO_BID { get; set; }
        public virtual string INFO_BID_EBID { get; set; }
        public virtual string MATERIAL_TYPE { get; set; }
        public virtual decimal TOTAL_SCORE { get; set; }
        public virtual decimal MEDIUM_SCORE { get; set; }
        public virtual string RATE { get; set; }
        public virtual string CONG_VIEC { get; set; }
        [Range(0, float.MaxValue, ErrorMessage = "Hãy nhập giá trị kiểu số")]
        public virtual decimal GIA_TRI { get; set; }
        [Range(0, float.MaxValue, ErrorMessage = "Hãy nhập giá trị kiểu số")]
        public virtual decimal RATIO_IN_PROJECT { get; set; }
    }
}
