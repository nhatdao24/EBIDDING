using SharpSapRfc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace SMO.Core.Entities
{
    public partial class T_MD_PACKAGE : BaseEntity
    {
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string CODE { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string TEXT { get; set; }
    }
}
