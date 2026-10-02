using SharpSapRfc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_MD_SCOPE_TYPE : BaseEntity
    {
        public virtual string CODE { get; set; }
        [Required (ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings =false)]
        [MaxLength(length: 100, ErrorMessage = "Chỉ được phép nhập tối đa 500 kí tự")]

        public virtual string TEXT { get; set; }
        [MaxLength(length: 100, ErrorMessage = "Chỉ được phép nhập tối đa 500 kí tự")]
        public virtual string TEXT_EN { get; set; }
    }
}
