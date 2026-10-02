using SharpSapRfc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    [Serializable]
    public partial class T_MD_OPTION : BaseEntity
    {
        public virtual string CODE { get; set; }
        [Required (ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings =false)]
        public virtual string TEXT { get; set; }
        public virtual string TEXT_EN { get; set; }
        public virtual int C_ORDER { get; set; }
        public virtual bool ACTIVE_TEXT { get; set; }
    }
}
