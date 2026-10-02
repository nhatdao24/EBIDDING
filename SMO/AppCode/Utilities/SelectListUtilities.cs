using SMO.Core.Entities;
using SMO.Core.Entities.AD;
using SMO.Repository.Common;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.EB;
using SMO.Repository.Implement.MD;
using SMO.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO
{
    public class SelectListUtilities
    {
        public class Data
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }

        /// <summary>
        /// Lấy ra danh sách Domain để chọn dropdownlist 
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetAllDomain(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstDomain = UnitOfWork.Repository<DomainRepo>().GetAll();
            foreach (var obj in lstDomain)
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.CODE + " - " + obj.NAME });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// Lấy ra danh sách chọn cho dropdownlist
        /// </summary>
        /// <param name="strDomainCode">Mã domain</param>
        /// <param name="isShowValue">Có hiển thị cả mã dictionary trên dropdownlist không. Ví dụ : vn - Việt Nam</param>
        /// <param name="isAddBlank">Có thêm giá trị blank vào đầu tiên trong list chọn của dropdownlist hay không</param>
        /// <param name="selected">Giá trị được chọn mặc định khi hiển thị dropdownlist</param>
        /// <param name="strLang">Ngôn ngữ - để lấy ra giá trị của dictionary theo ngôn ngữ</param>
        /// <param name="isShowDefault"></param>
        /// <returns></returns>
        public static SelectList GetDictionary(string strDomainCode, bool isShowValue = true, bool isAddBlank = true, string selected = "", string strLang = "vi", bool isShowDefault = true)
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            var strValue = string.Empty;
            var listDic = UnitOfWork.Repository<DictionaryRepo>().Queryable().Where(x => x.FK_DOMAIN == strDomainCode && x.LANG == strLang).OrderBy(x => x.C_ORDER).ToList();
            if (isAddBlank)
            {
                lstData.Add(new Data { Value = string.Empty, Text = " - " });
            }
            foreach (var dic in listDic)
            {
                if (string.IsNullOrWhiteSpace(selected) && dic.C_DEFAULT == true && isShowDefault == true)
                {
                    selected = dic.CODE;
                }
                strValue = dic.CODE + " - " + dic.C_VALUE;
                if (!isShowValue)
                {
                    strValue = dic.C_VALUE;
                }
                lstData.Add(new Data { Value = dic.CODE, Text = strValue });
            }
            return new SelectList(lstData, "Value", "Text", selected);
        }

        public static SelectList GetAllDatatableName()
        {
            var service = new DynamicSqlService();
            service.GetAllTableName();
            var lstData = new List<Data>();
            lstData.Add(new Data() { Value = "", Text = " - " });
            foreach (var item in service.ObjList)
            {
                lstData.Add(new Data() { Value = item.Name, Text = item.Name });
            }
            return new SelectList(lstData, "Value", "Text");
        }

        public static SelectList GetUserApproveProject(string right, string projectCode, bool isAddBlank = true)
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstUserName = new List<string>();
            var lstData = new List<Data>();

            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }

            if (string.IsNullOrEmpty(projectCode))
            {
                return new SelectList(lstData, "Value", "Text"); ;
            }

            //Tìm trong T_AD_USER_RIGHT
            var findUserRight = UnitOfWork.GetSession().Query<T_AD_RIGHT_PROJECT_USER>().Where(x => x.RIGHT_CODE == right && x.PROJECT_CODE == projectCode).ToList();

            lstUserName.AddRange(findUserRight.Select(x => x.USER_NAME).Distinct());

            var lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => lstUserName.Contains(x.USER_NAME)).OrderBy(x => x.USER_NAME).ToList();
            foreach (var obj in lstUser)
            {
                string titleName = obj.Title?.NAME ?? "Chưa có thông tin chức danh";

                lstData.Add(new Data { Value = obj.USER_NAME, Text = obj.USER_NAME + " - " + obj.FULL_NAME + " - " + titleName });
            }
            return new SelectList(lstData, "Value", "Text");
        }



        public static SelectList GetUserApprove(string right, bool isAddBlank = true)
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstUserName = new List<string>();
            var lstData = new List<Data>();

            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }

            //Tìm trong T_AD_USER_RIGHT
            var findUserRight = UnitOfWork.Repository<UserRightRepo>().Queryable().Where(x => x.IS_ADD == true && x.FK_RIGHT == right).ToList();
            lstUserName.AddRange(findUserRight.Select(x => x.USER_NAME));

            //Tìm theo ROLE
            var findRole = UnitOfWork.Repository<RoleDetailRepo>().Queryable().Where(x => x.FK_RIGHT == right).ToList();
            if (findRole.Count > 0)
            {
                var lstRole = findRole.Select(x => x.FK_ROLE).ToList();
                //Tìm user gán với role
                var findUserRole = UnitOfWork.Repository<UserRoleRepo>().Queryable().Where(x => lstRole.Contains(x.ROLE_CODE)).ToList();
                lstUserName.AddRange(findUserRole.Select(x => x.USER_NAME));

                //Tìm user group gán với role
                var findUserGroupRole = UnitOfWork.Repository<UserGroupRoleRepo>().Queryable().Where(x => lstRole.Contains(x.ROLE_CODE)).ToList();
                var lstGroup = findUserGroupRole.Select(x => x.USER_GROUP_CODE).ToList();
                var findUserUserGroup = UnitOfWork.Repository<UserUserGroupRepo>().Queryable().Where(x => lstGroup.Contains(x.USER_GROUP_CODE));

                lstUserName.AddRange(findUserUserGroup.Select(x => x.USER_NAME));
            }

            lstUserName = lstUserName.Distinct().ToList();

            var lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => lstUserName.Contains(x.USER_NAME) && x.USER_TYPE == UserType.NB).OrderBy(x => x.USER_NAME).ToList();
            foreach (var obj in lstUser)
            {
                string titleName = obj.Title?.NAME ?? "Chưa có thông tin chức danh";

                lstData.Add(new Data { Value = obj.USER_NAME, Text = obj.USER_NAME + " - " + obj.FULL_NAME + " - " + titleName });
            }
            return new SelectList(lstData, "Value", "Text");
        }

        public static SelectList GetAllUserFecon(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstUser = UnitOfWork.Repository<UserRepo>().Queryable().Where(x => x.USER_TYPE == UserType.NB).OrderBy(x => x.USER_NAME).ToList();
            foreach (var obj in lstUser)
            {
                string titleName = obj.Title?.NAME ?? "Chưa có thông tin chức danh";
                lstData.Add(new Data { Value = obj.USER_NAME, Text = obj.USER_NAME + " - " + obj.FULL_NAME + " - " + titleName });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetAllForm(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstForm = UnitOfWork.Repository<FormRepo>().GetAll();
            foreach (var obj in lstForm)
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.CODE + " - " + obj.NAME });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetAllRight(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<RightRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.C_ORDER))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.CODE + " - " + obj.NAME });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetAllConnectionFile(string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            var lstAll = UnitOfWork.Repository<ConnectionRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.NAME))
            {
                lstData.Add(new Data { Value = obj.PKID, Text = obj.NAME });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetAllOrganize(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<OrganizeRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.C_ORDER))
            {
                lstData.Add(new Data { Value = obj.PKID, Text = obj.NAME });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetProjectOfUser(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }

            var lstProjectCode = UnitOfWork.GetSession().Query<T_AD_USER_PROJECT>().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).Select(x => x.PROJECT_CODE).ToList();
            var lstProject = UnitOfWork.GetSession().Query<T_MD_PROJECT>().Where(x => lstProjectCode.Contains(x.CODE));
            foreach (var obj in lstProject.OrderBy(x => x.NAME))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = $"[{obj.SAP_CODE}] {obj.NAME}" });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetAllProject(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<ProjectRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.NAME))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.NAME });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetAllPackage(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.GetSession().Query<T_MD_PACKAGE>().ToList();
            foreach (var obj in lstAll.OrderBy(x => x.TEXT))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }


        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetAllContractor(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<ContractorRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.TEN_NGAN))
            {
                lstData.Add(new Data { Value = obj.PKID, Text = obj.TEN_NGAN });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        public static SelectList GetMaterialType(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<MaterialTypeRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.CODE))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        } 

        public static SelectList GetBidType(string lang = "vi", bool isAddBlank = false, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<BidTypeRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.CODE))
            {
                if (lang == "vi")
                {
                    lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
                }
                else
                {
                    lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT_EN });
                }
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        public static SelectList GetBidSpecType()
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            lstData.Add(new Data { Value = "NUMBER", Text = "Nhập giá trị số" });
            lstData.Add(new Data { Value = "SELECT", Text = "Danh sách chọn" });
            lstData.Add(new Data { Value = "TEXT", Text = "Nhập chữ" });
            lstData.Add(new Data { Value = "GROUP", Text = "Nhóm tiêu chí" });
            return new SelectList(lstData, "Value", "Text");
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetCompany(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstOrgId = UnitOfWork.Repository<UserOrgRepo>().Queryable().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME).Select(x => x.ORGANIZE_ID).ToList();
            lstOrgId.Add(ProfileUtilities.User.ORGANIZE_ID);
            var lstAll = UnitOfWork.Repository<OrganizeRepo>().GetAll();
            foreach (var obj in lstAll.Where(x => x.TYPE == "CP").OrderBy(x => x.C_ORDER))
            {
                lstData.Add(new Data { Value = obj.PKID, Text = obj.NAME });
            }

            //if (ProfileUtilities.User != null && !string.IsNullOrEmpty(ProfileUtilities.User.USER_NAME))
            //{
            //    var objOrgUser = UnitOfWork.Repository<OrganizeRepo>()
            //                    .Queryable()
            //                    .Where(x => x.PKID == ProfileUtilities.User.ORGANIZE_ID)
            //                    .FirstOrDefault();

            //    var parentOrgs = objOrgUser == null
            //                    ? new List<T_AD_ORGANIZE>()
            //                    : UnitOfWork.Repository<OrganizeRepo>()
            //                        .Queryable()
            //                        .Where(x => x.PKID == objOrgUser.PARENT)
            //                        .ToList();

            //    foreach (var obj in parentOrgs)
            //    {
            //        lstData.Add(new Data { Value = obj.PKID, Text = obj.NAME });
            //    }


            //}

            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetCompanyCode(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<OrganizeRepo>().GetAll();
            foreach (var obj in lstAll.Where(x => x.TYPE == "CP").OrderBy(x => x.C_ORDER))
            {
                lstData.Add(new Data { Value = obj.COMPANY_CODE, Text = obj.NAME });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetOrganizeTypeCP(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<OrganizeRepo>().GetAll();
            foreach (var obj in lstAll.Where(x => x.TYPE == "CP").OrderBy(x => x.C_ORDER))
            {
                lstData.Add(new Data { Value = obj.PKID, Text = obj.NAME });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetCountry(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<CountryRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.CODE))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        /// <summary>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetMdRating(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<MdRatingRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.RATING))
            {
                lstData.Add(new Data { Value = obj.RATING, Text = obj.RATING });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        public static SelectList GetActiveArea(string lang, bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<ActiveAreaRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.CREATE_DATE))
            {
                if (lang == "vi")
                {
                    lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
                }
                else
                {
                    lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT_EN });
                }
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        public static SelectList GetUnit(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<UnitRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.CODE))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        public static SelectList GetScopeType(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<ScopeTypeRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.CODE))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        public static SelectList GetScope(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<ScopeRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.TEXT))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = $"[{obj.CODE}] {obj.TEXT}" });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        ///// <summary>
        ///// </summary>
        ///// <param name="selected"></param>
        ///// <returns></returns>
        //public static SelectList GetUnit(bool isAddBlank = true, string selected = "")
        //{
        //    IUnitOfWork UnitOfWork = new NHUnitOfWork();
        //    var lstData = new List<Data>();
        //    if (isAddBlank)
        //    {
        //        lstData.Add(new Data() { Value = "", Text = " - " });
        //    }
        //    var lstAll = UnitOfWork.Repository<UnitRepo>().GetListActive();
        //    foreach (var obj in lstAll)
        //    {
        //        lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
        //    }
        //    return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        //}

        ///// <summary>
        ///// </summary>
        ///// <param name="selected"></param>
        ///// <returns></returns>
        //public static SelectList GetUnit(List<T_MD_UNIT> lstUnit)
        //{
        //    IUnitOfWork UnitOfWork = new NHUnitOfWork();
        //    var lstData = new List<Data>();
        //    foreach (var obj in lstUnit)
        //    {
        //        lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
        //    }
        //    return new SelectList(lstData, "Value", "Text");
        //}

        public static SelectList GetCurrency(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<CurrencyRepo>().GetAll();
            foreach (var obj in lstAll.OrderByDescending(x => x.CODE))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        // Get Rent Unit
        public static SelectList GetRentUnit(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<RentUnitRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.CODE))
            {
                lstData.Add(new Data { Value = obj.CODE, Text = obj.TEXT });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

        public static SelectList GetTitle(bool isAddBlank = true, string selected = "")
        {
            IUnitOfWork UnitOfWork = new NHUnitOfWork();
            var lstData = new List<Data>();
            if (isAddBlank)
            {
                lstData.Add(new Data() { Value = "", Text = " - " });
            }
            var lstAll = UnitOfWork.Repository<TitleRepo>().GetAll();
            foreach (var obj in lstAll.OrderBy(x => x.PKID))
            {
                lstData.Add(new Data { Value = obj.PKID.ToString(), Text = obj.NAME });
            }
            return new SelectList(lstData, "Value", "Text", new Data { Value = selected });
        }

    }
}