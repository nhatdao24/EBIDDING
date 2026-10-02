using SMO.Service.WF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SMO.Areas.WF.Controllers
{
    public class WorkFlowController : Controller
    {
        private WorkFlowService _service;

        public WorkFlowController()
        {
            _service = new WorkFlowService();
        }

        public ActionResult Index()
        {
            return PartialView(_service);
        }

        public ActionResult BuildTree(string idSelected)
        {
            var lstNode = _service.BuildTree();
            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            oSerializer.MaxJsonLength = int.MaxValue;
            ViewBag.zNode = oSerializer.Serialize(lstNode);
            ViewBag.IdSelected = idSelected;
            return PartialView();
        }

        public ActionResult CreateProcess()
        {
            return PartialView(_service);
        }

        public ActionResult CreateActivity(string idParent)
        {
            _service.ObjActivity.FK_PROCESS = idParent;
            return PartialView(_service);
        }

        public ActionResult CreateLevel(string idParent)
        {
            _service.ObjLevel.FK_ACTIVITY = idParent;
            return PartialView(_service);
        }

        public ActionResult CreateSender(string idParent)
        {
            _service.ObjSender.FK_LEVEL = idParent;
            return PartialView(_service);
        }

        public ActionResult CreateReceiver(string idParent)
        {
            _service.ObjReceiver.FK_SENDER = idParent;
            return PartialView(_service);
        }

        public ActionResult EditProcess(string id)
        {
            _service.GetProcessById(id);
            return PartialView(_service);
        }

        public ActionResult EditActivity(string id)
        {
            _service.GetActivityById(id);
            return PartialView(_service);
        }

        public ActionResult EditLevel(string id)
        {
            _service.GetLevelById(id);
            return PartialView(_service);
        }

        public ActionResult EditSender(string id)
        {
            _service.GetSenderById(id);
            return PartialView(_service);
        }

        public ActionResult EditReceiver(string id)
        {
            _service.GetReceiverById(id);
            return PartialView(_service);
        }

        public ActionResult EditCom(string id)
        {
            _service.GetComById(id);
            return PartialView(_service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProcess(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateProcess();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("BuildTree('{0}');", service.ObjProcess.CODE);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProcess(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.UpdateProcess();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("BuildTree('{0}');", service.ObjProcess.CODE);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateActivity(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateActivity();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("BuildTree('{0}');", service.ObjActivity.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateActivity(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.UpdateActivity();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("BuildTree('{0}');", service.ObjActivity.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateLevel(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateLevel();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("BuildTree('{0}');", service.ObjActivity.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateLevel(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.UpdateLevel();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("BuildTree('{0}');", service.ObjActivity.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSender(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateSender();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("BuildTree('{0}');", service.ObjSender.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSender(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.UpdateSender();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("BuildTree('{0}');", service.ObjSender.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReceiver(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateReceiver();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("BuildTree('{0}');", service.ObjReceiver.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateReceiver(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.UpdateReceiver();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = string.Format("BuildTree('{0}');", service.ObjReceiver.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCom(WorkFlowService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.UpdateCom();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                //result.ExtData = string.Format("BuildTree('{0}');", service.ObjCom.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }
    }
}