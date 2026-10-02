using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_WF_ACTIVITY_LEVEL_RECEIVER : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string FK_SENDER { get; set; }
        public virtual string USERNAMES { get; set; }
        public virtual string NOTES { get; set; }
        public virtual IList<T_WF_ACTIVITY_LEVEL_RECEIVER_COM> ListCom { get; set; }
    }
}
