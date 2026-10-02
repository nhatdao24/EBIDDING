using SMO.Core.Entities;
using SMO.Repository.Common;
using SMO.Repository.Interface.MD;
using System.Collections.Generic;
using System.Linq;

namespace SMO.Repository.Implement.MD
{
    public class ProcessRepo : GenericRepository<T_WF_PROCESS>, IProcessRepo
    {
        public ProcessRepo(NHUnitOfWork unitOfWork) : base(unitOfWork.Session)
        {

        }

        
    }
}
