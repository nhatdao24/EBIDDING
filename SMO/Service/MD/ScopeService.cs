using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.MD;
using System;
using System.Linq;

namespace SMO.Service.MD
{
    public class ScopeService : GenericService<T_MD_SCOPE, ScopeRepo>
    {

        public ScopeService() : base()
        {

        }
        private bool IsCodeDuplicate(string code)
        {
            var existingScopeMaterials = UnitOfWork.Repository<ScopeRepo>()
                     .GetAll() // Giả định có phương thức GetAll để lấy tất cả
                     .Where(sm => sm.CODE == this.ObjDetail.CODE)
                     .Select(sm => sm.CODE)
                     .ToList();
            return existingScopeMaterials.Any(existingCode => existingCode.Equals(code, StringComparison.OrdinalIgnoreCase));
        }

        public override void Create()
        {
            try
            {
                if (IsCodeDuplicate(this.ObjDetail.CODE))
                {
                    this.State = false;
                    this.ErrorMessage = "Mã CODE đã tồn tại !";
                    return; 
                }
            }
            catch (Exception ex)
            {
                this.State = false;
                this.ErrorMessage = ex.Message;
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