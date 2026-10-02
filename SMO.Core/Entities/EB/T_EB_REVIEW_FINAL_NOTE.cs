using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_FINAL_NOTE : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string HEADER_ID { get; set; }
        public virtual string NOTES { get; set; }
    }
}
