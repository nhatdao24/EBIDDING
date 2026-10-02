using System.Web.Mvc;
using SMO.Service.MD;

namespace SMO.Areas.MD.Controllers
{
    [AuthorizeCustom(Right = "R290")]
    public class VendorController : Controller
    {
        private VendorService _service;

        public VendorController()
        {
            _service = new VendorService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index()
        {
            return PartialView(_service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult List(VendorService service)
        {
            service.Search();
            return PartialView(service);
        }

        public ActionResult CheckCreateAccount(string id)
        {
            var result = new TransferObject();
            _service.CheckCreateAccount(id);
            if (_service.State)
            {
                result.Type = TransferType.JsCommand;
                result.ExtData = string.Format("Forms.LoadAjax('{0}');", @Url.Action("CreateUserContractor", "User", new { @area = "AD", vendorId = id }));
            }
            else
            {
                result.Type = TransferType.AlertDangerAndJsCommand;
                result.ExtData = "SubmitIndex();";
                _service.ErrorMessage = "Tài khoản của nhà thầu này đã được tạo từ trước đó!";
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult Synchronize()
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.Synchronize();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "SubmitIndex();";
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