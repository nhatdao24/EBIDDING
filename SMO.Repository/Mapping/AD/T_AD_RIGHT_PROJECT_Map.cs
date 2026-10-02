using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMO.Core.Entities.AD;

namespace SMO.Repository.Mapping.AD
{
    public class T_AD_RIGHT_PROJECT_Map : BaseMapping<T_AD_RIGHT_PROJECT>
    {
       public T_AD_RIGHT_PROJECT_Map()
        {
            Table("T_AD_RIGHT_PROJECT");
            Id(x => x.CODE);
            Map(x => x.NAME);
            Map(x => x.PARENT);
        }
    }
}
