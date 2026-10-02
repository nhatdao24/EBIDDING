using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_REQUIRE_USER_Map : BaseMapping<T_EB_BID_REQUIRE_USER>
    {
        public T_EB_BID_REQUIRE_USER_Map()
        {
            Table("T_EB_BID_REQUIRE_USER");
            Id(x => x.PKID);
            Map(x => x.HEADER_CODE);
            Map(x => x.USER_NAME);
            Map(x => x.C_LEVEL);
            Map(x => x.IS_APPROVED).Not.Nullable().CustomType<YesNoType>();
            Map(x => x.DATE_APPROVE);
            Map(x => x.ROLE);
            References(x => x.User).Column("USER_NAME").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
