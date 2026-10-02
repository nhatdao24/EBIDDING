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
    public class RequestRevenueService : GenericService<T_EB_REQUEST_REVENUE, RequestRevenueRepo>
    {
        public bool IsViewOnly { get; set; }
        public RequestRevenueService() : base()
        {

        }
    }
}
