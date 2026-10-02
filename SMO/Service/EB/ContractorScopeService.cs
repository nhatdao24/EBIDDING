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
    public class ContractorScopeService : GenericService<T_EB_CONTRACTOR_SCOPE, ContractorScopeRepo>
    {
        public bool IsViewOnly { get; set; }
        public ContractorScopeService() : base()
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
                UnitOfWork.GetSession().CreateSQLQuery($"DELETE T_EB_CONTRACTOR_SCOPE WHERE CONTRACTOR_ID = '{this.ObjDetail.CONTRACTOR_ID}'").ExecuteUpdate();

                if (!string.IsNullOrEmpty(this.ObjDetail.SCOPE_CODE))
                {
                    foreach (var item in this.ObjDetail.SCOPE_CODE.Split(','))
                    {
                        this.CurrentRepository.Create(new T_EB_CONTRACTOR_SCOPE()
                        {
                            PKID = Guid.NewGuid().ToString(),
                            CONTRACTOR_ID = this.ObjDetail.CONTRACTOR_ID,
                            SCOPE_CODE = item
                        });
                    }
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
