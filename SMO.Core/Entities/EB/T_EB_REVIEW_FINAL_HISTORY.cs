using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_FINAL_HISTORY : BaseEntity
    {
        public virtual int PKID { get; set; }
        public virtual string REVIEW_ID { get; set; }
        public virtual string STATUS { get; set; }
        public virtual string NOTES { get; set; }
        public virtual DateTime? STATUS_DATE { get; set; }

        public T_EB_REVIEW_FINAL_HISTORY()
        {

        }
    }
}
