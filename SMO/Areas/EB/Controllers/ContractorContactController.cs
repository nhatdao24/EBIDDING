using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class ContractorContactController : Controller
    {
        private ContractorContactService _service;

        public ContractorContactController()
        {
            _service = new ContractorContactService();
        }

        [Authorize]
        public ActionResult Index(string contractorId)
        {
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult List(ContractorContactService service)
        {
            service.Search();
            return PartialView(service);
        }

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(string contractorId)
        {
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            return PartialView(_service);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ContractorContactService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Create();
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = "SubmitIndexContact();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }

        [Authorize]
        public ActionResult Detail(string id)
        {
            _service.Get(id);
            return PartialView(_service);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ContractorContactService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update();
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = "SubmitIndexContact();";
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