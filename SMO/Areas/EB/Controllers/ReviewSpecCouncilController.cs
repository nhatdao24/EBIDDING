using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class ReviewSpecCouncilController : Controller
    {
        private ReviewSpecCouncilService _service;

        public ReviewSpecCouncilController()
        {
            _service = new ReviewSpecCouncilService();
        }

        [MyValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R905")]
        public ActionResult Review(string reviewId)
        {
            _service.GetListSpecCouncil(reviewId);
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R905")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateReviewSpecCouncil(string id, string viewId, ReviewSpecCouncilService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.UpdateReviewSpecCouncil();
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                //result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Review", new { reviewId = id }), viewId);
                result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}',  htmlTarget: '#divContentDetail', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Review", new { reviewId = id }), viewId);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1003", service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R905")]
        [MyValidateAntiForgeryToken]
        public ActionResult EndReview(string id, string viewId, ReviewSpecCouncilService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.EndReview(id);
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}',  htmlTarget: '#divContentDetail', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Review", new { reviewId = id }), viewId);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1003", service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R905")]
        [MyValidateAntiForgeryToken]
        public ActionResult OpenReview(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.OpenReview(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}',  htmlTarget: '#divContentDetail', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Review", new { reviewId = id }), viewId);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1003", _service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R904")]
        [MyValidateAntiForgeryToken]
        public ActionResult Summary(string reviewId, string viewId, string lstContractor, string lstCouncil)
        {
            _service.GetSummaryReview(reviewId, lstContractor, lstCouncil);
            ViewBag.ReviewId = reviewId;
            ViewBag.ViewId = viewId;
            return PartialView(_service);
        }

        public ActionResult ExportExcelSumary(string reviewId)
        {
            string path = Server.MapPath("~/TemplateExcel/" + "ExportExcelSumary.xlsx");
            MemoryStream outFileStream = new MemoryStream();
            _service.Generate_ExportExcelSumary(ref outFileStream, path, reviewId);

            if (!_service.State)
            {
                return Content($"<script>alert('{_service.ErrorMessage}');</script>;");
            }

            var fileName = "BangDanhGia_" + reviewId;

            return File(outFileStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
        }

        [AuthorizeCustom(Right = "R904")]
        [MyValidateAntiForgeryToken]
        public ActionResult Report(string reviewId, string viewId, string lstContractor, string lstCouncil)
        {
            _service.GetSummaryReview(reviewId, lstContractor, lstCouncil);
            ViewBag.ReviewId = reviewId;
            ViewBag.ViewId = viewId;
            return PartialView(_service);
        }
    }
}