using Newtonsoft.Json;
using SMO.Service.AD;
using SMO.Service.EB;
using SMO.Service.MD;
using SMO.Service.PR;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization; 

namespace SMO.Areas.EB.Controllers
{
    public class BidRequireController : Controller
    {
        private BidRequireService _service;

        public BidRequireController()
        {
            _service = new BidRequireService();
        }

        [AuthorizeCustom(Right = "R601")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index(string bidId)
        {
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R601")]
        [ValidateAntiForgeryToken]
        public ActionResult List(BidRequireService service)
        {
            service.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R601")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index_CreateByMe(string bidId)
        {
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R601")]
        [ValidateAntiForgeryToken]
        public ActionResult List_CreateByMe(BidRequireService service)
        {
            service.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
            service.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R601")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index_ProcessByMe(string bidId)
        {
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R601")]
        [ValidateAntiForgeryToken]
        public ActionResult List_ProcessByMe(BidRequireService service)
        {
            service.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
            service.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
            service.Search_ProcessByMe();
            return PartialView(service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Create(string prCode)
        {
            //var serviceUserApprove = new UserApproveService();
            //_service.ObjHeader.USER_APPROVE = serviceUserApprove.GetUserApprove("BID_REQUIRED");
            var bidDescription = _service.GetBidDescription();
            if(bidDescription != null)
            {
                _service.ObjHeader.NOTES = bidDescription.BidDescription;
                _service.ObjHeader.NOTES_EN = bidDescription.BidDescriptionEN;

            }

            if (prCode != null)
            {
                ViewBag.PRCode = prCode;
            }

            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(BidRequireService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Create(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = $"SubmitIndex();Forms.Close('{service.ViewId}');Forms.LoadAjax({{url:'{Url.Action("Detail", new { id = service.ObjHeader.CODE })}'}});";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        /// <summary>
        /// Chức năng chỉnh sửa đơn hàng của user cửa hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyValidateAntiForgeryToken]
        public ActionResult Edit(string id, string viewIdDetail)
        {
            dynamic param = new ExpandoObject();
            param.IsFetch = true;
            _service.Get(id, param);

            //ConfigViettinBank: Xóa phân quyền dự án

            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP203", _service.ObjHeader.PROJECT_CODE);
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
                ViewBag.Error = "Đề nghị không ở trạng thái mới tạo";
                return PartialView("Error");
            }
          
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

            //ConfigViettinBank: Xóa phân quyền dự án

            //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP204", _service.ObjHeader.PROJECT_CODE);
            //if (!checkRight.Item1)
            //{
            //    ViewBag.Error = checkRight.Item2;
            //    return PartialView("Error");
            //}

            return PartialView(_service);
        }


        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult Update(BidRequireService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = "try{ReloadDetailPR(); SubmitIndex();}catch(e){};";
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

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult AddUser(string projectCode)
        {
            _service.ObjSearchPR.PROJECT_CODE = projectCode;
            return PartialView(_service);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ListUserForAdd(BidRequireService service)
        {
            service.SearchUser();
            return PartialView(service);
        }

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult AddPR(string projectCode, string lstPrCodeJson)
        {
            _service.ObjSearchPR.TYPE = "SAP";
            _service.ObjSearchPR.PROJECT_CODE = projectCode;
            ViewBag.LIST_PR_CODE = lstPrCodeJson;
            return PartialView(_service);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ListPRForAdd(BidRequireService service)
        {
            var servicePREB = new PREBService();
            string lstPrCodeJson = service.LIST_PR_CODE;
            servicePREB.ObjDetail.CODE = service.ObjSearchPR.PR_CODE;
            servicePREB.ObjDetail.PROJECT_CODE = service.ObjSearchPR.PROJECT_CODE;
            servicePREB.ObjDetail.STATUS = EB_Status.DaPheDuyet;
            servicePREB.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
            servicePREB.Search(lstPrCodeJson);
            return PartialView("ListPREBForAdd", servicePREB);
        }

        
        [MyValidateAntiForgeryToken]
        public ActionResult Send(string code, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.Send(code);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Cancel(string code, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.Cancel(code);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
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

            _service.Approve(code, reason);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');$('#btn_close_modal_approve_bidRequire').click();}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult CancelApprove(string code, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.CancelApprove(code);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
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

            _service.Reject(code, reason);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');$('#btn_close_modal_reject_bidRequire').click();}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult RequireUpdate(string id, string viewId)
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
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');$('#btn_close_modal_requireUpdate_bidRequire').click();}}}});", Url.Action("Detail", new { id = code }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }

            return result.ToJsonResult();
        }

        [Authorize]
        public ActionResult ShowDetailPR(string type, string code)
        {
            ViewBag.Type = type;
            ViewBag.Code = code;
            return PartialView();
        }

        [Authorize]
        public ActionResult ShowAllDetailPR(BidRequireService service)
        {
            return PartialView(service);
        }
    }
}