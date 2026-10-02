using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Core.Entities.AD
{
    public class T_AD_RIGHT_PROJECT : BaseEntity
    {
        public virtual string CODE { get; set; }
        public virtual string NAME { get; set; }
        public virtual string PARENT { get; set; }
    }
}
