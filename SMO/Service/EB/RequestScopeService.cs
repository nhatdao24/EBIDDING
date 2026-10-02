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
    public class RequestScopeService : GenericService<T_EB_REQUEST_SCOPE, RequestScopeRepo>
    {
        public bool IsViewOnly { get; set; }
        public RequestScopeService() : base()
        {

        }
    }
}
