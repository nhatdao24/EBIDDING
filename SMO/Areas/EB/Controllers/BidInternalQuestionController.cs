using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SMO.Areas.EB.Controllers
{
    public class BidInternalQuestionController : Controller
    {
        private BidInternalQuestionService _service;

        public BidInternalQuestionController()
        {
            _service = new BidInternalQuestionService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            return PartialView(_service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult List(BidInternalQuestionService service)
        {
            service.Search();
            return PartialView(service);
        }

        public ActionResult CreateQuestion(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult CreateQuestion(BidInternalQuestionService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateQuestion(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "SubmitIndexInternalQuestion();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        public ActionResult CreateAnswer(string questionId, string userReply, string bidId, string parentId, bool isSendMail = false)
        {
            var serviceAnswer = new BidInternalAnswerService();
            serviceAnswer.ObjDetail.QUESTION_ID = questionId;
            serviceAnswer.ObjDetail.USER_REPLY = userReply;
            serviceAnswer.ObjDetail.IS_SEND_MAIL = isSendMail;
            serviceAnswer.ObjDetail.BID_ID = bidId;
            serviceAnswer.ObjDetail.PARENT_ID = parentId;
            return PartialView(serviceAnswer);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAnswer(BidInternalAnswerService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateAnswer(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "SubmitIndexInternalQuestion();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        public ActionResult CreateEmail(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEmail(string listCouncil, string bidId, string subject, string content)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.CreateEmail(listCouncil, bidId, subject, content, Request);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = "$('#btnClose').click();SubmitIndexInternalQuestion();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        public ActionResult ViewListEmail(string bidId)
        {
            _service.GetListEmail(bidId);
            return PartialView(_service);
        }
    }
}