using SharpSapRfc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_PR_SAP_DETAIL : BaseEntity
    {
        public virtual string PKID { get; set; }
        [RfcStructureField("BANFN")]
        public virtual string HEADER_CODE { get; set; }
        [RfcStructureField("BNFPO")]
        public virtual int ITEM_NUMBER { get; set; }
        [RfcStructureField("MATNR")]
        public virtual string MATERIAL_CODE { get; set; }
        [RfcStructureField("MAKTX")]
        public virtual string MATERIAL_NAME { get; set; }
        [RfcStructureField("MENGE")]
        public virtual decimal QUANTITY { get; set; }
        [RfcStructureField("MEINS")]
        public virtual string UNIT { get; set; }
        [RfcStructureField("PREIS")]
        public virtual decimal PRICE { get; set; }
        [RfcStructureField("GSWRT")]
        public virtual decimal SUM_VALUE { get; set; }
        [RfcStructureField("WAERS")]
        public virtual string CURRENCY { get; set; }
    }
}
