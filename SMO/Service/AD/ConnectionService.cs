using Hangfire;
using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMO.Service.AD
{
    public class ConnectionService : GenericService<T_AD_CONNECTION, ConnectionRepo>
    {
        public ConnectionService() : base()
        {
            
        }

        public override void Create()
        {
            this.ObjDetail.PKID = Guid.NewGuid().ToString();
            base.Create();  
        }
    }
}
