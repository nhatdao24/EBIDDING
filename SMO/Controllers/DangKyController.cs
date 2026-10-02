using SMO.Service;
using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.Controllers
{
    public class DangKyController : Controller
    {
        private RequestService _service;

        public DangKyController()
        {
            _service = new RequestService();
        }

        public ActionResult Index()
        {
            return View(_service);
        }

        public ActionResult DangKy(RequestService service)
        {
            var lang = "vi";
            if (Request.Cookies["lang"] != null)
            {
                lang = Request.Cookies["lang"].Value;
            }

            if (lang != "vi" && lang != "en")
            {
                lang = "vi";
            }

            var result = new TransferObject();
            result.Type = TransferType.JsCommand;
            result.State = true;

            var ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            //var isValidate = new RecaptchaValidationService(ConfigurationManager.AppSettings["RecaptchaSecretKey"]).Validate(System.Web.HttpContext.Current.Request["g-recaptcha-response"], ip);

            //if (!isValidate)
            //{
            //    result.State = false;
            //    var errorMessage = "Capcha không chính xác! Có thể bạn chưa chọn : Tôi không phải là người máy";
            //    if (lang != "vi")
            //    {
            //        errorMessage = "Capcha not valid! You must check: Im not a robot";
            //    }
            //    result.ExtData = $"DangKyKhongThanhCong('{errorMessage}');";
            //}
            //else
            //{
                service.ObjDetail.PKID = Guid.NewGuid().ToString();
                service.Create(lang, Request);
                if (service.State)
                {
                    result.ExtData = "DangKyThanhCong();";
                }
                else
                {
                    result.ExtData = $"DangKyKhongThanhCong('{service.ErrorMessage}');";
                }
            //}

            return result.ToJsonResult();
        }
    }
}