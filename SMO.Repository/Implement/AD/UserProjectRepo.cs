using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMO.Core.Entities.AD;
using SMO.Repository.Common;
using SMO.Repository.Interface.AD;

namespace SMO.Repository.Implement.AD
{
    public class UserProjectRepo : GenericRepository<T_AD_USER_PROJECT> , IUserProjectRepo
    {
        public UserProjectRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
