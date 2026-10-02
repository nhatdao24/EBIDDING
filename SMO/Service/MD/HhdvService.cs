using NHibernate;
using SharpSapRfc;
using SharpSapRfc.Plain;
using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using SMO.SAPINT;
using SMO.SAPINT.Function;
using System;
using System.Configuration;
using System.Data;
using SMO.Service.AD;
using System.Data.SqlClient;
using System.Linq;
using SMO.Core.Entities.MD;

namespace SMO.Service.MD
{
    public class HhdvService : GenericService<T_MD_HHDV, HhdvRepo>
    {
        public HhdvService() : base()
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
