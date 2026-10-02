using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Core.Entities.MD
{
    public class T_MD_HHDV: BaseEntity
    {
        public T_MD_HHDV() { ACTIVE = "Y"; }
        public virtual string CODE { get; set; }
        public virtual string NAME { get; set; }
        public virtual string ACTIVE { get; set; }


    }
}
