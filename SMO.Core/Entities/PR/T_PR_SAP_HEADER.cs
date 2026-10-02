using SharpSapRfc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_PR_SAP_HEADER : BaseEntity
    {
        [RfcStructureField("BANFN")]
        public virtual string CODE { get; set; }
        [RfcStructureField("NAME_COMPANY")]
        public virtual string COMPANY_NAME { get; set; }
        [RfcStructureField("NAME_YEUCAU")]
        public virtual string REQUIRE_NAME { get; set; }
        [RfcStructureField("NAME_DENGHI")]
        public virtual string REQUIRE_USER { get; set; }
        [RfcStructureField("NAME_BOPHAN")]
        public virtual string DEPARTMENT_NAME { get; set; }
        [RfcStructureField("NGAY")]
        public virtual DateTime? REQUIRE_DATE { get; set; }
        [RfcStructureField("THOIHAN")]
        public virtual DateTime? DESTINATION_DATE { get; set; }
        public virtual bool IS_SELECTED { get; set; }

        public virtual IList<T_PR_SAP_DETAIL> ListDetail { get; set; }
        public virtual IList<T_EB_BID_REQUIRE_PR> ListBidRequirePR { get; set; }

        public T_PR_SAP_HEADER()
        {
            ListDetail = new List<T_PR_SAP_DETAIL>();
            ListBidRequirePR = new List<T_EB_BID_REQUIRE_PR>();
        }
    }
}
