using SMO.Core.Entities;
using SMO.Repository.Implement.CM;
using SMO.Service.AD;
using SMO.Service.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SMO.Service.CM
{
    public class FileUploadService : GenericService<T_CM_FILE_UPLOAD, FileUploadRepo>
    {
        private readonly FileService _minioService;
        public FileUploadService() : base()
        {
            _minioService = new FileService();
        }

        //public async Task Create(List<FILE_STREAM> lstFile)
        //{
        //    try
        //    {
        //        var systemConfigService = new SystemConfigService();
        //        systemConfigService.GetConfig();

        //        if (systemConfigService.ObjDetail.Connection == null)
        //            throw new Exception("Chưa cấu hình đầy đủ thông tin lưu trữ file!");

        //        var userName = ProfileUtilities.User?.USER_NAME ?? "newUser";
        //        UnitOfWork.BeginTransaction();

        //        foreach (var item in lstFile)
        //        {
        //            if (item.FILESTREAM == null || item.FILESTREAM.ContentLength == 0) continue;
        //            var safePKID = string.IsNullOrEmpty(item.PKID) ? Guid.NewGuid().ToString() : item.PKID;

        //            // Upload lên MinIO
        //            string objectName = await _minioService.UploadFileAsync(
        //                item.FILESTREAM.InputStream,
        //                item.FILE_OLD_NAME,
        //                item.FILESTREAM.ContentType
        //            );

        //            // Lưu vào Database
        //            var fileUpload = new T_CM_FILE_UPLOAD()
        //            {
        //                PKID = safePKID,
        //                CONNECTION_ID = systemConfigService.ObjDetail.CURRENT_CONNECTION,
        //                FILE_NAME = objectName, // Tên file trên MinIO
        //                FILE_OLD_NAME = item.FILE_OLD_NAME, // Tên gốc người dùng thấy
        //                FILE_EXT = item.FILE_EXT,
        //                FILE_SIZE = item.FILE_SIZE,
        //                DIRECTORY_PATH = item.DIRECTORY_PATH,
        //                CREATE_BY = userName,
        //                CREATE_DATE = DateTime.Now
        //            };

        //            this.CurrentRepository.Create(fileUpload);
        //        }
        //        UnitOfWork.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        UnitOfWork.Rollback();
        //        throw new Exception("Lỗi hệ thống khi lưu file: " + ex.Message, ex);
        //    }
        //}

        public async Task Create(List<FILE_STREAM> lstFile)
        {
            try
            {
                var systemConfigService = new SystemConfigService();
                systemConfigService.GetConfig();
                var userName = ProfileUtilities.User?.USER_NAME ?? "newUser";
                var connectionId = systemConfigService.ObjDetail.CURRENT_CONNECTION;

                foreach (var item in lstFile)
                {
                    if (item.FILESTREAM == null || item.FILESTREAM.ContentLength == 0) continue;

                    var fileUpload = new T_CM_FILE_UPLOAD()
                    {
                        PKID = string.IsNullOrEmpty(item.PKID) ? Guid.NewGuid().ToString() : item.PKID,
                        REFERENCE_ID = string.IsNullOrEmpty(item.REFERENCE_ID) ? Guid.NewGuid().ToString() : item.REFERENCE_ID,
                        CONNECTION_ID = connectionId,
                        FILE_OLD_NAME = item.FILE_OLD_NAME,
                        FILE_EXT = item.FILE_EXT,
                        FILE_SIZE = item.FILE_SIZE,
                        CREATE_BY = userName,
                        CREATE_DATE = DateTime.Now
                    };

                    var objectName = fileUpload.PKID.ToString();
                    this.CurrentRepository.Create(fileUpload);
                    this.UnitOfWork.Flush();

                    byte[] fileData;
                    using (var ms = new MemoryStream())
                    {
                        item.FILESTREAM.InputStream.Position = 0;
                        item.FILESTREAM.InputStream.CopyTo(ms);
                        fileData = ms.ToArray();
                    }

                    await _minioService.UploadFileAsync(fileData, objectName);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
