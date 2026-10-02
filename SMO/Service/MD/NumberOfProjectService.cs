using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using System;

namespace SMO.Service.MD
{
    public class NumberOfProjectService : GenericService<T_MD_NUMBER_OF_PROJECT, NumberOfProjectRepo>
    {
        public NumberOfProjectService() : base()
        {

        }

        public override void Create()
        {
            try
            {
                this.ObjDetail.CODE = Guid.NewGuid().ToString();
                this.ObjDetail.ACTIVE_TEXT = false;
                base.Create();
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void ToggleActiveText(object id)
        {
            try
            {
                this.Get(id);
                UnitOfWork.BeginTransaction();

                if (ProfileUtilities.User != null)
                {
                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                }

                this.ObjDetail.ACTIVE_TEXT = !this.ObjDetail.ACTIVE_TEXT;
                this.ObjDetail = this.CurrentRepository.Update(this.ObjDetail);

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
