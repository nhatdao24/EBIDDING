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
    public class ContractorMarketService : GenericService<T_EB_CONTRACTOR_MARKET, ContractorMarketRepo>
    {
        public bool IsViewOnly { get; set; }
        public ContractorMarketService() : base()
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
                UnitOfWork.GetSession().CreateSQLQuery($"DELETE T_EB_CONTRACTOR_MARKET WHERE CONTRACTOR_ID = '{this.ObjDetail.CONTRACTOR_ID}'").ExecuteUpdate();

                foreach (var item in this.ObjDetail.COUNTRY_CODE.Split(','))
                {
                    this.CurrentRepository.Create(new T_EB_CONTRACTOR_MARKET()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        CONTRACTOR_ID = this.ObjDetail.CONTRACTOR_ID,
                        COUNTRY_CODE = item,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
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
