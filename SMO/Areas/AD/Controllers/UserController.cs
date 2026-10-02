using System.Web.Mvc;
using SMO.Service.AD;
using System.Web.Script.Serialization;
using System.Dynamic;
using SMO.Service.EB;
using System.Collections.Generic;
using SMO.Service.Class;
using System;
using System.IO;
using SMO.Core.Entities;
using System.Linq;

namespace SMO.Areas.AD.Controllers
{
    public class UserController : Controller
    {
        private UserService _service;

        public UserController()
        {
            _service = new UserService();
        }

        //[MyValidateAntiForgeryToken]
        //public ActionResult BuildTreeRight(string userName)
        //{
        //    var lstNode = _service.BuildTreeRight(userName);
        //    JavaScriptSerializer oSerializer = new JavaScriptSerializer();
        //    oSerializer.MaxJsonLength = int.MaxValue;
        //    ViewBag.zNode = oSerializer.Serialize(lstNode);
        //    return PartialView();
        //}

        [MyValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R140")]
        public ActionResult IndexOrganize()
        {
            var orgService = new OrganizeService();
            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            var lstNodeOrg = orgService.BuildTree();
            ViewBag.zNodeOrg = oSerializer.Serialize(lstNodeOrg);
            ViewBag.CountOrg = lstNodeOrg.Count;
            ViewBag.ListNodeOrg = lstNodeOrg;
            _service.ObjDetail.USER_TYPE = UserType.NB;
            //Nếu là người dùng tập đoàn thì có thể thấy ngay hết tất cả người dùng trong tập đoàn
            //if (!string.IsNullOrWhiteSpace(ProfileUtilities.User.Organize.PARENT))
            //{
            //    _service.ObjDetail.ORGANIZE_ID = ProfileUtilities.User.ORGANIZE_ID;
            //}
            return PartialView(_service);
        }

        [MyValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R140")]
        public ActionResult IndexContractor()
        {
            var contractorService = new ContractorService();
            return PartialView(contractorService);
        }

        [ValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R140")]
        public ActionResult ListUserContractor(ContractorService service)
        {
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R140")]
        public ActionResult BuildTreeUser(string idUserSelected)
        {
            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            var lstNodeUser = _service.BuildTreeUser();
            ViewBag.zNodeUser = oSerializer.Serialize(lstNodeUser);
            ViewBag.IdUserSelected = idUserSelected;
            return PartialView();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult BuildTreeRight(string userName, string orgId)
        {
            var lstNode = _service.BuildTreeRight(userName, orgId);
            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            oSerializer.MaxJsonLength = int.MaxValue;
            ViewBag.zNode = oSerializer.Serialize(lstNode);
            return PartialView();
        }

        [MyValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R140")]
        public ActionResult EditOrgOfUser(string userName)
        {
            _service.ObjDetail.USER_NAME = userName;
            return PartialView(_service);
        }
        // Cây user
        [MyValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R140")]
        public ActionResult EditRightOfUser(string userName)
        {
            var lstNode = _service.BuildTreeOrg(userName);
            JavaScriptSerializer oSerializer = new JavaScriptSerializer
            {
                MaxJsonLength = int.MaxValue
                
            };
            ViewBag.zNode = oSerializer.Serialize(lstNode);
            _service.Get(userName);
            //_service.ObjDetail.USER_NAME = userName;
            return PartialView(_service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult UpdateOrgOfUser(string userName, string orgList)
        {
            var result = new TransferObject
            {
                Type = TransferType.AlertSuccessAndJsCommand
            };
            _service.UpdateOrgOfUser(userName, orgList);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "LoadTabDanhSachQuyen();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult BuildTreeOrg(string userName)
        {
            var lstNode = _service.BuildTreeOrg(userName);
            JavaScriptSerializer oSerializer = new JavaScriptSerializer
            {
                MaxJsonLength = int.MaxValue
            };
            ViewBag.zNode = oSerializer.Serialize(lstNode);
            return PartialView();
        }


        [ValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R140")]
        public ActionResult List(UserService service)
        {
            service.Search();
            return PartialView(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ListUserGroupOfUser(UserService service)
        {
            dynamic param = new ExpandoObject();
            param.IsFetch_ListUserUserGroup = true;
            service.Get(service.ObjDetail.USER_NAME, param);
            return PartialView(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ListRoleOfUser(UserService service)
        {
            dynamic param = new ExpandoObject();
            param.IsFetch_ListUserUserGroup = true;
            param.IsFetch_ListUserRole = true;
            service.Get(service.ObjDetail.USER_NAME, param);
            return PartialView(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ListRoleForAdd(UserService service)
        {
            service.SearchRoleForAdd();
            return PartialView(service);
        }


        [MyValidateAntiForgeryToken]
        public ActionResult AddRole(string userName)
        {
            _service.ObjDetail.USER_NAME = userName;
            return PartialView(_service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult AddRoleToUser(string lstRole, string userName)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.AddRoleToUser(lstRole, userName);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = "SubmitListRoleForAdd(); SubmitListRoleOfUser(); BuildTreeRight();";
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
        public ActionResult DeleteRoleOfUser(string lstRole, string userName)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.DeleteRoleOfUser(lstRole, userName);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = "SubmitListRoleOfUser(); BuildTreeRight();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ListUserGroupForAdd(UserService service)
        {
            service.SearchUserGroupForAdd();
            return PartialView(service);
        }

        public ActionResult AddUserGroup(string userName)
        {
            _service.ObjDetail.USER_NAME = userName;
            return PartialView(_service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult AddUserGroupToUser(string lstUserGroup, string userName)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.AddUserGroupToUser(lstUserGroup, userName);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = "SubmitListUserGroupForAdd(); SubmitListUserGroupOfUser(); SubmitListRoleOfUser(); BuildTreeRight();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R140")]
        public ActionResult CreateUserOrganzie(string parent)
        {
            _service.ObjDetail.ORGANIZE_ID = parent;
            return PartialView(_service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R140")]
        public ActionResult CreateUserOrganzie(UserService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateUserOrganzie();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = $"BuildTreeUser('{service.ObjDetail.USER_NAME}');";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R502")]
        public ActionResult CreateUserContractor(string requestId, string vendorId)
        {
            var contractorService = new ContractorService();
            contractorService.RequestId = requestId;
            contractorService.VendorId = vendorId;
            contractorService.ContructorContractor();
            return PartialView(contractorService);
        }


        [HttpPost]
        [MyValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R502")] 
        public ActionResult CreateUserContractor(ContractorService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateUserContractor(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "SubmitIndex()";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R140")]
        public ActionResult Edit(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListUserUserGroup = true;
                param.IsFetch_ListUserRight = true;
                param.IsFetch_ListUserRole = true;
                param.IsFetch_ListUserCustomer = true;
                param.IsFetch_Contractor = true;
                _service.Get(id, param);

                var findOrg = _service.UnitOfWork.GetSession().Query<T_AD_ORGANIZE>().FirstOrDefault(x => x.PKID == _service.ObjDetail.ORGANIZE_ID);
                if (findOrg != null)
                {
                    if (findOrg.TYPE == "CP")
                    {
                        _service.COMPANY_ID = findOrg.PKID;
                    }
                    else
                    {
                        var findOrgParent = _service.UnitOfWork.GetSession().Query<T_AD_ORGANIZE>().FirstOrDefault(x => x.COMPANY_CODE == findOrg.COMPANY_CODE && x.TYPE == "CP");
                        _service.COMPANY_ID = findOrgParent?.PKID;
                    }
                }
            }
            return PartialView(_service);
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        [AuthorizeCustom(Right = "R2001")]
        public ActionResult UpdateRightOfUser(string userName, string rightList, string statusList, string orgId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.UpdateRightOfUser(userName, rightList, statusList, orgId);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "BuildTreeRight(); $('#cmdResetQuyen').show();";
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
        [AuthorizeCustom(Right = "R2002")]
        public ActionResult ResetRightOfUser(string userName)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.ResetRightOfUser(userName);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "BuildTreeRight(); $('#cmdResetQuyen').hide();";
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
        public ActionResult ResetPassword(string userName)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.ResetPassword(userName);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(UserService service)
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

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult Delete(string pStrListSelected)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.Delete(pStrListSelected);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = "SubmitIndex();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult DeleteUserGroupOfUser(string lstUserGroup, string userName)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.DeleteUserGroupOfUser(lstUserGroup, userName);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = "SubmitListUserGroupOfUser(); SubmitListRoleOfUser(); BuildTreeRight();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult ToggleActive(string id)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.ToggleActive(id);
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

        public ActionResult UpdatePhongBan(List<NodeUser> lstNode)
        {
            var result = new TransferObject();
            result.State = true;
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.UpdatePhongBan(lstNode);
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