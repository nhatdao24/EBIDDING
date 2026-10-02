using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_REQUIRE_PR : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string HEADER_CODE { get; set; }
        public virtual string TYPE { get; set; }
        public virtual string PR_CODE { get; set; }
        public virtual string PR_NAME { get; set; }

        public virtual string PROJECT_CODE { get; set; }

        public virtual IList<T_PR_SAP_DETAIL> ListDetail { get; set; }

        public T_EB_BID_REQUIRE_PR()
        {
            ListDetail = new List<T_PR_SAP_DETAIL>();
        }
    }
}
