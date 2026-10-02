using SMO.Service.CM;
using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class ContractorController : Controller
    {
        private ContractorService _service;

        public ContractorController()
        {
            _service = new ContractorService();
        }

        [AuthorizeCustom(Right = "R701")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index()
        {
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R701")]
        [ValidateAntiForgeryToken]
        public ActionResult List(ContractorService service)
        {
            if (service.ObjDetail.IS_EXPORT_EXCEL)
            {
                MemoryStream outFileStream = new MemoryStream();
                service.ExportExcel(ref outFileStream);
                if (!service.State)
                {
                    return Content($"<script>alert('{service.ErrorMessage}');</script>;");
                }
                return File(outFileStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Danh_Sach_Nha_Thau.xlsx");
            }
            service.Search();
            ViewBag.FormID = service.FormId;
            return PartialView(service);
        }

        [Authorize]
        public ActionResult AddContractorBid(string bidId)
        {
            _service.BidId = bidId;
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult AddContractorReview(string reviewId)
        {
            _service.ReviewId = reviewId;
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult ListContractorForAddToReview(ContractorService service)
        {
            service.SearchForAddContractorReview();
            return PartialView(service);
        }


        [Authorize]
        public ActionResult ListContractorForAdd(ContractorService service)
        {
            service.SearchForAddContractorBid();
            return PartialView(service);
        }

        [Authorize]
        public ActionResult AddContractorToBid(string bidId, string lstContractor)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.AddContractorToBid(bidId, lstContractor);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = "SubmitListContractorForAdd(); SubmitListContractorOfBid();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        [Authorize]
        public ActionResult AddContractorToReview(string reviewId, string lstContractor)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.AddContractorToReview(reviewId, lstContractor);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = "SubmitListContractorForAdd(); SubmitListContractorOfReview();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R703")]
        [MyValidateAntiForgeryToken]
        public async Task<ActionResult> Detail(string id)
        {
            if (ProfileUtilities.User.USER_TYPE == UserType.NhaThau)
            {
                id = ProfileUtilities.User.CONTRACTOR_ID;
            }

            _service.Get(id);

            if (_service.ObjDetail != null && !string.IsNullOrEmpty(_service.ObjDetail.FileLogo.PKID))
            {
                try
                {
                    var fileService = new FileService();
                    var presignedUrl = await fileService.GetPresignedUrlAsync(_service.ObjDetail.FileLogo.PKID, 3600);

                    _service.ObjDetail.LOGO_PATH = presignedUrl;
                }
                catch (Exception ex)
                {
                    _service.ObjDetail.LOGO_PATH = "/Content/images/no-logo.png";
                }
            }

            return PartialView(_service);
        }

        [Authorize]
        public ActionResult ViewInfo(string id)
        {
            _service.Get(id);
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R702")]
        [MyValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, string viewIdDetail)
        {
            _service.Get(id);
            ViewBag.ViewIdDetail = viewIdDetail;
            if (_service.ObjDetail != null && !string.IsNullOrEmpty(_service.ObjDetail.FileLogo.PKID))
            {
                try
                {
                    var fileService = new FileService();
                    var presignedUrl = await fileService.GetPresignedUrlAsync(_service.ObjDetail.FileLogo.PKID, 3600);

                    _service.ObjDetail.LOGO_PATH = presignedUrl;
                }
                catch (Exception ex)
                {
                    _service.ObjDetail.LOGO_PATH = "/Content/images/no-logo.png";
                }
            }
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R702")]
        [MyValidateAntiForgeryToken]
        public async Task<ActionResult> Update(ContractorService service, string viewIdDetail)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            await service.Update(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = service.ObjDetail.PKID }), viewIdDetail);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }
    }
}