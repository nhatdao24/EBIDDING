using System;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_AD_SYSTEM_CONFIG : BaseEntity
    {
        public virtual string PKID { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string SAP_HOST { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string SAP_CLIENT { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string SAP_NUMBER { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string SAP_USER_NAME { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string SAP_PASSWORD { get; set; }
        public virtual int SAP_TIME_DIFF { get; set; }
        public virtual string CURRENT_CONNECTION { get; set; }
        public virtual string CURRENT_DATABASE_NAME { get; set; }
        public virtual string DIRECTORY_CACHE { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string MAIL_HOST { get; set; }
        public virtual int MAIL_PORT { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string MAIL_USER { get; set; }
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string MAIL_PASSWORD { get; set; }
        public virtual bool MAIL_IS_SSL { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string AD_CONNECTION { get; set; }
        public virtual DateTime? LAST_UPDATE_PR { get; set; }
        public virtual T_AD_CONNECTION Connection { get; set; }
    }
}
