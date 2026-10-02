using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Type;
using SMO.Core.Entities.AD;

namespace SMO.Repository.Mapping.AD
{
    public class T_AD_COUNT_VISIT_Map : BaseMapping<T_AD_COUNT_VISIT>
    {
        public T_AD_COUNT_VISIT_Map()
        {
            Table("T_AD_COUNT_VISIT");
            Id(x => x.PKID);
            Map(x => x.ACCESS_TYPE);
            Map(x => x.VIEW_TYPE);
            Map(x => x.IS_MOBILE).Not.Nullable().CustomType<YesNoType>();
        }
    }
}
