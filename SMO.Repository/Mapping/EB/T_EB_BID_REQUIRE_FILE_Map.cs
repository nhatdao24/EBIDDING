using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_REQUIRE_FILE_Map : BaseMapping<T_EB_BID_REQUIRE_FILE>
    {
        public T_EB_BID_REQUIRE_FILE_Map()
        {
            Table("T_EB_BID_REQUIRE_FILE");
            Id(x => x.PKID);
            Map(x => x.HEADER_CODE);
            Map(x => x.FILE_ID);
            References(x => x.File)
                                .Column("FILE_ID")
                                .PropertyRef("REFERENCE_ID")
                                .Not.Insert()
                                .Not.Update()
                                .LazyLoad();
        }
    }
}
