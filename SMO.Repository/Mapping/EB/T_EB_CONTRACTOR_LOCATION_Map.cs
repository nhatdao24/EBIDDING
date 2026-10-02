using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_CONTRACTOR_LOCATION_Map : BaseMapping<T_EB_CONTRACTOR_LOCATION>
    {
        public T_EB_CONTRACTOR_LOCATION_Map()
        {
            Table("T_EB_CONTRACTOR_LOCATION");
            Id(x => x.PKID);
            Map(x => x.CONTRACTOR_ID);
            Map(x => x.COUNTRY_CODE);
            Map(x => x.ADDRESS);
            Map(x => x.EMAIL);
            Map(x => x.PHONE);
            References(x => x.Country).Column("COUNTRY_CODE").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
