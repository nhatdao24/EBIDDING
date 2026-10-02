using Microsoft.Ajax.Utilities;
using Microsoft.Graph.Models;
using SMO.Core.Entities;
using SMO.Repository.Implement.EB;
using SMO.Service.AD;
using SMO.Service.EB;
using SMO.Service.MD;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace SMO.Areas.EB.Controllers
{
    public class BidController : Controller
    {
        private BidService _service;

        public BidController()
        {
            _service = new BidService();
        }

        #region Quản lý bid fecon
        [AuthorizeCustom(Right = "R401")]
        public ActionResult Index()
        {
            _service.ObjDetail.GIAI_DOAN = Phase_Status.KhoiTao;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R401")]
        [ValidateAntiForgeryToken]
        public ActionResult List(BidService service)
        {
            if (service.ObjDetail.IS_EXPORT_EXCEL)
            {
                MemoryStream outFileStream = new MemoryStream();
                service.ExportExcel(ref outFileStream);
                if (!service.State)
                {
                    return Content($"<script>alert('{service.ErrorMessage}');</script>;");
                }
                return File(outFileStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Danh_Sach_Goi_Thau.xlsx");
            }
            service.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R401")]
        public ActionResult Index_CreateByMe()
        {
            _service.ObjDetail.GIAI_DOAN = Phase_Status.KhoiTao;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R401")]
        [ValidateAntiForgeryToken]
        public ActionResult List_CreateByMe(BidService service)
        {
            service.ObjDetail.CREATE_BY = ProfileUtilities.User.USER_NAME;
            service.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R401")]
        public ActionResult Index_ProcessByMe()
        {
            _service.ObjDetail.GIAI_DOAN = Phase_Status.KhoiTao;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R401")]
        [ValidateAntiForgeryToken]
        public ActionResult List_ProcessByMe(BidService service)
        {
            service.ObjDetail.USER_APPROVE = ProfileUtilities.User.USER_NAME;
            service.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;
            service.Search_ProcessByMe();
            return PartialView(service);
        }


        [Authorize]
        [MyValidateAntiForgeryToken]
        public ActionResult AddHHDV()
        {
            var service = new HhdvService();
            return PartialView(service);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ListHhdvForAdd(HhdvService service)
        {
            service.Search();
            return PartialView(service);    
        }


        [MyValidateAntiForgeryToken]
        public ActionResult Create(string requireCode, string bidID, string contractorID, string userSend, string isVtb, bool isNewRound = false)
        {
            if (isVtb == Global.IsViettinBank)
            {
                _service.ObjDetail = new T_EB_BID();

                _service.ObjDetail.NGAY_MO_THAU = DateTime.Now;
                _service.ObjDetail.NGAY_DONG_THAU = DateTime.Now.AddDays(7);
                _service.ObjDetail.IS_NEW_ROUND = false;

                _service.ObjDetail.COMPANY_CODE = ProfileUtilities.User.Organize.COMPANY_CODE;

                return PartialView("CreateVtb", _service);
            }

            if (!string.IsNullOrEmpty(requireCode) && string.IsNullOrEmpty(bidID) && !isNewRound)
            {
                var serviceBidRequired = new BidRequireService();
                serviceBidRequired.Get(requireCode);

                if (serviceBidRequired.ObjHeader.STATUS != EB_Status.DaPheDuyet)
                {
                    ViewBag.IsError = "1"; 
                }

                _service.ObjDetail.REQUIRED_CODE = requireCode;
                _service.ObjDetail.NGAY_MO_THAU = DateTime.Now;
                _service.ObjDetail.NGAY_DONG_THAU = DateTime.Now;
                _service.ObjDetail.TEN_GOI_THAU = serviceBidRequired.ObjHeader.NAME;
                _service.ObjDetail.MO_TA = serviceBidRequired.ObjHeader.NOTES;
                _service.ObjDetail.MO_TA_EN = serviceBidRequired.ObjHeader.NOTES_EN;
                _service.ObjDetail.PROJECT_CODE = serviceBidRequired.ObjHeader.PROJECT_CODE;
                _service.ObjDetail.PACKAGE_CODE = serviceBidRequired.ObjHeader.PACKAGE_CODE;
                _service.ObjDetail.SCOPE_CODE = serviceBidRequired.ObjHeader.SCOPE_CODE;
                _service.ObjDetail.SCOPE_TYPE_CODE = serviceBidRequired.ObjHeader.SCOPE_TYPE_CODE;
                _service.ObjDetail.TYPE = serviceBidRequired.ObjHeader.BID_TYPE_CODE;
                _service.ListRequireFile = serviceBidRequired.ObjHeader.ListFile;

                return PartialView(_service);
            }
            else if (userSend != null)
            {
                _service.Get(bidID);
                if (_service.ObjDetail.ROUND == 1)
                {
                    _service.ObjDetail.PARENT_BID = bidID;
                }
                _service.ObjDetail.NGAY_MO_THAU = DateTime.Now;
                _service.ObjDetail.NGAY_DONG_THAU = DateTime.Now;
                _service.ObjDetail.NGAY_KT_CHAM_THAU = DateTime.Now;
                _service.ObjDetail.IS_NEW_ROUND = isNewRound;
                ViewBag.ListContractorID = contractorID;

                return PartialView(_service);
            }
            else
            {
                var result = new TransferObject();
                result.Type = TransferType.AlertSuccessAndJsCommand;
                _service.CreateNewRound(bidID, contractorID, isNewRound);

                if (_service.State)
                {
                    SMOUtilities.GetMessage("1001", _service, result);
                    result.ExtData = "SubmitIndex();";
                }
                else
                {
                    result.Type = TransferType.AlertDanger;
                    SMOUtilities.GetMessage("1004", _service, result);
                }
                return result.ToJsonResult();
            }
        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult CreateVtb(BidService service, string lstContractorId, string hinhThucMuaSam)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            service.CreateVtb(Request, lstContractorId, hinhThucMuaSam);

            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);

                var urlDetail = Url.Action("MenuDetailBidManager", new { id = service.ObjDetail.PKID });

                result.ExtData = string.Format(
                    "try{{SubmitIndex();}}catch(e){{}}; " +
                    "Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}}); " +
                    "alert('Anh/chị cần hoàn thiện thông tin chi tiết gói thầu tại mục \"Nhập thông tin chi tiết gói thầu\"');",
                    urlDetail, service.ViewId);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }

            return result.ToJsonResult();
        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(BidService service, string lstContractorId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            service.Create(Request, lstContractorId);

            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);

                var urlDetail = Url.Action("MenuDetailBidManager", new { id = service.ObjDetail.PKID });

                result.ExtData = string.Format(
                    "try{{SubmitIndex();}}catch(e){{}}; " +
                    "Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}}); " +
                    "alert('Anh/chị cần hoàn thiện thông tin chi tiết gói thầu tại mục \"Nhập thông tin chi tiết gói thầu\"');",
                    urlDetail, service.ViewId);
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
                dynamic param = new ExpandoObject();
                param.IsFetch_ListFile = true;
                param.IsFetch_ListNew = true;
                _service.Get(id, param);

                //ConfigViettinBank: Tạm thời bỏ phân quyền

                //var checkRight = AuthorizeUtilities.CheckUserRightProject("RP303", _service.ObjDetail.PROJECT_CODE);
                //if (!checkRight.Item1)
                //{
                //    ViewBag.Error = checkRight.Item2;
                //    return PartialView("Error");
                //}

            }
            return PartialView(_service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult MenuEdit(string id)
        {
            _service.ObjDetail.PKID = id;
            _service.Get(id);
            return PartialView(_service);
        }


        //ConfigViettinBank: Đổi chức năng Update thành UpdateVtb 

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public async Task<ActionResult> Update(BidService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            await service.Update(Request);
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


        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateVtb(BidService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            await service.UpdateVtb(Request);
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

        [MyValidateAntiForgeryToken]
        public ActionResult MenuDetailBidManager(string id, string isOpenFromPortal, string idMenu = "")
        {
            _service.ObjDetail.PKID = id;
            _service.Get(id);
            ViewBag.IsOpenFromPortal = isOpenFromPortal;
            ViewBag.IdMenu = idMenu;
            return PartialView(_service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult DetailBidManager(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListFile = true;
                param.IsFetch_ListHistory = true;
                _service.Get(id, param);
            }
            return PartialView(_service);
        }


        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyValidateAntiForgeryToken]
        public ActionResult DetailRequire(string bidId)
        {
            var service = new BidRequireService();
            dynamic param = new ExpandoObject();
            param.IsFetch = true;
            service.Get(bidId, param);
            return PartialView(service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult DetailListCouncil(string bidId)
        {
            var service = new BidCouncilService();
            service.ObjDetail.BID_ID = bidId;
            service.Search();
            return PartialView(service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult DetailListContractor(string bidId)
        {
            var service = new BidContractorService();
            service.ObjDetail.BID_ID = bidId;
            service.Search();
            return PartialView(service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult DetailListSpec(string bidId)
        {
            var service = new BidSpecService();
            service.ObjDetail.BID_ID = bidId;
            service.Search();
            return PartialView(service);
        }

        #endregion

        #region Quản lý bid nhà thầu
        [Authorize]
        public ActionResult IndexBidContractor()
        {
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult ListBidOfContractor(BidService service)
        {
            service.SearchBidOfContractor();
            return PartialView(service);
        }

        [Authorize]
        public ActionResult IndexBidContractorClose()
        {
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult ListBidOfContractorClose(BidService service)
        {
            service.SearchBidOfContractorClose();
            return PartialView(service);
        }

        [Authorize]
        public ActionResult IndexBidContractorWait()
        {
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult IndexBidContractorOpen()
        {
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult ListBidOfContractorWait(BidService service)
        {
            service.SearchBidOfContractorWait();
            return PartialView(service);
        }

        [Authorize]
        public ActionResult ListBidOfContractorOpen(BidService service)
        {
            service.SearchBidOfContractorOpen();
            return PartialView(service);
        }

        [Authorize]
        public ActionResult DetailBidContractorWait(string id, string isOpenFromPortal)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListFile = true;
                _service.Get(id, param);

                var find = _service.UnitOfWork.Repository<BidContractorRepo>().Queryable().FirstOrDefault(x => x.CONTRACTOR_ID == ProfileUtilities.User.CONTRACTOR_ID && x.BID_ID == id);
                if (find == null)
                {
                    return Content("");
                }
                else if (find.STATUS == "AP")
                {
                    return PartialView("MenuDetailBidContractor", _service);
                }
                else if (find.STATUS == "RJ")
                {
                    ViewBag.STATUS = "RJ";
                }
            }
            ViewBag.IsOpenFromPortal = isOpenFromPortal;
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult DetailBidContractorOpen(string id, string isOpenFromPortal)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListFile = true;
                _service.Get(id, param);
            }
            ViewBag.IsOpenFromPortal = isOpenFromPortal;
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult MenuDetailBidContractor(string id, string isOpenFromPortal, string idMenu = "")
        {
            _service.ObjDetail.PKID = id;
            _service.Get(id);
            ViewBag.IsOpenFromPortal = isOpenFromPortal;
            ViewBag.IdMenu = idMenu;
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult DetailBidContractor(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListFile = true;
                _service.Get(id, param);
            }
            return PartialView(_service);
        }

        [Authorize]
        public ActionResult JoinBid(string bidId, string status)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            if (ProfileUtilities.User.USER_TYPE == UserType.NB)
            {
                return Content("");
            }

            _service.JoinBid(bidId, status);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = $"CloseForm();";
                if (status == "AP")
                {
                    result.ExtData = $"try{{GetTopBidContractor(); GetTopBidContractorOpen();}}catch(e){{}}; CloseForm();Forms.LoadAjax('{Url.Action("MenuDetailBidContractor", new { id = bidId })}')";
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }
        #endregion

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
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("MenuDetailBidManager", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3002", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult RequireUpdate(string id)
        {
            _service.Get(id);
            return PartialView(_service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]

        public ActionResult RequireUpdate(string id, string viewId, string reason)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.RequireUpdate(id, reason);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3003", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{$('#btn_close_modal_requireUpdate_bid').click();}}}});", Url.Action("MenuDetailBidManager", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3004", _service, result);
            }

            return result.ToJsonResult();
        }

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
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}')", Url.Action("MenuDetailBidManager", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3006", _service, result);
            }

            return result.ToJsonResult();
        }
        public ActionResult Approve(string id)
        {
            _service.Get(id);
            return PartialView(_service);
        }
        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult Approve(string id, string viewId, string reason)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.Approve(id, reason);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3007", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{$('#btn_close_modal_approve_bid').click();}}}});", Url.Action("MenuDetailBidManager", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3008", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult KetThuc(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.KetThuc(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3007", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("MenuDetailBidManager", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3008", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult ScoreBid(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.ScoreBid(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3009", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("MenuDetailBidManager", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3010", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult ContractorOfNewRound(string id, string viewId)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListContractor = true;
                _service.Get(id, param);
            }
            ViewBag.ViewId = viewId;
            return PartialView(_service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult CancelBid(string id)
        {
            _service.Get(id);
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult CancelBid(string id, string viewId, string reason)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.CancelBid(id, reason);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3011", _service, result);
                result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{$('#btn_close_modal_cancel_bid').click();}}}});", Url.Action("MenuDetailBidManager", new { id = id }));
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3012", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult CloseBid(string id)
        {
            _service.Get(id);
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult CloseBid(BidService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            service.CloseBid(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("3011", service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{$('#btn_close_modal_cancel_bid').click();}}}});", Url.Action("MenuDetailBidManager", new { id = service.ObjDetail.PKID }));
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3012", service, result);
            }

            return result.ToJsonResult();
        }


        /// <summary>
        /// Chọn danh sách sơ bộ các nhà thầu
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viewId"></param>
        /// <returns></returns>
        [MyValidateAntiForgeryToken]
        public ActionResult DebutContractor(string id, string viewId, bool isSelect)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListContractor = true;
                _service.Get(id, param);
            }
            ViewBag.ViewId = viewId;
            ViewBag.IsSelect = isSelect;
            return PartialView(_service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDebutContractor(BidService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.DebutContractor();
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = "$('#btnClose').click(); try{{SubmitListDebutContractor();}}catch(e){{}};";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult EndScoreBid(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.EndScoreBid(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3013", _service, result);
                result.ExtData = "";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("MenuDetailBidManager", new { id = id }), viewId);
                }
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3014", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Reject(string id)
        {
            _service.Get(id);
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult Reject(string id, string viewId, string reason)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.Reject(id, reason);
            if (_service.State)
            {
                SMOUtilities.GetMessage("3015", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{$('#btn_close_modal_reject_bid').click();}}}});", Url.Action("MenuDetailBidManager", new { id = id }));
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("3016", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult ChoiceContractorWin(string id, List<T_EB_BID_CONTRACTOR> lstContractorWin)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.ChoiceContractorWin(id, lstContractorWin);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitListDebutContractor();}}catch(e){{}};";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }

            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult CancelContractorWin(string id, string contractorId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.CancelContractorWin(id, contractorId);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitListDebutContractor();}}catch(e){{}};";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }

            return result.ToJsonResult();
        }

        /// <summary>
        /// Kết thúc giai đoạn thương thảo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viewId"></param>
        /// <returns></returns>
        //[AuthorizeCustom(Right = "R405")]
        [MyValidateAntiForgeryToken]
        public ActionResult KetThucThuongThao(string id, string viewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;

            _service.KetThucThuongThao(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("MenuDetailBidManager", new { id = id }), viewId);
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
        public ActionResult ThuongThaoLai(string id, string viewId) 
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.ThuongThaoLai(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "try{{SubmitIndex();}}catch(e){{}};";
                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    result.ExtData = string.Format("try{{SubmitIndex();}}catch(e){{}}; Forms.LoadAjax({{url:'{0}', complete:function(){{Forms.Close('{1}');}}}});", Url.Action("MenuDetailBidManager", new { id = id }), viewId);
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