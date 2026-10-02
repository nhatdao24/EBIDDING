using SMO.Core.Entities;
using SMO.Core.Entities.AD;
using SMO.Repository.Common;
using SMO.Repository.Implement.AD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMO
{
    public static class AuthorizeUtilities
    {
        internal static List<string> IGNORE_USERS { get; set; }
        internal static IUnitOfWork UnitOfWork = new NHUnitOfWork();

        /// <summary>
        /// Kiểm tra xem user current có nắm trong list user full quyền hay không
        /// </summary>
        /// <returns></returns>
        public static bool CheckIgnoreUser(string userName)
        {
            if (IGNORE_USERS != null)
            {
                return IGNORE_USERS.Contains(userName);
            }
            return false;
        }

        public static bool CheckUserRight(string right)
        {
            if (ProfileUtilities.User.IS_IGNORE_USER)
            {
                return true;
            }
            var lstRight = right.Split(',');
            foreach (var item in lstRight)
            {
                if (ProfileUtilities.UserRight.Select(x => x.CODE).Contains(item))
                {
                    return true;
                }
            }
            return false;
        }


        public static bool CheckUser(string right)
        {
            var lstRight = UnitOfWork.Repository<RightProjectUserRepo>().Queryable().Where(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.RIGHT_CODE == right).ToList();
            return lstRight.Count() > 0 ? true: false;
        }

        //public static Tuple<bool, string> CheckUserRightProject(string right, string projectCode)
        //{
        //    string rightSpecial = "";
        //    //if (ProfileUtilities.User.IS_IGNORE_USER)
        //    //{
        //    //    return new Tuple<bool, string>(true,"");
        //    //}
        //    //TODO: Kiểm tra user có quyền đặc biệt R00
        //    //if (ProfileUtilities.UserRight.Select(x => x.CODE).Contains(rightSpecial))
        //    //{
        //    //    return new Tuple<bool, string>(true, "");
        //    //}
        //    // Lấy danh sách quyền của user theo dự án
        //    var listRightOfUser = UnitOfWork.GetSession().Query<T_AD_RIGHT_PROJECT_USER>().Where(x => x.PROJECT_CODE == projectCode && x.USER_NAME == ProfileUtilities.User.USER_NAME).ToList();
        //    var objRight = UnitOfWork.GetSession().Query<T_AD_RIGHT_PROJECT>().FirstOrDefault(x => x.CODE == right);
        //    var objProject = UnitOfWork.GetSession().Query<T_MD_PROJECT>().FirstOrDefault(x => x.CODE == projectCode);

        //    if (objRight == null)
        //    {
        //        return new Tuple<bool, string>(false, $"Phân quyền lỗi: Quyền [{right}] không tồn tại");
        //    }

        //    if (objProject == null)
        //    {
        //        return new Tuple<bool, string>(false, $"Phân quyền lỗi: Dự án [{projectCode}] không tồn tại");
        //    }

        //    // Kiểm tra right có nằm trong danh sách quyền hay không
        //    if (listRightOfUser.Count > 0 && listRightOfUser.Select(x => x.RIGHT_CODE).Contains(right))
        //    {
        //        return new Tuple<bool, string>(true, "");
        //    }
        //    else
        //    {
        //        return new Tuple<bool, string>(false, $"Bạn không có quyền [{right} - {objRight.NAME}] trong dự án [{objProject.SAP_CODE} - {objProject.NAME}]");
        //    }

        //    return new Tuple<bool, string>(false,"");
        //}
    }
}