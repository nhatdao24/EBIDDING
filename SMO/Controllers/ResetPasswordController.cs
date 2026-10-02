using SMO.Service.AD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Controllers
{
    public class ResetPasswordController : Controller
    {
        private ResetPasswordService _service;
        public ResetPasswordController()
        {
            _service = new ResetPasswordService();
        }
        // GET: ResetPassword
        public ActionResult Index()
        {
            return View(_service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ResetPasswordService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.JsCommand;
            service.CheckMail();
            if (service.State)
            {
                result.ExtData = "SendMailSuccess();";
            }
            else
            {
                if (service.ErrorMessage == "01")
                {
                    result.ExtData = "EmailSendingFail();";
                }
                else if (service.ErrorMessage == "02")
                {
                    result.ExtData = "AlreadyRequested();";
                }
            }
            return result.ToJsonResult();
        }

        public ActionResult ResetPass(string id)
        {
            var result = new TransferObject();
            result.Type = TransferType.JsCommand;
            if (!string.IsNullOrWhiteSpace(id))
            {
                _service.CheckRequest(id);
            }
            return PartialView(_service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPass(ResetPasswordService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.JsCommand;
            service.ResetPass();
            if (service.State)
            {
                result.ExtData = "SavePassSuccess();";
            }
            else
            {
                if (service.ErrorMessage == "05")
                {
                    result.ExtData = "ErrorPassword();";
                }
                else if (service.ErrorMessage == "06")
                {
                    result.ExtData = "SavePassFail();";
                }
            }
            return result.ToJsonResult();
        }
    }
}