using SMO.Core.Entities.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMO.Repository.Mapping.MD
{
    public class T_MD_HHDV_Map:BaseMapping<T_MD_HHDV>
    {
        public T_MD_HHDV_Map()
        {
            Table("T_MD_HHDV");
            Id(x => x.CODE).GeneratedBy.Assigned();
            Map(x => x.NAME);
            Map(x => x.ACTIVE);
            
        }
    }
}
