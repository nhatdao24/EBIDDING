using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_WF_PROCESS : BaseEntity
    {
        public virtual string CODE { get; set; }
        public virtual string NAME { get; set; }
        public virtual string NOTES { get; set; }
        public virtual IList<T_WF_ACTIVITY> ListActivity { get; set; }
    }
}
