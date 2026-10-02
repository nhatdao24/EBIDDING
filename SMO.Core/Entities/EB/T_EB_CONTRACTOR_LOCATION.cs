using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_CONTRACTOR_LOCATION : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string CONTRACTOR_ID { get; set; }
        public virtual string COUNTRY_CODE { get; set; }

        public virtual string ADDRESS { get; set; }

        public virtual string EMAIL { get; set; }

        public virtual string PHONE { get; set; }
        public virtual bool IS_REMOVE { get; set; }
        public virtual T_MD_COUNTRY Country { get; set; }
    }
}
