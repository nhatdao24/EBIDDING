using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_WF_ACTIVITY : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string FK_PROCESS { get; set; }
        public virtual string CODE { get; set; }
        public virtual string NAME { get; set; }
        public virtual string TYPE { get; set; }
        public virtual int C_ORDER { get; set; }
        public virtual string NOTES { get; set; }
        public virtual IList<T_WF_ACTIVITY_LEVEL> ListLevel { get; set; }
    }
}
