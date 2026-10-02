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
    public class BidNewService : GenericService<T_EB_BID_NEW, BidNewRepo>
    {
        public BidNewService() : base()
        {

        }

        public void Update(HttpPostedFileBase fileInput)
        {
            try
            {
                var bid = UnitOfWork.Repository<BidRepo>().Get(this.ObjDetail.BID_ID);
                bid.IS_PUBLIC = this.ObjDetail.IS_PUBLIC;

                UnitOfWork.BeginTransaction();
                if (fileInput != null && fileInput.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(fileInput.FileName); 
                    var uploadPath = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadImageNews/image"), fileName); 

                    fileInput.SaveAs(uploadPath); 

                    this.ObjDetail.IMAGE_PATH = $"/UploadImageNews/image/{fileName}"; 
                }


                UnitOfWork.Repository<BidRepo>().Update(bid);
                this.CurrentRepository.Update(this.ObjDetail);

                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.Exception = ex;
                this.State = false;
            }
        }
    }
}
