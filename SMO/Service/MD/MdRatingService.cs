using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using System;
using System.Linq;

namespace SMO.Service.MD
{
    public class MdRatingService : GenericService<T_MD_RATING, MdRatingRepo>
    {
        public MdRatingService() : base()
        {

        }

        public override void Create()
        {
            if (this.CurrentRepository.Queryable().Count(x => x.FROM_VALUE == this.ObjDetail.FROM_VALUE && x.TO_VALUE == this.ObjDetail.TO_VALUE) > 0)
            {
                this.State = false;
                this.ErrorMessage = "Khoảng điểm đã tồn tại!";
                return;
            }
            if (this.ObjDetail.FROM_VALUE == this.ObjDetail.TO_VALUE)
            {
                this.State = false;
                this.ErrorMessage = "Điểm từ - Điểm đến không được trùng nhau!";
                return;
            }
            try
            {
                this.ObjDetail.PKID = Guid.NewGuid().ToString();
                this.ObjDetail.RATING = this.ObjDetail.RATING.ToUpper();
                base.Create();
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public override void Update()
        {
            if (this.CurrentRepository.Queryable().Count(x => x.PKID != this.ObjDetail.PKID && x.FROM_VALUE == this.ObjDetail.FROM_VALUE && x.TO_VALUE == this.ObjDetail.TO_VALUE) > 0)
            {
                this.State = false;
                this.ErrorMessage = "Khoảng điểm đã tồn tại!";
                return;
            }
            if (this.ObjDetail.FROM_VALUE == this.ObjDetail.TO_VALUE)
            {
                this.State = false;
                this.ErrorMessage = "Điểm từ - Điểm đến không được trùng nhau!";
                return;
            }
            try
            {
                this.ObjDetail.RATING = this.ObjDetail.RATING.ToUpper();
                base.Update();
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }

        public void ToggleBlackList(object id)
        {
            try
            {
                this.Get(id);
                UnitOfWork.BeginTransaction();

                if (ProfileUtilities.User != null)
                {
                    this.ObjDetail.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                }

                this.ObjDetail.IS_BLACK_LIST = !this.ObjDetail.IS_BLACK_LIST;
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