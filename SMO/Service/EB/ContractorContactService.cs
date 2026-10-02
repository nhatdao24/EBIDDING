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
    public class ContractorContactService : GenericService<T_EB_CONTRACTOR_CONTACT, ContractorContactRepo>
    {
        public bool IsViewOnly { get; set; }
        public ContractorContactService() : base()
        {

        }

        public override void Create()
        {
            var lstContact = this.CurrentRepository.Queryable().Where(x => x.CONTRACTOR_ID == this.ObjDetail.CONTRACTOR_ID).ToList();
            if (lstContact.Count(x => x.IS_DEFAULT == true) > 0)
            {
                this.State = false;
                this.ErrorMessage = "Thông tin người liên hệ chính đã tồn tại!";
                return;
            }
            this.ObjDetail.PKID = Guid.NewGuid().ToString();
            base.Create();
        }
        public void Update()
        {
            var lstContact = this.CurrentRepository.Queryable().Where(x => x.CONTRACTOR_ID == this.ObjDetail.CONTRACTOR_ID).ToList();
            if (lstContact.Count(x => x.IS_DEFAULT == true && x.PKID != this.ObjDetail.PKID) > 0)
            {
                this.State = false;
                this.ErrorMessage = "Thông tin người liên hệ chính đã tồn tại!";
                return;
            }
            try
            {
                UnitOfWork.Clear();
                UnitOfWork.BeginTransaction();
                this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                this.CurrentRepository.Update(this.ObjDetail);
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
