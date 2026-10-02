using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_HHDV_Map : BaseMapping<T_EB_BID_HHDV>
    {
        public T_EB_BID_HHDV_Map()
        {
            Table("T_EB_BID_HHDV");
            Id(x => x.PKID);
            Map(x => x.BID_ID);
            Map(x => x.CODE);
            Map(x => x.TEXT);
            Map(x => x.DESCRIPTION);
            Map(x => x.QUANTITY);
            Map(x => x.UNIT);
            Map(x => x.PRICE_VTB);
            References(x => x.Bid).Column("BID_ID").Not.Insert().Not.Update().LazyLoad();
        }
    }
}
