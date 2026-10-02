using SMO.Service.AD;
using SMO.Service.CM;
using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class EBController : Controller
    {
        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult TopBidContractor()
        {
            var service = new BidService();
            service.NumerRecordPerPage = 10;
            service.Page = 1;
            service.SearchBidOfContractor();
            return PartialView(service);
        }

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult TopBidContractorWait()
        {
            var service = new BidService();
            service.NumerRecordPerPage = 10;
            service.Page = 1;
            service.SearchBidOfContractorWait();
            return PartialView(service);
        }

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult TopBidContractorOpen()
        {
            var service = new BidService();
            service.NumerRecordPerPage = 10;
            service.Page = 1;
            service.SearchBidOfContractorOpen();
            return PartialView(service);
        }

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult TopBid()
        {
            var service = new BidService();
            service.NumerRecordPerPage = 10;
            service.Page = 1;
            service.SearchTopBid();
            if (service.ObjList.Count == 0)
            {
                service.Search();
            }
            return PartialView(service);
        }

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult TopRequest()
        {
            var service = new RequestService();
            service.NumerRecordPerPage = 50;
            service.Page = 1;
            service.ObjDetail.STATUS = EB_Status.KhoiTao;
            service.Search();
            return PartialView(service);
        }

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult TopQuestionContractor()
        {
            var service = new BidContractorQuestionService();
            var lstBid = service.TopQuestionContractor();
            ViewBag.ListBid = lstBid;
            return PartialView(service);
        }

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult TopQuestion()
        {
            var service = new BidContractorQuestionService();
            var lstBid = service.TopQuestion();
            ViewBag.ListBid = lstBid;
            return PartialView(service);
        }

        public async Task<ActionResult> DownloadFile(string id, string isViewFile = "0")
        {
            try
            {
                // 1. Lấy thông tin file từ DB
                var serviceFile = new FileUploadService();
                serviceFile.Get(id);

                if (serviceFile.ObjDetail == null)
                {
                    return Content("<script>alert('Không tìm thấy file!');</script>");
                }

                string fileName = serviceFile.ObjDetail.PKID;    
                string oldName = serviceFile.ObjDetail.FILE_OLD_NAME; 

                // 2. Lấy Stream từ MinIO
                var minioService = new FileService();
                MemoryStream fileStream = null;

                if (!string.IsNullOrEmpty(fileName))
                {
                    // QUAN TRỌNG: Phải gán vào biến fileStream
                    fileStream = await minioService.DownloadFileAsync(fileName);
                }

                if (fileStream == null)
                {
                    return Content("<script>alert('File không tồn tại trên hệ thống lưu trữ!');</script>");
                }

                // 3. Xác định Content Type (MIME)
                string contentType = MimeMapping.GetMimeMapping(oldName ?? fileName);

                // 4. Trả file về trình duyệt
                if (isViewFile == "1")
                {
                    // Chế độ xem trực tiếp (Inline)
                    var cd = new System.Net.Mime.ContentDisposition
                    {
                        Inline = true,
                        FileName = oldName
                    };
                    Response.Headers.Add("Content-Disposition", cd.ToString());
                    return File(fileStream, contentType);
                }

                // Chế độ tải về (Attachment)
                return File(fileStream, contentType, oldName);
            }
            catch (Exception ex)
            {
                // Log lỗi ex nếu cần
                return Content("<script>alert('Download file bị lỗi!');</script>");
            }
        }

        [Authorize]
        public async Task<ActionResult> ViewFileOnline(string id)
        {
            try
            {
                var serviceFile = new FileUploadService();
                serviceFile.Get(id);

                if (serviceFile.ObjDetail == null)
                {
                    return Content("<script>alert('Không tìm thấy thông tin file!'); window.close();</script>");
                }

                var minioService = new FileService();
                string presignedUrl = await minioService.GetPresignedUrlAsync(serviceFile.ObjDetail.PKID, 3600);
                string ext = (serviceFile.ObjDetail.FILE_EXT ?? "").Replace(".", "").ToLower();

                ViewBag.PresignedUrl = presignedUrl;
                ViewBag.FileId = id;
                ViewBag.FileName = serviceFile.ObjDetail.FILE_OLD_NAME;
                ViewBag.FileExt = ext;
                ViewBag.OnlyOfficeApiUrl = "https://smr.d2s.com.vn:1010/web-apps/apps/api/documents/api.js";

                var officeExtensions = new List<string> { "doc", "docx", "xls", "xlsx", "ppt", "pptx", "txt", "pdf" };
                ViewBag.IsOffice = officeExtensions.Contains(ext);

                return View(serviceFile);
            }
            catch (Exception ex)
            {
                return Content($"<script>alert('Lỗi: {ex.Message}'); window.close();</script>");
            }
        }

        [Authorize]
        public ActionResult MoveFileToTemp(string id)
        {
            var result = new TransferObject();
            result.Type = TransferType.JsCommand;
            try
            {
                var serviceFile = new FileUploadService();
                serviceFile.Get(id);

                var serviceConnection = new ConnectionService();
                serviceConnection.Get(serviceFile.ObjDetail.CONNECTION_ID);

                var filePath = Path.Combine(serviceConnection.ObjDetail.DIRECTORY.TrimEnd(System.IO.Path.DirectorySeparatorChar), serviceFile.ObjDetail.DIRECTORY_PATH.TrimStart(System.IO.Path.DirectorySeparatorChar), serviceFile.ObjDetail.REFERENCE_ID);
                if (!System.IO.File.Exists(filePath))
                {
                    result.State = false;
                    result.Message.Message = "Đường dẫn tới file không tồn tại!";
                }
                else
                {
                    var filePathTemp = Path.Combine("\\TempViewFile", serviceFile.ObjDetail.REFERENCE_ID);
                    if (!System.IO.File.Exists(HostingEnvironment.MapPath(filePathTemp)))
                    {
                        System.IO.File.Copy(filePath, HostingEnvironment.MapPath(filePathTemp));
                    }

                    result.State = true;
                    result.ExtData = filePathTemp.Replace('\\','/');
                }
            }
            catch (Exception ex)
            {
                result.State = false;
                result.Message.Message = "Quá trình xem file bị lỗi!";
                result.Message.Detail = ex.ToString();
            }
            return result.ToJsonResult();   
        }
    }
}