using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Core.Entities.AD
{
    public class T_AD_RIGHT_PROJECT_USER :BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string PROJECT_CODE { get; set; }
        public virtual string USER_NAME { get; set; }
        public virtual string RIGHT_CODE { get; set; }
    }
}
