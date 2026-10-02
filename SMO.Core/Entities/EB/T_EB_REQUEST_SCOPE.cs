using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REQUEST_SCOPE : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string REQUEST_ID { get; set; }
        public virtual string SCOPE_CODE { get; set; }
        public virtual T_MD_SCOPE Scope { get; set; }
    }
}
