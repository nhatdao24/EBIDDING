using System;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_AD_CONNECTION : BaseEntity
    {
        public virtual string PKID { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]

        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string NAME { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string ADDRESS { get; set; }
        public virtual string USERNAME { get; set; }
        public virtual string PASSWORD { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string DIRECTORY { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string NOTES { get; set; }
    }
}
