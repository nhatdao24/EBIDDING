using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using System;

namespace SMO.Service.MD
{
    public class ScopeTypeService : GenericService<T_MD_SCOPE_TYPE, ScopeTypeRepo>
    {
        public ScopeTypeService() : base()
        {

        }

        public override void Create()
        {
            try
            {
                    this.ObjDetail.CODE = Guid.NewGuid().ToString();
                    base.Create();
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }
    }
}