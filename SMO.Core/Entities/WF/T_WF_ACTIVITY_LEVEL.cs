using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_WF_ACTIVITY_LEVEL : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string FK_ACTIVITY { get; set; }
        public virtual string NAME { get; set; }
        public virtual int C_ORDER { get; set; }
        public virtual string NOTES { get; set; }
        public virtual IList<T_WF_ACTIVITY_LEVEL_SENDER> ListSender { get; set; }
    }
}
