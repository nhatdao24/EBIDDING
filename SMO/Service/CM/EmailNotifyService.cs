using SMO.Core.Entities;
using SMO.Repository.Implement.CM;
using System;
using System.Linq;
using NHibernate.Linq;

namespace SMO.Service.CM
{
    public class EmailNotifyService : GenericService<T_CM_EMAIL, EmailRepo>
    {
        public EmailNotifyService() : base()
        {

        }

        public void Reset(string id)
        {
            this.Get(id);

            var newEmail = new T_CM_EMAIL()
            {
                PKID = Guid.NewGuid().ToString(),
                CONTENTS = this.ObjDetail.CONTENTS,
                SUBJECT = this.ObjDetail.SUBJECT,
                IS_SEND = false,
                EMAIL = this.ObjDetail.EMAIL,
                NUMBER_RETRY = 0,
                CREATE_BY = ProfileUtilities.User.USER_NAME
            };

            try
            {
                UnitOfWork.BeginTransaction();
                this.CurrentRepository.Create(newEmail);
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                this.State = false;
                this.Exception = ex;
            }
        }
    }
}
