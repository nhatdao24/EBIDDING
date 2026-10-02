using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_WF_ACTIVITY_LEVEL_SENDER : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string FK_LEVEL { get; set; }
        public virtual string USERNAMES { get; set; }
        public virtual string TYPE { get; set; }
        public virtual string LEVEL { get; set; }
        public virtual string NOTES { get; set; }
        public virtual IList<T_WF_ACTIVITY_LEVEL_RECEIVER> ListReceiver { get; set; }
    }
}
