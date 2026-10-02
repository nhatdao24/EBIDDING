using SMO.Service.EB;
using SMO.Service.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SMO.Areas.EB.Controllers
{
    public class BidSpecController : Controller
    {
        private BidSpecService _service;

        public BidSpecController()
        {
            _service = new BidSpecService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            return PartialView(_service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult BuildTree(string specSelected, string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            var lstNodeSpec = _service.BuildTreeSpec();
            ViewBag.TongTrongSo = _service.ObjList.Sum(x => x.RATIO);
            ViewBag.zNodeSpec = oSerializer.Serialize(lstNodeSpec);
            ViewBag.SpecSelected = specSelected;
            ViewBag.BID_ID = bidId;
            return PartialView();
        }

        public ActionResult AddSpecMD(string bidId)
        {
            var service = new MdBidSpecService();
            var lstNodeSpec = service.BuildTreeSpec(bidId);
            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            ViewBag.zNodeSpec = oSerializer.Serialize(lstNodeSpec);
            ViewBag.BidId = bidId;
            return PartialView(service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult AddSpecMDToBid(string lstSpec, string bidId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.AddSpecMDToBid(lstSpec, bidId);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "BuildTreeSpec('', true);";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            result.State = true;
            _service.Delete(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = string.Format("BuildTreeSpec('', true);");
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Preview(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.Search();
            return PartialView(_service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult ViewSpecScore(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.ObjSpecContractor.BID_ID = bidId;
            _service.GetListViewSpecScore();
            return PartialView(_service);
        }

        /// <summary>
        /// Chấm điểm tiêu chí thầu
        /// </summary>
        /// <param name="bidId"></param>
        /// <returns></returns>
        [MyValidateAntiForgeryToken]
        public ActionResult CouncilScore(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.ObjSpecContractor.BID_ID = bidId;
            _service.GetListSpecCouncil(bidId);
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCouncilScore(BidSpecService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.UpdateSpecCouncil();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "setTimeout(function() { $('#mnuChamThau').click(); }, 2000);";
                //result.ExtData = "$('#"+service.ViewId+"').remove(); $('#mnuChamThau').click();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        /// <summary>
        /// Nhà thầu nhập giá trị cho các tiêu chí thầu
        /// </summary>
        /// <param name="bidId"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResult ContractorInputSpec(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.GetListSpecContractor(ProfileUtilities.User.CONTRACTOR_ID);
            return PartialView(_service);
        }

        //[HttpPost, ValidateInput(false)]
        //[MyValidateAntiForgeryToken]
        //public ActionResult UpdateContractorInputSpec(BidSpecService service, string moTaKyThuat, string moTaNangLuc, string moTaGia, string moTaPhapLy)
        //{
        //    var result = new TransferObject();
        //    result.Type = TransferType.AlertSuccessAndJsCommand;
        //    service.UpdateContractorInputSpec(Request, moTaKyThuat, moTaNangLuc, moTaGia, moTaPhapLy);
        //    if (service.State)
        //    {
        //        SMOUtilities.GetMessage("1001", service, result);
        //        result.ExtData = "Forms.LoadAjax({url:'" + Url.Action("ContractorInputSpec", "BidSpec", new { @bidId = service.ObjDetail.BID_ID }) + "',  htmlTarget: '#divContentDetail'})";
        //    }
        //    else
        //    {
        //        result.Type = TransferType.AlertDanger;
        //        SMOUtilities.GetMessage("1004", service, result);
        //    }
        //    return result.ToJsonResult();
        //}


        //ConfigViettinBank: Chỉ thực hiện nộp 1 file hồ sơ(tổng hợp)


        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult UpdateContractorInputSpec(BidSpecService service, string moTaTongHop)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.UpdateContractorInputSpec(Request, moTaTongHop);
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "Forms.LoadAjax({url:'" + Url.Action("ContractorInputSpec", "BidSpec", new { @bidId = service.ObjDetail.BID_ID }) + "',  htmlTarget: '#divContentDetail'})";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Create(string parent, string bidId)
        {
            _service.ObjHeader.PARENT_ID = parent;
            _service.ObjHeader.BID_ID = bidId;
            _service.ObjHeader.TYPE = "NUMBER";
            return PartialView(_service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(BidSpecService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Create();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = string.Format("BuildTreeSpec('{0}', true);Forms.CloseRightContent();", service.ObjDetail.PKID);
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


        [MyValidateAntiForgeryToken]
        public ActionResult Update(BidSpecService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update();
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = string.Format("BuildTreeSpec('{0}', true);Forms.CloseRightContent();", service.ObjHeader.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult UpdateTree(List<NodeSpec> lstNode)
        {
            var result = new TransferObject();
            result.State = true;
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.UpdateTree(lstNode);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "";
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