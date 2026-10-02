using System;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_AD_LANGUAGE : BaseEntity
    {
        public virtual string PKID { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string FK_CODE { get; set; }
        public virtual string OBJECT_TYPE { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string FORM_CODE { get; set; }
        public virtual string LANG { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string VALUE { get; set; }
    }   
}
