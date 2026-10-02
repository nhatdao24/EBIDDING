using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class ReviewRatingController : Controller
    {
        private ReviewRatingService _service;

        public ReviewRatingController()
        {
            _service = new ReviewRatingService();
        }

        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index(string reviewId)
        {
            _service.ObjDetail.REVIEW_ID = reviewId;
            _service.Search();
            return PartialView(_service);
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult CreateOrUpdate(ReviewRatingService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateOrUpdate();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }
    }
}