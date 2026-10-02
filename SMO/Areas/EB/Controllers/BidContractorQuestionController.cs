using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SMO.Areas.EB.Controllers
{
    public class BidContractorQuestionController : Controller
    {
        private BidContractorQuestionService _service;

        public BidContractorQuestionController()
        {
            _service = new BidContractorQuestionService();
        }

        public ActionResult Index(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            return PartialView(_service);
        }

        public ActionResult List(BidContractorQuestionService service)
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
        public ActionResult ListManager(BidContractorQuestionService service)
        {
            service.Search();
            return PartialView(service);
        }

        public ActionResult CreateQuestion(string bidId, string contractorId)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.ObjDetail.TYPE = ProfileUtilities.User.USER_TYPE;
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult CreateQuestion(BidContractorQuestionService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateQuestion(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "SubmitIndexContractorQuestion();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        public ActionResult CreateAnswer(string questionId)
        {
            var serviceAnswer = new BidContractorAnswerService();
            serviceAnswer.ObjDetail.QUESTION_ID = questionId;
            return PartialView(serviceAnswer);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAnswer(BidContractorAnswerService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateAnswer(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "SubmitIndexContractorQuestion();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        public ActionResult CreateEmail(string bidId, string typeEmail)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.ObjDetail.TYPE_EMAIL = typeEmail;
            return PartialView(_service);
        }

        public ActionResult CreateEmailBidWin(string bidId, string typeEmail)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.ObjDetail.TYPE_EMAIL = typeEmail;
            return PartialView(_service);
        }

        public ActionResult CreateEmailThanks(string bidId, string typeEmail)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.ObjDetail.TYPE_EMAIL = typeEmail;
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEmail(string listContractor, string bidId, string subject, string content, string typeEmail)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.CreateEmail(listContractor, bidId, subject, content, typeEmail);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = "$('#btnClose').click();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R417")]
        public ActionResult ViewListEmail(string bidId, string type)
        {
            _service.GetListEmail(bidId, type);
            return PartialView(_service);
        }
    }
}