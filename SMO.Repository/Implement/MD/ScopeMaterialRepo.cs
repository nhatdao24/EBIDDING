using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMO.Core.Entities.MD;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;

namespace SMO.Repository.Implement.MD
{
    public class ScopeMaterialRepo : GenericRepository<T_MD_SCOPE_MATERIAL>, IScopeMaterialRepo
    {
        public ScopeMaterialRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

    }
}
