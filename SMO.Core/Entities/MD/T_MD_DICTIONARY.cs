using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Core.Entities
{
    public class T_MD_DICTIONARY : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string FK_DOMAIN { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        [MaxLength(length: 100, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]
        [RegularExpression(@"^\S*$", ErrorMessage = "Không được phép nhập dấu cách")]
        public virtual string CODE { get; set; } 
        public virtual string LANG { get; set; }
        [MaxLength(length: 100, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string C_VALUE { get; set; }
        public virtual int C_ORDER { get; set; }
        public virtual bool C_DEFAULT { get; set; }
        public virtual bool ACTIVE { get; set; }
    }
}
