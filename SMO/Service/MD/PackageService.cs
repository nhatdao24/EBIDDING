using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using System;

namespace SMO.Service.MD
{
    public class PackageService : GenericService<T_MD_PACKAGE, PackageRepo>
    {
        public PackageService() : base()
        {

        }

        public override void Create()
        {
            try
            {
                if (!this.CheckExist(x => x.CODE == this.ObjDetail.CODE))
                {
                    base.Create();
                }
                else
                {
                    this.State = false;
                    this.MesseageCode = "1101";
                }
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

    }
}