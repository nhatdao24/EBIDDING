using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMO.Core.Entities.AD;

namespace SMO.Repository.Mapping.AD
{
    public class T_AD_RIGHT_PROJECT_USER_Map : BaseMapping<T_AD_RIGHT_PROJECT_USER>
    {
        public T_AD_RIGHT_PROJECT_USER_Map()
        {
            Table("T_AD_RIGHT_PROJECT_USER");
            Id(x => x.PKID);
            Map(x => x.PROJECT_CODE).Not.Nullable();
            Map(x => x.USER_NAME).Not.Nullable();
            Map(x => x.RIGHT_CODE).Not.Nullable();
        }
    }
}
