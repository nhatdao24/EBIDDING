using System.Web.Mvc;
using System.Web.Script.Serialization;
using NPOI.SS.Formula.Functions;
using SMO.Service.AD;
using SMO.Service.MD;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SMO.Areas.MD.Controllers
{
    [AuthorizeCustom(Right = "R294")]
    public class ProjectController : Controller
    {
        private ProjectService _service;

        public ProjectController()
        {
            _service = new ProjectService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index()
        {
            return PartialView(_service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult List(ProjectService service)
        {
            service.Search();
            return PartialView(service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Create()
        {
            return PartialView(_service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProjectService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Create();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "SubmitIndex();";
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ProjectService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update();
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = "SubmitIndex();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }

        public ActionResult GetUserProject(string projectCode)
        {
            _service.ObjListUserProject = _service.SearchListUserProject(projectCode);
            return PartialView(_service);
        }

        public ActionResult ListUserProject(string ProjectCode)
        {

            ViewBag.ProjectCode = ProjectCode;
            return PartialView(_service);
        }

        public ActionResult RightUserProject(string ProjectCode)
        {

            ViewBag.ProjectCode = ProjectCode;
            return PartialView(_service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult AddUserProject(string ProjectCode)
        {
            ViewBag.ProjectCode = ProjectCode;
            return PartialView(_service);
        }
        [HttpPost]
        public ActionResult ListUserProjectForAdd(ProjectService service, string ProjectCode)
        {
            string userName = service.SearchUserName;
            service.ObjUser = service.SearchUserProjectForAdd(ProjectCode, userName);
            return PartialView(service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult AddUserProjectToUser(string lstUserProject, string ProjectCode)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.AddUserProjectToUser(lstUserProject, ProjectCode);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = string.Format("SubmitGetUserProject();GetListUserOfProject();SubmitListUserProjectForAdd()");

            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult DeleteUserOfProject(string lstUserProject, string ProjectCode)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.DeleteUserOfProject(lstUserProject, ProjectCode);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = string.Format("SubmitGetUserProject();GetListUserOfProject()");

            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }
        public ActionResult BuildTreeRight(string userName, string projectCode)
        {
            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            var lstNode = _service.BuildTreeRight(userName, projectCode);
            oSerializer.MaxJsonLength = int.MaxValue;
            ViewBag.zNode = oSerializer.Serialize(lstNode);
            return PartialView();
        }
        public ActionResult GetListUserOfProject(string projectCode)
        {
            _service.ObjListUserProject = _service.SearchListUserProject(projectCode);
            return PartialView(_service);
        }
        [HttpPost]
        [MyValidateAntiForgeryToken]

        public ActionResult UpdateRightProJectOfUser(string userName, string rightList, string statusList, string projectCode)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.UpdateRightProJectOfUser(userName, rightList, statusList, projectCode);
            if (_service.State)
            {

                SMOUtilities.GetMessage("1001", _service, result);
                //result.ExtData = string.Format("BuildTreeRight()");

            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }
    }
}