using SMO.Service.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SMO.Areas.MD.Controllers
{
    [AuthorizeCustom(Right = "R291")]
    public class MdBidSpecController : Controller
    {
        private MdBidSpecService _service;

        public MdBidSpecController()
        {
            _service = new MdBidSpecService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index(string bidId)
        {
            return PartialView(_service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult BuildTree(string specSelected, string bidId)
        {
            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            var lstNodeSpec = _service.BuildTreeSpec();
            ViewBag.zNodeSpec = oSerializer.Serialize(lstNodeSpec);
            ViewBag.SpecSelected = specSelected;
            return PartialView();
        }


        [MyValidateAntiForgeryToken]
        public ActionResult Create(string parent)
        {
            _service.ObjHeader.PARENT_ID = parent;
            _service.ObjHeader.TYPE = "NUMBER";
            return PartialView(_service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(MdBidSpecService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Create();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = string.Format("BuildTreeSpec('{0}', true);", service.ObjDetail.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Edit(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                _service.Get(id);
            }
            return PartialView(_service);
        }


        [MyValidateAntiForgeryToken]
        public ActionResult Update(MdBidSpecService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update();
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = string.Format("BuildTreeSpec('{0}', true);", service.ObjHeader.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult UpdateTree(List<NodeSpec> lstNode)
        {
            var result = new TransferObject();
            result.State = true;
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.UpdateTree(lstNode);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            result.State = true;
            _service.Delete(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = string.Format("BuildTreeSpec('', true);");
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