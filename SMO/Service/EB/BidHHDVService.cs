using NHibernate;
using SharpSapRfc;
using SharpSapRfc.Plain;
using SMO.Core.Entities;
using SMO.Repository.Implement.EB;
using SMO.SAPINT;
using SMO.SAPINT.Function;
using System;
using System.Configuration;
using System.Data;
using SMO.Service.AD;
using System.Data.SqlClient;
using System.Linq;

namespace SMO.Service.EB
{
    public class BidHHDVService : GenericService<T_EB_BID_HHDV, BidHHDVRepo>
    {
        public BidHHDVService() : base()
        {

        }

        
    }
}
