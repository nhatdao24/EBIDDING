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
    public class BidPhaseFileService : GenericService<T_EB_BID_PHASE_FILE, BidPhaseFileRepo>
    {
        public BidPhaseFileService() : base()
        {

        }

        public void CreateFileInSelectContractor(HttpRequestBase Request)
        {
            try
            {
                // Lưu file vào database
                var lstFileStream = new List<FILE_STREAM>();
                for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                {
                    var file = Request.Files[i];
                    var code = Guid.NewGuid().ToString();
                    var referenceCode = Guid.NewGuid().ToString();

                    lstFileStream.Add(new FILE_STREAM()
                    {
                        PKID = code,
                        FILE_OLD_NAME = file.FileName,
                        REFERENCE_ID = referenceCode,
                        FILE_EXT = Path.GetExtension(file.FileName),
                        FILE_SIZE = file.ContentLength,
                        FILESTREAM = Request.Files[i]
                    });
                }
                FileStreamService.InsertFile(lstFileStream);

                UnitOfWork.BeginTransaction();

                // Tạo file
                foreach (var file in lstFileStream)
                {
                    UnitOfWork.Repository<BidPhaseFileRepo>().Create(new T_EB_BID_PHASE_FILE()
                    {
                        BID_ID = this.ObjDetail.BID_ID,
                        FILE_ID = file.REFERENCE_ID,
                        PHASE = Phase_Status.ChonNhaThau,
                        CREATE_BY = ProfileUtilities.User.USER_NAME
                    });
                }

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
