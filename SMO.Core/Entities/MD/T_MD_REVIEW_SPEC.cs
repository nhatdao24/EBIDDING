using SharpSapRfc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_MD_REVIEW_SPEC : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string PARENT_ID { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        [MaxLength(length: 2000, ErrorMessage = "Chỉ được phép nhập tối đa 2000 kí tự")]

        public virtual string NAME { get; set; }
        [MaxLength(length: 2000, ErrorMessage = "Chỉ được phép nhập tối đa 2000 kí tự")]

        public virtual string NAME_EN { get; set; }
        [MaxLength(length: 2000, ErrorMessage = "Chỉ được phép nhập tối đa 2000 kí tự")]

        public virtual string DESCRIPTION { get; set; }
        [MaxLength(length: 2000, ErrorMessage = "Chỉ được phép nhập tối đa 2000 kí tự")]

        public virtual string DESCRIPTION_EN { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual int C_ORDER { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual decimal RATIO { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual int MAX_SCORE { get; set; }
        public virtual string VAI_TRO { get; set; }
        public virtual bool IS_DEFAULT { get; set; }

        public virtual IList<T_MD_REVIEW_SPEC_DETAIL> ListDetail { get; set; }

        public T_MD_REVIEW_SPEC()
        {
            ListDetail = new List<T_MD_REVIEW_SPEC_DETAIL>();
        }
    }
}
