using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_REQUIRE_PR_Map : BaseMapping<T_EB_BID_REQUIRE_PR>
    {
        public T_EB_BID_REQUIRE_PR_Map()
        {
            Table("T_EB_BID_REQUIRE_PR");
            Id(x => x.PKID);
            Map(x => x.HEADER_CODE);
            Map(x => x.TYPE);
            Map(x => x.PR_CODE);
            Map(x => x.PR_NAME);
        }
    }
}
