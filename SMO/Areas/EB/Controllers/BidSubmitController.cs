using SMO.Service.CM;
using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SMO.Areas.EB.Controllers
{
    public class BidSubmitController : Controller
    {
        private BidSubmitService _service;

        public BidSubmitController()
        {
            _service = new BidSubmitService();
        }

        [Authorize]
        public ActionResult Index(string bidId, bool isInputSpec = false)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.ObjDetail.IS_INPUT_SPEC = isInputSpec;
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult List(BidSubmitService service)
        {
            service.ObjDetail.CONTRACTOR_ID = ProfileUtilities.User.CONTRACTOR_ID;
            service.Search();
            return PartialView(service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult IndexManager(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            return PartialView(_service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult ListManager(BidSubmitService service)
        {
            if (service.ObjDetail.CONTRACTOR_ID == "NEW")
            {
                ViewBag.IS_NEW = "1";
                service.ObjDetail.CONTRACTOR_ID = "";
            }
            service.Search();
            return PartialView(service);
        }

        [Authorize]
        public ActionResult CreateSubmit(string bidId, string contractorId)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        // 1. Chuyển sang async Task
        public async Task<ActionResult> CreateSubmit(BidSubmitService service, string moTaKyThuat, string moTaNangLuc, string moTaGia, string moTaTongHop, string moTaPhapLy)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            // 2. Thêm await và đảm bảo hàm CreateSubmit trong service cũng là async
            await service.CreateSubmit(Request, moTaKyThuat, moTaNangLuc, moTaGia, moTaTongHop, moTaPhapLy);

            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = "try{{SubmitIndexSubmit();}}catch(e){{}}; try{{SubmitListFile();}}catch(e){{}};";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteSubmit(string id, string bidId)
        {
            var minioService = new FileService();
            var serviceBid = new BidService();
            serviceBid.Get(bidId);
            if (serviceBid.ObjDetail.GIAI_DOAN != Phase_Status.NopHoSo)
            {
                return Content("");
            }

            _service.Get(id);
            var submitData = _service.ObjDetail;
            if (_service.ObjDetail.CONTRACTOR_ID != ProfileUtilities.User.CONTRACTOR_ID)
            {
                return Content("");
            }
            if(submitData.ListFile != null)
            {
               foreach(var file in submitData.ListFile)
                {
                    if(file.FileUpload != null || !string.IsNullOrEmpty(file.FileUpload.REFERENCE_ID))
                    {
                        await minioService.DeleteFileAsync(file.FileUpload.REFERENCE_ID);
                    }
                }
            }
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.Delete(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }
    }
}