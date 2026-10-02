using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMO.Core.Entities.PR;

namespace SMO.Repository.Mapping.PR
{
    public class T_PR_EB_HISTORY_Map :BaseMapping<T_PR_EB_HISTORY>
    {
        public T_PR_EB_HISTORY_Map()
        {
            Table("T_PR_EB_HISTORY");
            Id(x => x.PKID);
            Map(x => x.HEADER_CODE);
            Map(x => x.ACTION);
            Map(x => x.ACTION_DATE);
            Map(x => x.ACTION_USER);
            Map(x => x.NOTES);
            References(x => x.ActionUser).Column("ACTION_USER").Not.Insert().Not.Update().LazyLoad(); 
        }
    }
}
