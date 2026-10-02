using SMO.Service.AD;
using SMO.Service.MD;
using SMO.Service.PR;
using System;
using System.Dynamic;
using System.Web.Mvc;

namespace SMO.Areas.PR.Controllers
{
    public class PREBController : Controller
    {
        private PREBService _service; 

        public PREBController()
        {
            _service = new PREBService();
        }

        [AuthorizeCustom(Right = "R301")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index()
        {
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R301")] 
        [ValidateAntiForgeryToken]
        public ActionResult List(PREBService service, string lstPrCodeJson)
        { 
            service.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
            service.Search(lstPrCodeJson);
            return PartialView(service);
        }

         public ActionResult GetRentUnits()
        {
            var rentUnits = SelectListUtilities.GetRentUnit(); 
            return Json(rentUnits, JsonRequestBehavior.AllowGet); 
        }

        [AuthorizeCustom(Right = "R301")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index_CreateByMe()
        {
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R301")]
        [ValidateAntiForgeryToken]
        public ActionResult List_CreateByMe(PREBService service)
        {
            service.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
            service.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R301")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index_ProcessByMe()
        {
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R301")]
        [ValidateAntiForgeryToken]
        public ActionResult List_ProcessByMe(PREBService service)
        {
            service.ObjDetail.USER_APPROVE = ProfileUtilities.User.USER_NAME;
            service.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R302")]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(string projectCode)
        {
            _service.ObjHeader.REQUIRE_DATE = DateTime.Now;
            _service.ObjHeader.DESTINATION_DATE = DateTime.Now.AddDays(7);
            _service.ObjHeader.PROJECT_CODE = projectCode;
            //var serviceUserApprove = new UserApproveService();
            //_service.ObjHeader.USER_APPROVE = serviceUserApprove.GetUserApprove("PREB");

            return PartialView(_service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(PREBService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Create(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = $"SubmitIndex();CloseFormCreate();Forms.LoadAjax({{url:'{Url.Action("Detail", new { id = service.ObjHeader.CODE })}'}});";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        /// <summary>
        /// Chức năng chỉnh sửa PR
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyValidateAntiForgeryToken]
        public ActionResult Edit(string id, string viewIdDetail)
        {
            dynamic param = new ExpandoObject();
            param.IsFetch = true;
            _service.Get(id, param);

            //ConfigViettinBank: Xoá phân quyền dự án

            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP103", _service.ObjHeader.PROJECT_CODE);
            //if (!checkRight.Item1)
            //{
            //    ViewBag.Error = checkRight.Item2;
            //    return PartialView("Error");
            //}

            if (_service.ObjHeader.STATUS == EB_Status.KhoiTao || _service.ObjHeader.STATUS == EB_Status.ChoCapNhat)
            {
                ViewBag.ViewIdDetail = viewIdDetail;
                return PartialView(_service);
            }
            else
            {
                ViewBag.Error = "PR không ở trạng thái mới tạo";
                return PartialView("Error");
            }
        }

        /// <summary>
        /// Chức năng xem chi tiết PR
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyValidateAntiForgeryToken]
        public ActionResult Detail(string id)
        {
            dynamic param = new ExpandoObject();
            param.IsFetch = true;
            _service.Get(id);

            //ConfigViettinBank: Xoá phân quyền dự án

            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP104", _service.ObjHeader.PROJECT_CODE);
            //if (!checkRight.Item1)
            //{
            //    ViewBag.Error = checkRight.Item2;
            //    return PartialView("Error");
            //}

            return PartialView(_service);
        }


        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult Update(PREBService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                //result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = service.ObjHeader.CODE }), service.ViewId);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult AddMaterial()
        {
            var service = new MaterialService();
            return PartialView(service);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ListMaterialForAdd(MaterialService service)
        {
            service.Search();
            return PartialView(service);
        }

        public ActionResult Send(string code, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.SendPR(code);
            if (_service.State)
            {
                SMOUtilities.GetMessage("2001", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("2002", _service, result);
            }

            return result.ToJsonResult();
        }

        public ActionResult Cancel(string code, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.CancelPR(code);
            if (_service.State)
            {
                SMOUtilities.GetMessage("2003", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("2004", _service, result);
            }

            return result.ToJsonResult();
        }


        [MyValidateAntiForgeryToken]
        public ActionResult Approve(string id,string viewId)
        {
            _service.Get(id);
            ViewBag.ViewId = viewId;
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]

        public ActionResult Approve(string code, string viewId, string reason)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.ApprovePR(code, reason);
            if (_service.State)
            {
                SMOUtilities.GetMessage("2005", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');$('#btn_close_modal_approve_pr').click();}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("2006", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Reject(string id, string viewId)
        {
            _service.Get(id);
            ViewBag.ViewId = viewId;
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult Reject(string code, string viewId, string reason)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.RejectPR(code, reason);
            if (_service.State)
            {
                SMOUtilities.GetMessage("2007", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');$('#btn_close_modal_reject_pr').click();}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("2008", _service, result);
            }

            return result.ToJsonResult();
        }


        [MyValidateAntiForgeryToken]
        public ActionResult RequireUpdate(string id,string viewId)
        {
            _service.Get(id);
            ViewBag.ViewId = viewId;
            return PartialView(_service);

        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult RequireUpdate(string code, string viewId, string reason)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.RequireUpdate(code, reason);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');$('#btn_close_modal_requireUpdate_pr').click();}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
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