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
using System.Data.SqlClient;
using SMO.Service.AD;
using System.Linq;
using SMO.Core.Entities.MD;
using System.Collections.Generic;


namespace SMO.Service.MD
{
    public class MaterialService : GenericService<T_MD_MATERIAL, MaterialRepo>
    {
        public MaterialService() : base()
        {

        }

        public override void Create()
        {
            try
            {

                this.ObjDetail.CODE = "MT" + this.GetSequence("MATERIAL");
                UnitOfWork.BeginTransaction();


                UnitOfWork.Commit();
                base.Create();

            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
                UnitOfWork.Rollback();
            }

        }

        public override void Get(object id, dynamic param = null)
        {
            try
            {

                this.ObjDetail = this.CurrentRepository.Get(id, param);

              
                base.Get(id);
            }
            catch (Exception ex)
            {
                //UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

        public override void Update()
        {
            try
            {
                UnitOfWork.BeginTransaction();

                if (ProfileUtilities.User != null)
                {
                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                }


                UnitOfWork.Commit();
                base.Update();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }

    }
}
