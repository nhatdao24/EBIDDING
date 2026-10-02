using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class ContractorFinanceController : Controller
    {
        private ContractorFinanceService _service;

        public ContractorFinanceController()
        {
            _service = new ContractorFinanceService();
        }

        [Authorize]
        public ActionResult Index(string contractorId)
        {
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult List(ContractorFinanceService service)
        {
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R702")]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(string contractorId)
        {
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            return PartialView(_service);
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R702")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ContractorFinanceService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Create(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = "SubmitIndexFinance();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R702")]
        public ActionResult Detail(string id)
        {
            _service.Get(id);
            return PartialView(_service);
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R702")]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ContractorFinanceService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = "SubmitIndexProject();";
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