using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using SMO.Core.Entities.MD;
using System;

namespace SMO.Service.MD
{
    public class RentUnitService : GenericService<T_MD_RENT_UNIT, RentUnitRepo>
    {
        public RentUnitService() : base()
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