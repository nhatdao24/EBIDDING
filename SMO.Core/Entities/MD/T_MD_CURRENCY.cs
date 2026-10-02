using SharpSapRfc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_MD_CURRENCY : BaseEntity
    {
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        [RegularExpression(@"^\S*$", ErrorMessage = "Không được phép nhập dấu cách")]
        public virtual string CODE { get; set; }
        [MaxLength(length: 100, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        [Required (ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings =false)]
        public virtual string TEXT { get; set; }
    }
}
