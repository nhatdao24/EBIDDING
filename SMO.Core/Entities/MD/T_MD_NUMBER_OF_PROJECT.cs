using SharpSapRfc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    [Serializable]
    public partial class T_MD_NUMBER_OF_PROJECT : BaseEntity
    {
        public virtual string CODE { get; set; }
        [Required (ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings =false)]
        [MaxLength(length: 100, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string TEXT { get; set; }
        [MaxLength(length: 100, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string TEXT_EN { get; set; }
        public virtual int C_ORDER { get; set; }
        public virtual bool ACTIVE_TEXT { get; set; }
    }
}
