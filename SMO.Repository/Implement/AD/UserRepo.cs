using NHibernate.Criterion;
using NHibernate.Transform;
using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.AD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.AD
{
    public class UserRepo : GenericRepository<T_AD_USER>, IUserRepo
    {
        public UserRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        //public override IList<T_AD_USER> Search(T_AD_USER objFilter, int pageSize, int pageIndex, out int total)
        //{
        //    var query = NHibernateSession.QueryOver<T_AD_USER>();

        //    if (!string.IsNullOrWhiteSpace(objFilter.USER_NAME))
        //    {
        //        query = query.Where(x => x.USER_NAME.IsLike(string.Format("%{0}%", objFilter.USER_NAME)) || x.FULL_NAME.IsLike(string.Format("%{0}%", objFilter.USER_NAME)));
        //    }

        //    query = query.Fetch(x => x.Organize).Eager;

        //    return base.Paging(query, pageSize, pageIndex, out total);
        //}

        public override IList<T_AD_USER> Search(T_AD_USER objFilter, int pageSize, int pageIndex, out int total)
        {
            var query = NHibernateSession.QueryOver<T_AD_USER>();
            var queryOrg = NHibernateSession.QueryOver<T_AD_ORGANIZE>();
            var queryOrgList = NHibernateSession.QueryOver<T_AD_ORGANIZE>();

            if (!string.IsNullOrWhiteSpace(objFilter.USER_NAME))
            {
                query = query.Where(x => x.USER_NAME.IsLike(string.Format("%{0}%", objFilter.USER_NAME)) || x.FULL_NAME.IsLike(string.Format("%{0}%", objFilter.USER_NAME)));
            }

            if (!string.IsNullOrWhiteSpace(objFilter.ORGANIZE_ID))
            {
                var org = queryOrg.Where(x => x.PKID == objFilter.ORGANIZE_ID).List().FirstOrDefault();
                if (org.TYPE == "CP")
                {
                    var lstOrgId = queryOrgList.Where(x => x.COMPANY_CODE == org.COMPANY_CODE).List().Select(x => x.PKID).ToList();
                    query = query.Where(x => x.ORGANIZE_ID.IsIn(lstOrgId));
                }
                else
                {
                    query = query.Where(x => x.ORGANIZE_ID == objFilter.ORGANIZE_ID);
                }
            }

            if (!string.IsNullOrWhiteSpace(objFilter.USER_TYPE))
            {
                query = query.Where(x => x.USER_TYPE == objFilter.USER_TYPE);
            }

            query = query.Fetch(x => x.Organize).Eager;

            return base.Paging(query, pageSize, pageIndex, out total);
        }

        public override T_AD_USER Get(object id, dynamic param = null)
        {
            var query = NHibernateSession.QueryOver<T_AD_USER>();

            query = query.Where(x => x.USER_NAME == id);

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListUserUserGroup"))
            {
                query = query.Fetch(x => x.ListUserUserGroup).Eager
                        .Fetch(x => x.ListUserUserGroup.First().UserGroup).Eager
                        .Fetch(x => x.ListUserUserGroup.First().UserGroup.ListUserGroupRole).Eager
                        .Fetch(x => x.ListUserUserGroup.First().UserGroup.ListUserGroupRole.First().Role).Eager;
            }

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListUserRight"))
            {
                query = query.Fetch(x => x.ListUserRight).Eager;
            }

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListUserRole"))
            {
                query = query.Fetch(x => x.ListUserRole).Eager
                    .Fetch(x => x.ListUserRole.First().Role).Eager;
            }

            //if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListUserVendor"))
            //{
            //    query = query.Fetch(x => x.ListUserVendor).Eager
            //                .Fetch(x => x.ListUserVendor.First().Vendor).Eager;
            //}

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_ListUserOrg"))
            {
                query = query.Fetch(x => x.ListUserOrg).Eager
                            .Fetch(x => x.ListUserOrg.First().Organize).Eager;
            }

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_Organize"))
            {
                query = query.Fetch(x => x.Organize).Eager;
            }

            if (UtilsRepo.IsPropertyExist(param, "IsFetch_Contractor"))
            {
                query = query.Fetch(x => x.Contractor).Eager;
            }

            query = query.TransformUsing(new DistinctRootEntityResultTransformer());
            return query.List().FirstOrDefault();
        }
    }
}
