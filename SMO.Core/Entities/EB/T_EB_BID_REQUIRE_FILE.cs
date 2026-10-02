using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_REQUIRE_FILE : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string HEADER_CODE { get; set; }
        public virtual string FILE_ID { get; set; }
        public virtual T_CM_FILE_UPLOAD File { get; set; }
    }
}
