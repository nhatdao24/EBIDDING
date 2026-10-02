using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_MD_DOMAIN : BaseEntity
    {
        [Required (ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings =false)]
        [MaxLength(length:50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]
        [RegularExpression(@"^\S*$", ErrorMessage = "Không được phép nhập dấu cách")]
        public virtual string CODE { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string NAME { get; set; }
        public virtual string DATA_TYPE { get; set; }
        [MaxLength(length: 200, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string NOTE { get; set; }
        //public virtual bool ACTIVE { get; set; }
        public virtual IList<T_MD_DICTIONARY> ListDictionary { get; set; }
    }
}
