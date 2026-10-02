using SharpSapRfc;
using SMO.Core.Entities.MD;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_MD_SCOPE : BaseEntity
    {
        public virtual string CODE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string TYPE_CODE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        [MaxLength(length: 100, ErrorMessage = "Chỉ được phép nhập tối đa 500 kí tự")]

        public virtual string TEXT { get; set; }
        [MaxLength(length: 100, ErrorMessage = "Chỉ được phép nhập tối đa 500 kí tự")]

        public virtual string TEXT_EN { get; set; }
        public virtual bool ACTIVE_TEXT { get; set; }
        public virtual T_MD_SCOPE_TYPE SCOPE_TYPE { get; set; }
        public virtual IList<T_MD_SCOPE_MATERIAL> ScopeMaterials { get; set; } = new List<T_MD_SCOPE_MATERIAL>();

    }
}
