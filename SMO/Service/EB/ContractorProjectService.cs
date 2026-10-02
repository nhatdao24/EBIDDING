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
    public class ContractorProjectService : GenericService<T_EB_CONTRACTOR_PROJECT, ContractorProjectRepo>
    {
        public bool IsViewOnly { get; set; }
        public ContractorProjectService() : base()
        {

        }

        public override void Create()
        {
            this.ObjDetail.PKID = Guid.NewGuid().ToString();
            base.Create();
        }
    }
}
