using System;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_AD_REQUEST_RESET_PASS : BaseEntity
    {
        public virtual string PKID { get; set; }
        [Required(ErrorMessage = "Trường này bắt buộc nhập (required)", AllowEmptyStrings = false)]
        public virtual string USER_NAME { get; set; }
        public virtual DateTime REQUEST_DATE { get; set; }

        //Trường Nhập mật khẩu
        public virtual string Password { get; set; }
        public virtual string Password2 { get; set; }
    }
}
