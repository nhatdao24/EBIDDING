using SMO.Service.AD;
using SMO.Service.Class;
using SMO.Service.CM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace SMO.Service.EB
{
    public class FileStreamService
    {
        public static void DeleteFile(string lstFile)
        {
            var systemConfigService = new SystemConfigService();
            systemConfigService.GetConfig();

            if (systemConfigService.ObjDetail.Connection == null || string.IsNullOrWhiteSpace(systemConfigService.ObjDetail.Connection.DIRECTORY))
            {
                throw new Exception("Chưa cấu hình đầy đủ thông tin lưu trữ file!");
            }

            if (string.IsNullOrWhiteSpace(lstFile))
            {
                return;
            }

            //TODO: Xóa file vào thư mục cache

            //Xóa file ở database file
            var strConnection = $"Server={systemConfigService.ObjDetail.Connection.ADDRESS};Database={systemConfigService.ObjDetail.CURRENT_DATABASE_NAME};user id={systemConfigService.ObjDetail.Connection.USERNAME};password={systemConfigService.ObjDetail.Connection.PASSWORD};";
            try
            {


                var strInsert = @"DELETE TB_DATA_FILE WHERE PKID = @PKID";

                using (TransactionScope ts = new TransactionScope())
                {
                    using (SqlConnection conn = new SqlConnection(strConnection))
                    {

                        conn.Open();
                        foreach (var file in lstFile.Split('|'))
                        {
                            using (SqlCommand cmd = new SqlCommand(strInsert, conn))
                            {
                                cmd.CommandTimeout = 1800;
                                cmd.Parameters.Add("@PKID", SqlDbType.NVarChar).Value = file;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Xóa file ở database file không thành công!", ex);
            }

            var fileUploadService = new FileUploadService();
            fileUploadService.Delete(lstFile);

            if (fileUploadService.State == false)
            {
                throw fileUploadService.Exception;
            }
        }

        public static async Task InsertFile(List<FILE_STREAM> lstFile)
        {
            // Kiểm tra đầu vào cơ bản
            if (lstFile == null || lstFile.Count == 0)
            {
                return;
            }

            // Khởi tạo service xử lý upload MinIO và lưu DB File
            var fileUploadService = new FileUploadService();

            // Gọi hàm xử lý chính
            await fileUploadService.Create(lstFile);

            // Kiểm tra trạng thái nếu service có cơ chế lưu State/Exception riêng
            if (fileUploadService.State == false)
            {
                throw fileUploadService.Exception;
            }
        }

        public static string CreatePath(string parentPath)
        {
            var path = "";
            try
            {
                var now = DateTime.Now;
                path = Path.Combine(parentPath, now.Year.ToString(), now.Month.ToString(), now.Day.ToString());
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return path;
        }
    }
}