using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class ContractorProductController : Controller
    {
        private ContractorProductService _service;

        public ContractorProductController()
        {
            _service = new ContractorProductService();
        }

        [Authorize]
        public ActionResult Index(string contractorId)
        {
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult List(ContractorProductService service)
        {
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R702")]
        [MyValidateAntiForgeryToken]
        public ActionResult Edit(string contractorId)
        {
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            _service.Search();
            return PartialView(_service);
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R702")]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ContractorProductService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update();
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = "$('#btnClose').click();SubmitIndexProduct();";
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
    }
}