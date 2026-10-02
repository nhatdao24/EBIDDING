using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_CONTRACTOR : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string REVIEW_ID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string NOTES { get; set; }
        public virtual T_EB_CONTRACTOR Contractor { get; set; }
        public virtual T_EB_REVIEW Review { get; set; }
        public virtual IList<T_EB_REVIEW_CONTRACTOR_DETAIL> ListDetail { get; set; }
    }
}
