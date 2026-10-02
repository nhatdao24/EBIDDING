using System;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_AD_MESSAGE : BaseEntity
    {
        public virtual string PKID { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string CODE { get; set; }
        public virtual string LANGUAGE { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string MESSAGE { get; set; }
        public virtual bool ACTIVE { get; set; }
    }   
}
