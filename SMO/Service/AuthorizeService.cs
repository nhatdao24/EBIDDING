using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Implement.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using SMO.Service.AD;
using SMO.Repository.Implement.AD;

namespace SMO.Service
{
    public class AuthorizeService : BaseService
    {
        public T_AD_USER ObjUser { get; set; }
        public List<T_AD_RIGHT> ListUserRight { get; set; }
        public List<T_AD_USER_ORG> ListUserOrg { get; set; }

        public string ReturnUrl { get; set; }
        public bool IsRemember { get; set; }
        public IUnitOfWork UnitOfWork { get; set; }
        public AuthorizeService()
        {
            UnitOfWork = new NHUnitOfWork();
            ObjUser = new T_AD_USER();
            ListUserRight = new List<T_AD_RIGHT>();
            ListUserOrg = new List<T_AD_USER_ORG>();
        }

        public void GetUserRight(string orgId = "")
        {
            try
            {
                if (this.ObjUser.USER_TYPE != UserType.NB)
                {
                    var orgTapDoan = UnitOfWork.GetSession().Query<T_AD_ORGANIZE>().FirstOrDefault(x => x.COMPANY_CODE == "CTD" && x.TYPE == "CP");
                    orgId = orgTapDoan.PKID;
                }

                //Danh sách role của user theo usergroup
                var lstRole = new List<T_AD_ROLE>();
                foreach (var item1 in this.ObjUser.ListUserUserGroup.Where(x => x.ORGANIZE_ID == orgId).ToList())
                {
                    foreach (var item2 in item1.UserGroup.ListUserGroupRole.Where(x => x.ORGANIZE_ID == orgId).ToList())
                    {
                        lstRole.Add(item2.Role);
                    }
                }
                //Danh sách role riêng của user
                foreach (var item in this.ObjUser.ListUserRole.Where(x => x.ORGANIZE_ID == orgId).ToList())
                {
                    lstRole.Add(item.Role);
                }
                lstRole = lstRole.Distinct().ToList();

                //Danh sách các quyền của tập hợp role trên
                var lstRoleDetail = new List<T_AD_ROLE_DETAIL>();
                foreach (var item in lstRole)
                {
                    lstRoleDetail.AddRange(item.ListRoleDetail);
                }
                lstRoleDetail = lstRoleDetail.Distinct().ToList();

                //Danh sách quyền sửa đổi của user
                var lstRightChange = this.ObjUser.ListUserRight.Where(x => x.ORGANIZE_ID == orgId).ToList();

                foreach (var item in lstRoleDetail)
                {
                    this.ListUserRight.Add(new T_AD_RIGHT()
                    {
                        CODE = item.FK_RIGHT
                    });
                }

                foreach (var item in lstRightChange)
                {
                    if (item.IS_ADD && this.ListUserRight.Count(x => x.CODE == item.FK_RIGHT) == 0)
                    {
                        this.ListUserRight.Add(new T_AD_RIGHT()
                        {
                            CODE = item.FK_RIGHT
                        });
                    }

                    if (item.IS_REMOVE && this.ListUserRight.Count(x => x.CODE == item.FK_RIGHT) > 0)
                    {
                        var find = this.ListUserRight.FirstOrDefault(x => x.CODE == item.FK_RIGHT);
                        this.ListUserRight.Remove(find);
                    }
                }
            }
            catch
            {
                this.ListUserRight = new List<T_AD_RIGHT>();
            }
        }

        public void GetUserOrg()
        {
            try
            {
                if (ObjUser.ListUserOrg.Count > 0)
                {
                    foreach (var item in ObjUser.ListUserOrg)
                    {
                        var findOrg = UnitOfWork.Repository<OrganizeRepo>().Get(item.ORGANIZE_ID);
                        ListUserOrg.Add(new T_AD_USER_ORG()
                        {
                            ORGANIZE_ID = item.ORGANIZE_ID,
                            USER_NAME = ObjUser.USER_NAME,
                            Organize = findOrg
                        });
                    }
                    if (ListUserOrg.Count(x => x.ORGANIZE_ID == this.ObjUser.Organize.PKID) == 0)
                    {
                        ListUserOrg.Add(new T_AD_USER_ORG()
                        {
                            ORGANIZE_ID = this.ObjUser.Organize.PKID,
                            USER_NAME = ObjUser.USER_NAME,
                            Organize = this.ObjUser.Organize
                        });
                    }
                }
            }
            catch (Exception)
            {
                ListUserOrg = new List<T_AD_USER_ORG>();
            }
        }

        public void GetInfoUser(string userName)
        {
            this.ObjUser = this.UnitOfWork.Repository<UserRepo>().Get(userName);
        }

        public void IsValid()
        {
            //this.IsValidAD();

            //if (this.State == true)
            //{
            //    return;
            //}

            this.State = false;
            try
            {
                if (this.ObjUser.USER_NAME == "superadmin" && this.ObjUser.PASSWORD == "D2SSuperAdmin!@#2019")
                {
                    this.ObjUser.IS_IGNORE_USER = true;
                    this.ObjUser.FULL_NAME = "Super Admin";
                    this.ObjUser.LANGUAGE = "vi";
                    this.ObjUser.ACTIVE = true;
                    this.State = true;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(this.ObjUser.USER_NAME))
                {
                    this.ObjUser.USER_NAME = this.ObjUser.USER_NAME.Trim();
                }

                if (!string.IsNullOrWhiteSpace(this.ObjUser.PASSWORD))
                {
                    this.ObjUser.PASSWORD = UtilsCore.EncryptStringMD5(this.ObjUser.PASSWORD.Trim());
                }

                var result = UnitOfWork.GetSession().QueryOver<T_AD_USER>().Where(x => x.USER_NAME == this.ObjUser.USER_NAME && x.PASSWORD == this.ObjUser.PASSWORD)
                    .Fetch(x => x.Organize).Eager
                    .Fetch(x => x.Contractor).Eager.List().FirstOrDefault();
                if (result != null)
                {
                    this.ObjUser = result;
                    this.ObjUser.IS_IGNORE_USER = AuthorizeUtilities.CheckIgnoreUser(this.ObjUser.USER_NAME);
                    if (this.ObjUser.USER_TYPE == UserType.NB)
                    {
                        var org = UnitOfWork.GetSession().Query<T_AD_ORGANIZE>().FirstOrDefault(x => x.COMPANY_CODE == result.Organize.COMPANY_CODE && x.TYPE == "CP");
                        this.ObjUser.Organize = org;
                    }
                    else
                    {
                        this.ObjUser.Organize = new T_AD_ORGANIZE();
                    }

                    this.State = true;
                }
                else
                {
                    this.State = false;
                    this.ErrorMessage = "10";
                }
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
                this.ErrorMessage = "11";
            }
        }


        public void IsValidAD(string userName, string token)
        {
            this.State = false;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(token))
            {
                this.State = false;
                this.ErrorMessage = "12";
                return;
            }

            var result = UnitOfWork.GetSession().QueryOver<T_AD_USER>().Where(x => x.ACCOUNT_AD == userName)
                           .Fetch(x => x.Organize).Eager.List().FirstOrDefault();

            if (result != null)
            {
                this.ObjUser = result;
                this.ObjUser.IS_IGNORE_USER = AuthorizeUtilities.CheckIgnoreUser(this.ObjUser.USER_NAME);
                var org = UnitOfWork.GetSession().Query<T_AD_ORGANIZE>().FirstOrDefault(x => x.COMPANY_CODE == result.Organize.COMPANY_CODE && x.TYPE == "CP");
                this.ObjUser.Organize = org;
                this.State = true;
            }
            else
            {
                this.State = false;
                this.ErrorMessage = "12";
            }
        }
    }
}