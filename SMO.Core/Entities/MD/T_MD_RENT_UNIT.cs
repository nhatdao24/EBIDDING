using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Core.Entities.MD
{
    public class T_MD_RENT_UNIT : BaseEntity
    {
        [MaxLength(length: 10, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]


        public virtual string CODE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        [MaxLength(length: 30, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string TEXT { get; set; }
        [Required]
        public virtual char ACTIVE { get; set; } = '1';
        public virtual bool IsActive => ACTIVE == '1';
    }
}
