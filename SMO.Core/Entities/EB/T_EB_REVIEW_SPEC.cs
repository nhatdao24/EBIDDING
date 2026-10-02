using SharpSapRfc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_REVIEW_SPEC : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string PARENT_ID { get; set; }
        public virtual string REVIEW_ID { get; set; }
        public virtual string MD_PKID { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual string NAME { get; set; }
        public virtual string NAME_EN { get; set; }
        public virtual string DESCRIPTION { get; set; }
        public virtual string DESCRIPTION_EN { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual int C_ORDER { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual decimal RATIO { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]
        public virtual int MAX_SCORE { get; set; }
        public virtual string VAI_TRO { get; set; }
        public virtual bool IS_DEFAULT { get; set; }

        public virtual IList<T_EB_REVIEW_SPEC_DETAIL> ListDetail { get; set; }

        public T_EB_REVIEW_SPEC()
        {
            ListDetail = new List<T_EB_REVIEW_SPEC_DETAIL>();
        }
    }
}
