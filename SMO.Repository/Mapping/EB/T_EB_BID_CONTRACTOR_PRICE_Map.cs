using NHibernate.Type;
using SMO.Core.Entities;

namespace SMO.Repository.Mapping.EB
{
    public class T_EB_BID_CONTRACTOR_PRICE_Map : BaseMapping<T_EB_BID_CONTRACTOR_PRICE>
    {
        public T_EB_BID_CONTRACTOR_PRICE_Map()
        {
            Table("T_EB_BID_CONTRACTOR_PRICE");
            Id(x => x.PKID);
            Map(x => x.BID_ID).Not.Update();
            Map(x => x.CONTRACTOR_ID).Not.Update();
            Map(x => x.FIRST_PRICE);
            Map(x => x.PRICE);
            Map(x => x.C_ORDER);
            Map(x => x.CURRENCY_CODE);
            Map(x => x.TYPE).Not.Update();
            Map(x => x.NOTE);
        }
    }
}
