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
    public class RightProjectUserRepo : GenericRepository<T_AD_RIGHT_PROJECT_USER>, IRightProjectUserRepo
    {
        public RightProjectUserRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
