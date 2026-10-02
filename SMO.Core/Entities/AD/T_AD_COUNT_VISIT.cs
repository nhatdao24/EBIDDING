using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Core.Entities.AD
{
    public partial class T_AD_COUNT_VISIT : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string ACCESS_TYPE { get; set; }
        public virtual string VIEW_TYPE { get; set; }
        public virtual bool IS_MOBILE { get; set; }
    }
}
