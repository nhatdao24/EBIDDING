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
    public class ContractorFinanceService : GenericService<T_EB_CONTRACTOR_FINANCE, ContractorFinanceRepo>
    {
        public bool IsViewOnly { get; set; }
        public ContractorFinanceService() : base()
        {

        }

        public void Create(HttpRequestBase Request)
        {
            try
            {
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

                if (lstFileStream.Count > 0)
                {
                    this.ObjDetail.FILE_ID = lstFileStream[0].REFERENCE_ID;
                }

                this.ObjDetail.PKID = Guid.NewGuid().ToString();
                base.Create();
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                this.State = false;
            }
        }

        public void Update(HttpRequestBase Request)
        {
            try
            {
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

                if (lstFileStream.Count > 0)
                {
                    this.ObjDetail.FILE_ID = lstFileStream[0].REFERENCE_ID;
                }

                base.Update();
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                this.State = false;
            }
        }

        public override void Create()
        {
            this.ObjDetail.PKID = Guid.NewGuid().ToString();
            base.Create();
        }
    }
}
