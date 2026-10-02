using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Core.Entities.PR
{
    public class T_PR_EB_FILE : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string HEADER_CODE { get; set; }
        public virtual string FILE_ID { get; set; }

        public virtual T_CM_FILE_UPLOAD File { get; set; }


    }
}
