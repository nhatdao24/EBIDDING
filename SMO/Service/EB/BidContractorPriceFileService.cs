using NHibernate.Linq;
using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Service.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class BidContractorPriceFileService : GenericService<T_EB_BID_CONTRACTOR_PRICE_FILE, BidContractorPriceFileRepo>
    {
        public BidContractorPriceFileService() : base()
        {

        }

    }
}
