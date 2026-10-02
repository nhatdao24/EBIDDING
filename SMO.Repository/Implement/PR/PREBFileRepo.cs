using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMO.Core.Entities.PR;
using SMO.Repository.Common;
using SMO.Repository.Interface.PR;

namespace SMO.Repository.Implement.PR
{
    public class PREBFileRepo : GenericRepository<T_PR_EB_FILE> , IPREBFileRepo
    {
        public PREBFileRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }
    }
}
