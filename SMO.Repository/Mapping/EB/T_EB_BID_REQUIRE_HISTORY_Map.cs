using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_REQUIRE_HISTORY_Map : BaseMapping<T_EB_BID_REQUIRE_HISTORY>
    {
        public T_EB_BID_REQUIRE_HISTORY_Map()
        {
            Table("T_EB_BID_REQUIRE_HISTORY");
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
