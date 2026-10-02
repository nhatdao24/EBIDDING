using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    [Serializable]
    public partial class T_AD_USER_GROUP : BaseEntity
    {
        [MaxLength(length: 50, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string CODE { get; set; }
        public virtual string ORGANIZE_ID { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string NAME { get; set; }
        [MaxLength(length: 500, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string NOTES { get; set; }
        public virtual bool ACTIVE { get; set; }
        public virtual IList<T_AD_USER_USER_GROUP> ListUserUserGroup { get; set; }
        public virtual IList<T_AD_USER_GROUP_ROLE> ListUserGroupRole { get; set; }

        public T_AD_USER_GROUP()
        {
            ListUserUserGroup = new List<T_AD_USER_USER_GROUP>();
            ListUserGroupRole = new List<T_AD_USER_GROUP_ROLE>();
        }
    }
}
