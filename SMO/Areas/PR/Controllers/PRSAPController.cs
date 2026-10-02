using SMO.Service.MD;
using SMO.Service.PR;
using System;
using System.Dynamic;
using System.Web.Mvc;

namespace SMO.Areas.PR.Controllers
{
    [AuthorizeCustom(Right = "R301")]
    public class PRSAPController : Controller
    {
        private PRSAPService _service;

        public PRSAPController()
        {
            _service = new PRSAPService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index()
        {
            return PartialView(_service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult List(PRSAPService service)
        {
            service.Search();
            return PartialView(service);
        }

        /// <summary>
        /// Chức năng xem chi tiết đơn hàng của user cửa hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyValidateAntiForgeryToken]
        public ActionResult Detail(string id)
        {
            dynamic param = new ExpandoObject();
            param.IsFetch = true;
            _service.Get(id);
            return PartialView(_service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public virtual ActionResult Synchronize()
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