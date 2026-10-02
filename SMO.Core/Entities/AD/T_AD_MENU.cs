using System;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_AD_MENU : BaseEntity
    {
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string CODE { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string DESCRIPTION { get; set; }
        public virtual string PARENT { get; set; }
        public virtual int C_ORDER { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string FK_RIGHT { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string LINK { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string ICON { get; set; }
        //Thuộc tính thêm để lấy dữ liệu từ bảng Language
        public virtual string LANGUAGE_VALUE { get; set; }
    }
}
