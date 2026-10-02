using SMO.Core.Entities.MD;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMO.Core.Entities
{
    public partial class T_EB_BID_REQUIRE_DETAIL : BaseEntity
    {
        public virtual string PKID { get; set; }
        public virtual string HEADER_CODE { get; set; }
        public virtual int ITEM_NUMBER { get; set; }
        public virtual string MATERIAL_CODE { get; set; }
        public virtual string UNIT_CODE { get; set; }
        public virtual decimal QUANTITY { get; set; }
        public virtual decimal SUM_VALUE { get; set; }
        public virtual string CURRENCY_CODE { get; set; }
        public virtual string SCOPE_CODE { get; set; }
        public virtual int TIME_RENT { get; set; }
        public virtual string RENT_UNIT_CODE { get; set; }
        public virtual string NOTE { get; set; }
        public virtual T_MD_UNIT Unit { get; set; }
        public virtual T_MD_MATERIAL Material { get; set; }
        public virtual T_MD_RENT_UNIT RentUnit { get; set; }
        public virtual T_MD_SCOPE Scope { get; set; }
    }
}
