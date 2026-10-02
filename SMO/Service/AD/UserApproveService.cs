using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Service.AD
{
    public class UserApproveService : GenericService<T_AD_USER_APPROVE, UserApproveRepo>
    {
        public void Update(string userApprove, string modul)
        {
            try
            {
                var find = this.CurrentRepository.Queryable().FirstOrDefault(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.MODUL == modul);

                UnitOfWork.BeginTransaction();
                if (find == null)
                {
                    this.CurrentRepository.Create(new T_AD_USER_APPROVE()
                    {
                        PKID = Guid.NewGuid().ToString(),
                        USER_NAME = ProfileUtilities.User.USER_NAME,
                        MODUL = modul,
                        USER_APPROVE = userApprove
                    });
                }
                else
                {
                    find.USER_APPROVE = userApprove;
                    this.CurrentRepository.Update(find);
                }
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
            }
        }

        public string GetUserApprove(string modul)
        {
            var find = this.CurrentRepository.Queryable().FirstOrDefault(x => x.USER_NAME == ProfileUtilities.User.USER_NAME && x.MODUL == modul);
            if (find != null)
            {
                return find.USER_APPROVE; 
            }
            return "";
        }
    }
}
