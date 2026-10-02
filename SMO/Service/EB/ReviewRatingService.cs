using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.EB
{
    public class ReviewRatingService : GenericService<T_EB_REVIEW_RATING, ReviewRatingRepo>
    {
        public ReviewRatingService() : base()
        {
        }

        public void CreateOrUpdate()
        {
            try
            {

                UnitOfWork.BeginTransaction();
                foreach (var item in this.ObjList.Where(x => x.PKID != null && x.PKID != "").ToList())
                {
                    item.UPDATE_BY = ProfileUtilities.User.USER_NAME;
                    this.CurrentRepository.Update(item);
                }

                var lstCurrent = this.CurrentRepository.Queryable().Where(x => x.REVIEW_ID == this.ObjDetail.REVIEW_ID).ToList();
                var lstID = this.ObjList.Where(x => x.PKID != null && x.PKID != "").Select(x => x.PKID).ToList();
                var lstDelete = lstCurrent.Where(x => !lstID.Contains(x.PKID)).ToList();
                this.CurrentRepository.Delete(lstDelete);
                foreach (var item in this.ObjList.Where(x => x.PKID == null || x.PKID == "").ToList())
                {
                    item.PKID = Guid.NewGuid().ToString();
                    item.CREATE_BY = ProfileUtilities.User.USER_NAME;
                    this.CurrentRepository.Create(item);
                }
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
