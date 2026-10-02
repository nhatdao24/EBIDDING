using SharpSapRfc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    [Serializable]
    public partial class T_MD_RATING : BaseEntity
    {
        public virtual string PKID { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]


        public virtual decimal FROM_VALUE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]

        public virtual decimal TO_VALUE { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập", AllowEmptyStrings = false)]

        public virtual string RATING { get; set; }
        public virtual bool IS_BLACK_LIST { get; set; }
    }
}
