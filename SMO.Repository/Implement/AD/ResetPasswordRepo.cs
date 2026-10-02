using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.AD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.AD
{
    public class ResetPasswordRepo : GenericRepository<T_AD_REQUEST_RESET_PASS>, IResetPasswordRepo
    {
        public ResetPasswordRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
