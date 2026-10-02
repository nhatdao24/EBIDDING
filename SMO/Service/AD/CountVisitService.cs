using Hangfire;
using SMO.Core.Entities;
using SMO.Core.Entities.AD;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMO.Service.AD
{
    public class CountVisitService : GenericService<T_AD_COUNT_VISIT, CountVisitRepo>
    {
        public CountVisitService() : base()
        {

        }

        public override void Create()
        {
            try
            {
                UnitOfWork.BeginTransaction();
                this.ObjDetail.PKID = Guid.NewGuid().ToString();
                this.ObjDetail = this.CurrentRepository.Create(this.ObjDetail);
                UnitOfWork.Commit();
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
