using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_INTERNAL_QUESTION_FILE : BaseEntity
    {
        public virtual string FILE_ID { get; set; }
        public virtual string QUESTION_ID { get; set; }
        public virtual T_CM_FILE_UPLOAD FileUpload { get; set; }
    }
}
