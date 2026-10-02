using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class ReviewFinalController : Controller
    {
        private ReviewFinalService _service;
        public ReviewFinalController()
        {
            _service = new ReviewFinalService();
        }

        [AuthorizeCustom(Right = "R2101")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index()
        {
            _service.ObjDetail.STATUS = Review_Final_Status.KhoiTao;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R2101")]
        [ValidateAntiForgeryToken]
        public ActionResult List(ReviewFinalService service)
        {
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R2101")]
        public ActionResult Index_CreateByMe()
        {
            _service.ObjDetail.STATUS = Review_Final_Status.KhoiTao;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R2101")]
        [ValidateAntiForgeryToken]
        public ActionResult List_CreateByMe(ReviewFinalService service)
        {
            service.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R2101")]
        public ActionResult Index_ProcessByMe()
        {
            _service.ObjDetail.STATUS = Review_Final_Status.KhoiTao;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R2101")]
        [ValidateAntiForgeryToken]
        public ActionResult List_ProcessByMe(ReviewFinalService service)
        {
            service.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
            service.Search_ProcessByMe();
            return PartialView(service);
        }

        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult AddUser()
        {
            _service.ObjSearchUser.USER_TYPE = UserType.NB;
            return PartialView(_service);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ListUserForAdd(ReviewFinalService service)
        {
            service.SearchUser();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R2102")]
        [MyValidateAntiForgeryToken]
        public ActionResult Create()
        {
            _service.ObjDetail.FROM_DATE = DateTime.Now;
            _service.ObjDetail.TO_DATE = DateTime.Now.AddDays(1);
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeCustom(Right = "R2102")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewFinalService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Create();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                //result.ExtData = $"SubmitIndex();CloseFormCreate();Forms.LoadAjax({{url:'{Url.Action("Detail", new { id = service.ObjDetail.CODE })}'}});";
                result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = service.ObjDetail.CODE }), service.ViewId);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R2103")]
        [MyValidateAntiForgeryToken]
        public ActionResult Edit(string id, string viewIdDetail)
        {
            dynamic param = new ExpandoObject();
            param.IsFetch_ListContractor = true;
            _service.Get(id, param);
            ViewBag.ViewIdDetail = viewIdDetail;
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeCustom(Right = "R2103")]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ReviewFinalService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                //result.ExtData = $"SubmitIndex();CloseFormEdit();Forms.LoadAjax({{url:'{Url.Action("Detail", new { id = service.ObjDetail.CODE })}'}});";
                result.ExtData = "try{SubmitIndex();}catch(e){};";
                result.State = true;
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R2103")]
        [MyValidateAntiForgeryToken]
        public ActionResult AddContractor(string reviewId)
        {
            var service = new ContractorService();
            service.ObjDetail.REVIEW_FINAL_ID = reviewId;
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R2103")]
        [ValidateAntiForgeryToken]
        public ActionResult ListContractorForAdd(ContractorService service)
        {
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R2103")]
        [MyValidateAntiForgeryToken]
        public ActionResult AddContractorToReview(string reviewId, string lstContractor)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.AddContractorToReview(reviewId, lstContractor);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = $"SubmitListContractor();CloseFormDetail();CloseFormEdit();Forms.LoadAjax({{url:'{Url.Action("Detail", new { id = reviewId })}'}});Forms.LoadAjax({{url:'{Url.Action("Edit", new { id = reviewId })}'}});";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R2104")]
        [MyValidateAntiForgeryToken]
        public ActionResult Detail(string id)
        {
            dynamic param = new ExpandoObject();
            param.IsFetch_ListContractor = true;
            _service.Get(id, param);
            return PartialView(_service);
        }

        /// <summary>
        /// Chức năng xem lịch sử xếp hạng nhà thầu
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthorizeCustom(Right = "R2104")]
        [MyValidateAntiForgeryToken]
        public ActionResult ViewHistory(string id)
        {
            dynamic param = new ExpandoObject();
            param.IsFetch_ListContractor = true;
            _service.Get(id);
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R2102")]
        [MyValidateAntiForgeryToken]
        public ActionResult DeleteContractor(string contractorId, string reviewId, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.DeleteContractor(contractorId, reviewId);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3001", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    //result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = reviewId }), viewId);
                    result.ExtData = $"CloseFormDetail();CloseFormEdit();Forms.LoadAjax({{url:'{Url.Action("Detail", new { id = reviewId })}'}});Forms.LoadAjax({{url:'{Url.Action("Edit", new { id = reviewId })}'}});";
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3002", _service, result);
            }

            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R2105")]
        [MyValidateAntiForgeryToken]
        public ActionResult Send(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.Send(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3001", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3002", _service, result);
            }

            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R2105")]
        [MyValidateAntiForgeryToken]
        public ActionResult CancelSend(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.CancelSend(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3001", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3002", _service, result);
            }

            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R2106")]
        [MyValidateAntiForgeryToken]
        public ActionResult Cancel(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.Cancel(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3005", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3006", _service, result);
            }

            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R2107")]
        [MyValidateAntiForgeryToken]
        public ActionResult Approve(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.Approve(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3007", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3008", _service, result);
            }

            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R2108")]
        [MyValidateAntiForgeryToken]
        public ActionResult Reject(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.Reject(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3015", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3016", _service, result);
            }

            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R2107")]
        [MyValidateAntiForgeryToken]
        public ActionResult CancelApprove(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.CancelApprove(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("Detail", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }

            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R2104")]
        [MyValidateAntiForgeryToken]
        public ActionResult MenuDetail(string id, string idMenu = "")
        {
            _service.ObjDetail.CODE = id;
            _service.Get(id);
            ViewBag.IdMenu = idMenu;
            return PartialView(_service);
        }
    }
}