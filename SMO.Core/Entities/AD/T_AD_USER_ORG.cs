using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    [Serializable]
    public partial class T_AD_USER_ORG : BaseEntity
    {
        public virtual string ORGANIZE_ID { get; set; }
        [MaxLength(length: 20, ErrorMessage = "Chỉ được phép nhập tối đa {1} kí tự")]

        public virtual string USER_NAME { get; set; }

        public virtual T_AD_ORGANIZE Organize { get; set; }
        public virtual T_AD_USER User { get; set; }

        public T_AD_USER_ORG()
        {
            Organize = new T_AD_ORGANIZE();
            User = new T_AD_USER();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is T_AD_USER_ORG other)) return false;
            return ReferenceEquals(this, other) ? true : ORGANIZE_ID == other.ORGANIZE_ID && USER_NAME == other.USER_NAME;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = GetType().GetHashCode();
                hash = (hash * 31) ^ ORGANIZE_ID.GetHashCode();
                hash = (hash * 31) ^ USER_NAME.GetHashCode();
                return hash;
            }
        }
    }
}
