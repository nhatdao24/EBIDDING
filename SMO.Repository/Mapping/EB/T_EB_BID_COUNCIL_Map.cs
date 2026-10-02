using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_COUNCIL_Map : BaseMapping<T_EB_BID_COUNCIL>
    {
        public T_EB_BID_COUNCIL_Map()
        {
            Table("T_EB_BID_COUNCIL");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.USER_NAME);
            Map(x => x.VAI_TRO);
            Map(x => x.IS_QUYET_DINH).CustomType<YesNoType>();
            References(x => x.User).Column("USER_NAME").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
