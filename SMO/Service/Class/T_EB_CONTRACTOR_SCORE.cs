using SMO.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMO.Service
{
    public class T_EB_CONTRACTOR_SCORE
    {
        public T_EB_BID_CONTRACTOR BidContractor { get; set; }
        public decimal SYSTEM_SCORE { get; set; }
        public decimal USER_SCORE { get; set; }
    }
}