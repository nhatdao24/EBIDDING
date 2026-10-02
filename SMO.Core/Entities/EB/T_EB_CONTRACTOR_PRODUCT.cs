using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_CONTRACTOR_PRODUCT : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string PRODUCT_CODE { get; set; }
        public virtual string DESCRIPTION { get; set; }
        public virtual T_MD_MATERIAL_TYPE Product { get; set; }
    }
}
