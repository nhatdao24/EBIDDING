using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_CONTRACTOR_MARKET_Map : BaseMapping<T_EB_CONTRACTOR_MARKET>
    {
        public T_EB_CONTRACTOR_MARKET_Map()
        {
            Table("T_EB_CONTRACTOR_MARKET");
            Id(x => x.PKID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.COUNTRY_CODE);
            References(x => x.Country).Column("COUNTRY_CODE").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
