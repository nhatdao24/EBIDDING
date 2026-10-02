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
    public class ContractorProductService : GenericService<T_EB_CONTRACTOR_PRODUCT, ContractorProductRepo>
    {
        public bool IsViewOnly { get; set; }
        public ContractorProductService() : base()
        {

        }

        public override void Update()
        {
            try
            {
                var serviceContractor = new ContractorService();
                serviceContractor.Get(this.ObjDetail.CONTRACTOR_ID);
                if (serviceContractor.ObjDetail == null)
                {
                    this.State = false;
                    return;
                }

                UnitOfWork.BeginTransaction();
                UnitOfWork.GetSession().CreateSQLQuery($"DELETE T_EB_CONTRACTOR_PRODUCT WHERE CONTRACTOR_ID = '{this.ObjDetail.CONTRACTOR_ID}'").ExecuteUpdate();

                foreach (var item in this.ObjDetail.PRODUCT_CODE.Split(','))
                {
                    this.CurrentRepository.Create(new T_EB_CONTRACTOR_PRODUCT()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        CONTRACTOR_ID = this.ObjDetail.CONTRACTOR_ID,
                        PRODUCT_CODE = item
                    });
                }

                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = true;
                this.Exception = ex;
            }
        }

    }
}
