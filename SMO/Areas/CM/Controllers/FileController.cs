using SMO.Service.CM;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SMO.Controllers.CM
{
    public class FileController : Controller
    {
        private readonly FileService _fileService;

        public FileController()
        {
            _fileService = new FileService();
        }

        /// <summary>
        /// Upload single file
        /// </summary>
        //[HttpPost]
        //public async Task<JsonResult> Upload(HttpPostedFileBase file, string folder = null)
        //{
        //    try
        //    {
        //        if (file == null || file.ContentLength == 0)
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "Không có file được chọn"
        //            });
        //        }

        //        // Validate file size (giới hạn 500MB)
        //        const int maxFileSize = 500 * 1024 * 1024; // 500MB
        //        if (file.ContentLength > maxFileSize)
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "File vượt quá dung lượng cho phép (500MB)"
        //            });
        //        }

        //        // Validate file extension
        //        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".rar" };
        //        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        //        if (!allowedExtensions.Contains(fileExtension))
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "Định dạng file không được hỗ trợ"
        //            });
        //        }

        //        // Upload file
        //        var objectName = await _fileService.UploadFileAsync(
        //            file.InputStream,
        //            file.FileName,
        //            file.ContentType
        //        );

        //        return Json(new
        //        {
        //            success = true,
        //            message = "Upload file thành công",
        //            data = new
        //            {
        //                objectName = objectName,
        //                fileName = file.FileName,
        //                fileSize = file.ContentLength,
        //                contentType = file.ContentType
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = $"Lỗi upload file: {ex.Message}"
        //        });
        //    }
        //}

        /// <summary>
        /// Upload multiple files
        /// </summary>
        //[HttpPost]
        //public async Task<JsonResult> UploadMultiple(string folder = null)
        //{
        //    try
        //    {
        //        var files = Request.Files;

        //        if (files.Count == 0)
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "Không có file được chọn"
        //            });
        //        }

        //        var uploadedFiles = new System.Collections.Generic.List<object>();

        //        for (int i = 0; i < files.Count; i++)
        //        {
        //            var file = files[i];

        //            if (file != null && file.ContentLength > 0)
        //            {
        //                var objectName = await _fileService.UploadFileAsync(
        //                    file.InputStream,
        //                    file.FileName,
        //                    file.ContentType
        //                );

        //                uploadedFiles.Add(new
        //                {
        //                    objectName = objectName,
        //                    fileName = file.FileName,
        //                    fileSize = file.ContentLength
        //                });
        //            }
        //        }

        //        return Json(new
        //        {
        //            success = true,
        //            message = $"Upload thành công {uploadedFiles.Count} file",
        //            data = uploadedFiles
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = $"Lỗi upload files: {ex.Message}"
        //        });
        //    }
        //}

        /// <summary>
        /// Download file
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Download(string objectName)
        {
            try
            {
                if (string.IsNullOrEmpty(objectName))
                {
                    return Content("Object name không hợp lệ");
                }

                var fileStream = await _fileService.DownloadFileAsync(objectName);
                var fileName = Path.GetFileName(objectName);

                // Lấy content type dựa vào extension
                var contentType = GetContentType(fileName);

                return File(fileStream, contentType, fileName);
            }
            catch (Exception ex)
            {
                return Content($"Lỗi download file: {ex.Message}");
            }
        }

        /// <summary>
        /// Get presigned download URL
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDownloadUrl(string objectName, int expiryInSeconds = 3600)
        {
            try
            {
                if (string.IsNullOrEmpty(objectName))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Object name không hợp lệ"
                    }, JsonRequestBehavior.AllowGet);
                }

                var url = await _fileService.GetPresignedUrlAsync(objectName, expiryInSeconds);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        url = url,
                        expiryInSeconds = expiryInSeconds
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi tạo download URL: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetPresignedUrl(string fileId,int expiryInSeconds = 3600)
        {
            try
            {
                var fileService = new FileUploadService();
                fileService.Get(fileId);
                var objectName = fileService.ObjDetail.REFERENCE_ID;
                string url = string.Empty;
                if(objectName != null || !string.IsNullOrEmpty(objectName)) 
                { 
                url = await _fileService.GetPresignedUrlAsync(objectName, expiryInSeconds);
                }
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        url = url,
                        expiryInSeconds = expiryInSeconds
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi tạo download URL: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Delete file
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> Delete(string objectName)
        {
            try
            {
                if (string.IsNullOrEmpty(objectName))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Object name không hợp lệ"
                    });
                }

                await _fileService.DeleteFileAsync(objectName);

                return Json(new
                {
                    success = true,
                    message = "Xóa file thành công"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi xóa file: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Delete multiple files
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> DeleteMultiple(string[] objectNames)
        {
            try
            {
                if (objectNames == null || objectNames.Length == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không có file để xóa"
                    });
                }

                await _fileService.DeleteMultipleFilesAsync(objectNames);

                return Json(new
                {
                    success = true,
                    message = $"Xóa thành công {objectNames.Length} file"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi xóa files: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Check if file exists
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> Exists(string objectName)
        {
            try
            {
                var exists = await _fileService.FileExistsAsync(objectName);

                return Json(new
                {
                    success = true,
                    exists = exists
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi kiểm tra file: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get file metadata
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetMetadata(string objectName)
        {
            try
            {
                var metadata = await _fileService.GetFileMetadataAsync(objectName);

                return Json(new
                {
                    success = true,
                    data = metadata
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi lấy metadata: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Copy file
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> Copy(string sourceObjectName, string destinationObjectName)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceObjectName) || string.IsNullOrEmpty(destinationObjectName))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Object name không hợp lệ"
                    });
                }

                var newObjectName = await _fileService.CopyFileAsync(sourceObjectName, destinationObjectName);

                return Json(new
                {
                    success = true,
                    message = "Copy file thành công",
                    data = new
                    {
                        objectName = newObjectName
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi copy file: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Helper method to get content type from file extension
        /// </summary>
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            var contentTypes = new System.Collections.Generic.Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".zip", "application/zip" },
                { ".rar", "application/x-rar-compressed" },
                { ".txt", "text/plain" }
            };

            return contentTypes.ContainsKey(extension)
                ? contentTypes[extension]
                : "application/octet-stream";
        }
    }
}