using SMO.Repository.Implement.EB;
using SMO.Service.AD;
using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class ReviewController : Controller
    {
        private ReviewService _service;
        public ReviewController()
        {
            _service = new ReviewService();
        }

        [AuthorizeCustom(Right = "R901")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index()
        {
            _service.ObjDetail.STATUS = Review_Status.KhoiTao;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R901")]
        [ValidateAntiForgeryToken]
        public ActionResult List(ReviewService service)
        {
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R901")]
        public ActionResult Index_CreateByMe()
        {
            _service.ObjDetail.STATUS = Review_Status.KhoiTao;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R901")]
        [ValidateAntiForgeryToken]
        public ActionResult List_CreateByMe(ReviewService service)
        {
            service.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R901")]
        public ActionResult Index_ProcessByMe()
        {
            _service.ObjDetail.STATUS = Review_Status.KhoiTao;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R901")]
        [ValidateAntiForgeryToken]
        public ActionResult List_ProcessByMe(ReviewService service)
        {
            service.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
            service.Search_ProcessByMe();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(string requireCode)
        {
            _service.ObjDetail.EVALUATE_DATE = DateTime.Now;
            _service.ObjDetail.END_DATE = DateTime.Now;
            _service.ObjDetail.ORG_CODE = ProfileUtilities.User.ORGANIZE_ID;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R906")]
        [MyValidateAntiForgeryToken]
        public ActionResult EndReview(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.EndReview(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("MenuDetail", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }

            return result.ToJsonResult();
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeCustom(Right = "R902")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Create();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("MenuEdit", new { id = service.ObjDetail.CODE }), service.ViewId);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R903")]
        [MyValidateAntiForgeryToken]
        public ActionResult Edit(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                _service.Get(id);
            }
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R903")]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ReviewService service)
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
                SMOUtilities.GetMessage("1006", service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R903")]
        [MyValidateAntiForgeryToken]
        public ActionResult OpenReview(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.OpenReview(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("MenuDetail", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R903")]
        [MyValidateAntiForgeryToken]
        public ActionResult SendMailCouncilOfReview(string id)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.CreateNotifyCouncil(id);
            _service.CreateNotiEmail(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("MenuDetail", new { id = id }), id);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R904")]
        [MyValidateAntiForgeryToken]
        public ActionResult Detail(string id)
        {
            dynamic param = new ExpandoObject();
            param.IsFetch_ListCouncil = true;
            param.IsFetch_ListContractor = true;
            _service.Get(id, param);
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R904")]
        [MyValidateAntiForgeryToken]
        public ActionResult DetailListCouncil(string id)
        {
            var service = new ReviewCouncilService();
            service.ObjDetail.REVIEW_ID = id;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R904")]
        [MyValidateAntiForgeryToken]
        public ActionResult DetailListContractor(string id)
        {
            var service = new ReviewContractorService();
            service.ObjDetail.REVIEW_ID = id;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R904")]
        [MyValidateAntiForgeryToken]
        public ActionResult DetailListSpec(string id)
        {
            var service = new ReviewSpecService();
            service.ObjDetail.REVIEW_ID = id;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R904")]
        [MyValidateAntiForgeryToken]
        public ActionResult MenuDetail(string id, string idMenu = "")
        {
            _service.ObjDetail.CODE = id;
            _service.Get(id);
            ViewBag.IdMenu = idMenu;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R903")]
        [MyValidateAntiForgeryToken]
        public ActionResult MenuEdit(string id)
        {
            _service.ObjDetail.CODE = id;
            _service.Get(id);
            return PartialView(_service);
        }
    }
}