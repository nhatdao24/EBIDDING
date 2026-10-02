using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_PHASE_FILE : BaseEntity
    {
        public virtual string BID_ID { get; set; }
        public virtual string FILE_ID { get; set; }
        public virtual string PHASE { get; set; }
        public virtual T_CM_FILE_UPLOAD FileUpload { get; set; }
    }
}
